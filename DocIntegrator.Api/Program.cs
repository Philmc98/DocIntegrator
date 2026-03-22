using System.Text;
using DocIntegrator.Application.Auth;
using DocIntegrator.Application.Interfaces;
using DocIntegrator.Application.Behaviors;
using DocIntegrator.Application.Documents.Queries.GetAllDocuments;
using DocIntegrator.Infrastructure.Auth;
using DocIntegrator.Infrastructure.Repositories;
using DocIntegrator.Infrastructure.Data;
using DocIntegrator.Infrastructure.Caching;
using DocIntegrator.Infrastructure.Events;
using DocIntegrator.Infrastructure.Analytics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using FluentValidation;
using DocIntegrator.Application.Documents.Queries;
using DocIntegrator.Application.Documents.Validators;
using DocIntegrator.Api.Middleware;
using DocIntegrator.Api.Swagger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// Создаём строитель приложения, инициализируем DI-контейнер
var builder = WebApplication.CreateBuilder(args);

// Регистрируем контроллеры
builder.Services.AddControllers();

// gRPC для межсервисного взаимодействия (микросервисный сценарий).
builder.Services.AddGrpc();

// Регистрируем валидаторы из сборки Application для DI
builder.Services.AddValidatorsFromAssemblyContaining<CreateDocumentDtoValidator>();

// Swagger (схема v1, XML-комментарии, отключается в prod через конфиг)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = builder.Configuration["Swagger:Title"] ?? "DocIntegrator API",
        Version = builder.Configuration["Swagger:Version"] ?? "v1",
        Description = builder.Configuration["Swagger:Description"] ?? "REST API для работы с документами."
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    options.OperationFilter<ErrorResponseOperationFilter>();
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Введите JWT-токен. Получить токен: POST /api/auth/login → скопируйте значение поля \"token\".",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// MediatR (CQRS)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetAllDocumentsQuery).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// EF Core + PostgreSQL / MS SQL.
// В конфиге можно выбрать провайдера БД:
// "Database:Provider": "PostgreSql" (по умолчанию) или "SqlServer".
// Таким образом демонстрируется умение работать и с PostgreSQL, и с MS SQL.
var dbProvider = builder.Configuration["Database:Provider"] ?? "PostgreSql";

builder.Services.AddDbContext<DocIntegratorDbContext>(options =>
{
    if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        // MS SQL (например, для развёртывания в on-premise инфраструктуре).
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionSqlServer"));
    }
    else if (dbProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
    {
        // InMemory — используется только в интеграционных тестах.
        options.UseInMemoryDatabase("DocIntegratorTestDb");
    }
    else
    {
        // PostgreSQL (по умолчанию для docker-compose).
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    }

    if (builder.Environment.IsDevelopment())
        options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
});

// Репозиторий документов (PostgreSQL / MS SQL / InMemory)
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

// Event store всегда использует EF Core — работает и в InMemory, и с реальной БД.
builder.Services.AddScoped<IDocumentEventStore, DocumentEventStore>();

// Kafka + ClickHouse — только если БД настоящая (PostgreSql / SqlServer).
// В InMemory-режиме используем локальные заглушки без внешних соединений.
if (dbProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IDocumentAnalyticsWriter, NoOpDocumentAnalyticsWriter>();
    builder.Services.AddScoped<IDocumentIntegrationService, LocalDocumentIntegrationService>();
}
else
{
    builder.Services.AddScoped<IDocumentAnalyticsWriter, ClickHouseDocumentAnalyticsWriter>();
    builder.Services.AddScoped<IDocumentIntegrationService, KafkaDocumentIntegrationService>();
}

// Статистика документов: Dapper (PostgreSql/SqlServer) или LINQ (InMemory).
if (dbProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IDocumentStatsRepository, InMemoryDocumentStatsRepository>();
}
else
{
    builder.Services.AddScoped<IDocumentStatsRepository, DocumentStatsRepository>();
}

// Redis-кеш для документов.
// В InMemory-режиме (для локальной разработки без Docker) используем заглушку.
if (dbProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IDocumentCache, NoOpDocumentCache>();
}
else
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    {
        var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
        // abortConnect=false: не бросаем исключение при запуске, если Redis недоступен.
        // В продакшене Redis должен быть доступен; здесь — для удобства локальной разработки.
        var options = ConfigurationOptions.Parse(redisConnection);
        options.AbortOnConnectFail = false;
        return ConnectionMultiplexer.Connect(options);
    });
    builder.Services.AddScoped<IDocumentCache, RedisDocumentCache>();
}

// JWT и аутентификация
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddScoped<IAuthService, AuthService>();

// В продакшене Jwt:Key задаётся через переменную окружения (Jwt__Key).
var jwtKey = builder.Configuration["Jwt:Key"] ?? "demo-key-min-32-chars-for-hs256!!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Логирование
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// CORS (пока открытый — для локальной разработки)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Health Checks (для мониторинга и оркестраторов)
builder.Services.AddHealthChecks();

// OpenTelemetry + Prometheus: технические метрики по HTTP/gRPC.
// Это демонстрация сбора метрик и мониторинга приложения.
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
        resource.AddService("DocIntegrator.Api"))
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddHttpClientInstrumentation();
        metrics.AddRuntimeInstrumentation();
        metrics.AddPrometheusExporter();
    });

var app = builder.Build();

// Проверка конфигурации при старте: строка подключения (задаётся через env ConnectionStrings__DefaultConnection)
if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection")))
{
    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    var startupLogger = loggerFactory.CreateLogger("Startup");
    startupLogger.LogWarning("ConnectionStrings:DefaultConnection не задана. Задайте в appsettings или переменной окружения ConnectionStrings__DefaultConnection.");
}

// Swagger включается по конфигу (в Production значение false)
var swaggerEnabled = builder.Configuration.GetValue<bool>("Swagger:Enable");
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DocIntegrator API v1");
        c.DocumentTitle = "DocIntegrator API";
        c.DefaultModelsExpandDepth(-1);
    });
    // Редирект с корня на Swagger, чтобы не получать 404 в браузере
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<DocIntegrator.Api.Services.DocumentGrpcService>();
app.MapHealthChecks("/health");
// Точка /metrics будет отдаваться Prometheus-экспортёром OpenTelemetry.
app.MapPrometheusScrapingEndpoint("/metrics");

app.Run();
