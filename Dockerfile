# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Sale_Saas.API/Sale_Saas.API.csproj", "Sale_Saas.API/"]
COPY ["Sale_Saas.Application/Sale_Saas.Application.csproj", "Sale_Saas.Application/"]
COPY ["Sale_Saas.Domain/Sale_Saas.Domain.csproj", "Sale_Saas.Domain/"]
COPY ["Sale_Saas.Infrastructure/Sale_Saas.Infrastructure.csproj", "Sale_Saas.Infrastructure/"]
COPY ["Sale_Saas.Utilities/Sale_Saas.Utilities.csproj", "Sale_Saas.Utilities/"]

RUN dotnet restore "Sale_Saas.API/Sale_Saas.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Sale_Saas.API"
RUN dotnet build "Sale_Saas.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Sale_Saas.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published app
COPY --from=publish /app/publish .

# Create directories for logs and templates
RUN mkdir -p /app/Logs /app/Template && \
    chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Sale_Saas.API.dll"]

