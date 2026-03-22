using DocIntegrator.Application.Interfaces;
using DocIntegrator.Infrastructure.Caching;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace DocIntegrator.Tests.Integration;

/// <summary>
/// Фабрика тестового приложения: переключает БД на InMemory через конфигурацию,
/// отключает Redis/Kafka и добавляет тестовую аутентификацию.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<DocIntegrator.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Переключаем провайдер БД на InMemory через конфигурацию — тогда Program.cs
        // зарегистрирует только InMemory провайдер, и конфликта провайдеров не возникнет.
        builder.UseSetting("Database:Provider", "InMemory");

        builder.ConfigureTestServices(services =>
        {
            // --- Redis: убираем настоящее подключение ---
            var redisDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionMultiplexer));
            if (redisDescriptor != null)
                services.Remove(redisDescriptor);

            // --- IDocumentCache: заглушка вместо Redis ---
            var cacheDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDocumentCache));
            if (cacheDescriptor != null)
                services.Remove(cacheDescriptor);
            services.AddScoped<IDocumentCache, NoOpDocumentCache>();

            // --- Kafka/ClickHouse: тестовая заглушка ---
            var integrationDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDocumentIntegrationService));
            if (integrationDescriptor != null)
                services.Remove(integrationDescriptor);
            services.AddScoped<IDocumentIntegrationService, TestDocumentIntegrationService>();

            // --- Аутентификация: тестовая схема, которая всегда аутентифицирует ---
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }
}
