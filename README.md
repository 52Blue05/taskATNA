# Sale_SaaS
## Technologies
- ASP.NET Core 8
- Entity Framework Core 8
- jquery
- MediatR
- AutoMapper
- FluentValidation
- NUnit, FluentAssertions, Moq & Respawn
## Install Tools
- .NET Core SDK 8
- Git client
- Visual Studio 2022
- SQL Server 2022
# Getting Started
## How to configure and run
- Clone code
- Open solution sln in Visual Studio 2022
- Set startup project is Sale_Saas.API
- Open Tools --> Nuget Package Manager -->  Package Manager Console in Visual Studio
- Run Update-database -Context TenantDbContext and Enter.
- Run Update-database -Context ApplicationDbContext and Enter.
- After migrate database successful, set Startup Project is Sale_Saas.API
- Change database connection in appsettings.Development.json in Sale_Saas.API project.
- If add new migrate:
Add-Migration newtable -OutputDir Migrations/ApplicationDb -Namespace Sale_Saas.Infrastructure.Migrations.ApplicationDb -Context ApplicationDbContext --> application
Add-Migration newtable -OutputDir Migrations/TenantDb -Namespace Sale_Saas.Infrastructure.Migrations.TenantDb -Context TenantDbContext --> master of tenant
## Install packages for Sale_Saas.Utilities
- Install-Package Microsoft.AspNetCore.Http.Extensions
- ## Install packages for Sale_Saas.Domain
- Install-Package MediatR
- Install-Package Microsoft.AspNetCore.Identity.EntityFrameworkCore 
- Install-Package Microsoft.Extensions.Options.ConfigurationExtensions
## Install packages for Sale_Saas.Application
- Install-Package Microsoft.AspNetCore.Http
- Install-Package Ardalis.GuardClauses
- Install-Package AutoMapper.Extensions.Microsoft.DependencyInjection
- Install-Package FluentValidation.DependencyInjectionExtensions
- Install-Package Microsoft.EntityFrameworkCore
- Install-Package System.Data.SqlClient
- Install-Package Newtonsoft.Json
- Install-Package EPPlus
## Install packages for Sale_Saas.Infrastructure
- Install-Package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
- Install-Package Microsoft.AspNetCore.Identity.UI
- Install-Package Microsoft.EntityFrameworkCore.Relational
- Install-Package Microsoft.EntityFrameworkCore.Sqlite
- Install-Package Microsoft.EntityFrameworkCore.SqlServer
- Install-Package Microsoft.EntityFrameworkCore.Tools
## Install packages for Sale_Saas.API
- Install-Package Microsoft.EntityFrameworkCore.Design
- Install-Package Azure.Extensions.AspNetCore.Configuration.Secrets
- Install-Package Azure.Identity
- Install-Package FluentValidation.AspNetCore
- Install-Package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
- Install-Package Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Install-Package Microsoft.AspNetCore.Identity.UI
- Install-Package Microsoft.AspNetCore.OpenApi
- Install-Package Microsoft.AspNetCore.SpaProxy
- Install-Package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore
- Install-Package NSwag.AspNetCore -version 14.0.0-preview010
- Install-Package NSwag.MSBuild -version 14.0.0-preview010
- Install-Package ZymLabs.NSwag.FluentValidation.AspNetCore
- Install-Package Microsoft.AspNetCore.Authentication.JwtBearer