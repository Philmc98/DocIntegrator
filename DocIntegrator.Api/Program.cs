using DocIntegrator.Application.Interfaces;
using DocIntegrator.Infrastructure.Repositories;
using MediatR;
using DocIntegrator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FluentValidation.AspNetCore;
using DocIntegrator.Application.Documents.Queries;

var builder = WebApplication.CreateBuilder(args);

// Controllers + FluentValidation
builder.Services
    .AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CreateDocumentValidator>());

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetAllDocumentsQuery).Assembly));

// EF Core + PostgreSQL
builder.Services.AddDbContext<DocIntegratorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// –епозиторий (только PostgreSQL)
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

// √лобальный обработчик ошибок Ч до контроллеров
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
