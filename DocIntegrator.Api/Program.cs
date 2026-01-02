using DocIntegrator.Application.Interfaces;
using DocIntegrator.Infrastructure.Repositories;
using DocIntegrator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using DocIntegrator.Application.Documents.Queries;
using DocIntegrator.Application.Documents.Validators;
using DocIntegrator.Api.Middleware;

// здесь загружаются конфиги, настраивается DI-контейнер
var builder = WebApplication.CreateBuilder(args);

// регистрируем контроллеры
builder.Services.AddControllers();

// Автозапуск серверной валидации FluentValidation для моделей (DTO)
// Без этого валидаторы не будут исполняться автоматически
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Автоматически регистрируем все валидаторы из сборки Application
builder.Services.AddValidatorsFromAssemblyContaining<CreateDocumentDtoValidator>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetAllDocumentsQuery).Assembly));

// EF Core + PostgreSQL
builder.Services.AddDbContext<DocIntegratorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Репозиторий (только PostgreSQL)
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

// Логирование
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// CORS (если нужен фронт)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Глобальный обработчик ошибок — до контроллеров (и до Authorization, если он интерпретирует ошибки)
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
