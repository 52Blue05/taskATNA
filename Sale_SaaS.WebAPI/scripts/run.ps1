$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
$ApiProj = Join-Path $Root "Sale_Saas.API\Sale_Saas.API.csproj"

Write-Host "==> Running API..."
dotnet run --project $ApiProj
