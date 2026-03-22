# Сборка
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["DocIntegrator.Api/DocIntegrator.Api.csproj", "DocIntegrator.Api/"]
COPY ["DocIntegrator.Application/DocIntegrator.Application.csproj", "DocIntegrator.Application/"]
COPY ["DocIntegrator.Domain/DocIntegrator.Domain.csproj", "DocIntegrator.Domain/"]
COPY ["DocIntegrator.Infrastructure/DocIntegrator.Infrastructure.csproj", "DocIntegrator.Infrastructure/"]

RUN dotnet restore "DocIntegrator.Api/DocIntegrator.Api.csproj"
COPY . .
WORKDIR "/src/DocIntegrator.Api"
RUN dotnet build "DocIntegrator.Api.csproj" -c Release -o /app/build

# Публикация
FROM build AS publish
RUN dotnet publish "DocIntegrator.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Рантайм
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "DocIntegrator.Api.dll"]
