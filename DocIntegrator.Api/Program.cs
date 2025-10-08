using DocIntegrator.Application.Interfaces;
using DocIntegrator.Infrastructure.Repositories;
using DocIntegrator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using DocIntegrator.Application.Documents.Queries;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Автозапуск серверной валидации FluentValidation для моделей (DTO)
// Без этого валидаторы не будут исполняться автоматически
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Автоматически регистрируем все валидаторы из сборки Application
builder.Services.AddValidatorsFromAssemblyContaining<CreateDocumentDtoValidator>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetAllDocumentsQuery).Assembly));

// EF Core + PostgreSQL
builder.Services.AddDbContext<DocIntegratorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Репозиторий (только PostgreSQL)
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Глобальный обработчик ошибок — до контроллеров (и до Authorization, если он интерпретирует ошибки)
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
