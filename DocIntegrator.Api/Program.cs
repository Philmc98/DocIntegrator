using DocIntegrator.Application.Interfaces;
using DocIntegrator.Infrastructure.Repositories;
using DocIntegrator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using DocIntegrator.Application.Documents.Queries;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// ���������� ��������� ��������� FluentValidation ��� ������� (DTO)
// ��� ����� ���������� �� ����� ����������� �������������
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// ������������� ������������ ��� ���������� �� ������ Application
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

// ����������� (������ PostgreSQL)
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ���������� ���������� ������ � �� ������������ (� �� Authorization, ���� �� �������������� ������)
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
