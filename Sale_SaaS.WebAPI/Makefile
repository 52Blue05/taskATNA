SHELL := /bin/bash

# ===== Paths =====
API_PROJ := ./Sale_Saas.API/Sale_Saas.API.csproj
SLN      := ./Sale_Saas.sln

# ===== EF Context =====
TENANT_CONTEXT := Sale_Saas.Infrastructure.Data.TenantDbContext

# ===== Docker / DB config (dev default) =====
DB_HOST ?= localhost
DB_PORT ?= 5432
DB_NAME ?= postgres
DB_USER ?= postgres
DB_PASSWORD ?= 123456

# Connection string used ONLY for migrations (does not edit files)
CONNECTION_STRING ?= Host=$(DB_HOST);Port=$(DB_PORT);Database=$(DB_NAME);Username=$(DB_USER);Password=$(DB_PASSWORD);

# ===== Compose =====
COMPOSE ?= docker compose

.PHONY: help db-up db-down db-logs db-status restore build ef-check migrate setup run watch swagger clean

help:
	@echo "Targets:"
	@echo "  make setup        - Start Postgres (docker) + apply migrations (create tables)"
	@echo "  make db-up        - Start Postgres container"
	@echo "  make db-down      - Stop containers"
	@echo "  make db-logs      - Tail DB logs"
	@echo "  make migrate      - Apply EF migrations (TenantDbContext)"
	@echo "  make run          - Run API"
	@echo "  make watch        - Run API with dotnet watch"
	@echo "  make build        - Build solution"
	@echo "  make clean        - Clean solution"

db-up:
	@$(COMPOSE) up -d

db-down:
	@$(COMPOSE) down

db-logs:
	@$(COMPOSE) logs -f

db-status:
	@$(COMPOSE) ps

restore:
	@dotnet restore $(SLN)

build:
	@dotnet build $(SLN)

clean:
	@dotnet clean $(SLN)

ef-check:
	@dotnet ef --version >/dev/null 2>&1 || (echo "❌ dotnet-ef not found. Install it:" && echo "   dotnet tool install --global dotnet-ef --version 8.0.3" && exit 1)

migrate: ef-check
	@ASPNETCORE_ENVIRONMENT=Development \
	ConnectionStrings__Default="$(CONNECTION_STRING)" \
	dotnet ef database update --context $(TENANT_CONTEXT) --project $(API_PROJ)

setup: db-up migrate
	@echo "Setup done: Postgres is up + migrations applied."

run:
	@dotnet run --project $(API_PROJ)

watch:
	@dotnet watch --project $(API_PROJ) run


