# DocIntegrator

Backend REST API на **.NET 9 / C#**: управление документами. Clean Architecture, CQRS (MediatR), EF Core.

## Stack

- ASP.NET Core 9, EF Core 9, C# 12
- PostgreSQL / MS SQL Server
- CQRS (MediatR), FluentValidation, Event Sourcing
- Kafka, Redis, ClickHouse
- gRPC, JWT, OpenTelemetry + Prometheus
- Docker, Kubernetes (`k8s/`), GitHub Actions

## Структура

```
DocIntegrator.Api            — HTTP API, gRPC, middleware, Swagger
DocIntegrator.Application    — CQRS, валидация, интерфейсы
DocIntegrator.Domain         — доменные модели
DocIntegrator.Infrastructure — EF Core, Redis, Kafka, ClickHouse
DocIntegrator.Tests          — unit- и интеграционные тесты
```

## Быстрый старт

```bash
dotnet restore
dotnet run --project DocIntegrator.Api
```

Или через Docker:

```bash
docker-compose up -d
```

- Swagger: http://localhost:5260/swagger (локально) / http://localhost:8080/swagger (Docker)
- Health: `GET /health`
- Metrics: `GET /metrics`

Миграции:

```bash
dotnet ef database update --project DocIntegrator.Infrastructure --startup-project DocIntegrator.Api
```

## Тесты

```bash
dotnet test DocIntegrator.sln
```
