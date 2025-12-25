$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
$ApiProj = Join-Path $Root "Sale_Saas.API\Sale_Saas.API.csproj"
$TenantContext = "Sale_Saas.Infrastructure.Data.TenantDbContext"

Write-Host "==> Applying Tenant migrations..."
dotnet ef database update --context $TenantContext --project $ApiProj
