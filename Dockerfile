# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for better layer caching
COPY AssistIQ.slnx global.json ./
COPY src/AssistIQ.Domain/AssistIQ.Domain.csproj src/AssistIQ.Domain/
COPY src/AssistIQ.Application/AssistIQ.Application.csproj src/AssistIQ.Application/
COPY src/AssistIQ.Infrastructure/AssistIQ.Infrastructure.csproj src/AssistIQ.Infrastructure/
COPY src/AssistIQ.Api/AssistIQ.Api.csproj src/AssistIQ.Api/
RUN dotnet restore src/AssistIQ.Api/AssistIQ.Api.csproj

# Copy remaining source and publish
COPY . .
RUN dotnet publish src/AssistIQ.Api/AssistIQ.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Run as root to avoid Render Firecracker permissions issues with .NET 10 preview
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AssistIQ.Api.dll"]
