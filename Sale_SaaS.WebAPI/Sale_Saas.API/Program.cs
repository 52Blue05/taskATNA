using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Sale_Saas.API;
using Sale_Saas.API.BackgroundServices;
using Sale_Saas.API.Configs;
using Sale_Saas.API.Middlewares;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Infrastructure.Middleware;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Configuration.AddJsonFile("Configs/BackgroundServiceSetting.json", optional: false, reloadOnChange: true);

// Config limit body length
var requestBodyConfiguration = new RequestBodyConfiguration();
builder.Configuration.GetSection("RequestBodyConfiguration").Bind(requestBodyConfiguration);
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = requestBodyConfiguration.MaxMultipartBodyLength != 0 ? requestBodyConfiguration.MaxMultipartBodyLength : RequestBodyConfigConstant.DefaultMaxRequestBodySize; // 2GB
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = requestBodyConfiguration.MaxMultipartBodyLength != 0 ? requestBodyConfiguration.MaxMultipartBodyLength : RequestBodyConfigConstant.DefaultMaxRequestBodySize;
});

builder.Services.AddControllers();
builder.Services.AddOptions();

builder.Services.AddHttpClient();

//Declare DI
builder.Services.AddDomainServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger Basic Solution", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.OperationFilter<MyHeaderFilter>();
    c.AddSecurityRequirement(new OpenApiSecurityRequirement(){{
        new OpenApiSecurityScheme {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            },
            Scheme = "oauth2",
            Name = "Bearer",
            In = ParameterLocation.Header,
        },
        new List<string>()
    }});
});

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = JWTConstant.ValidAudience,
        ValidIssuer = JWTConstant.ValidIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTConstant.Secret))
    };
});

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.Logging.AddConsole();

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Content-Disposition");
        });
});

var backgroundServiceSetting = new BackgroundServiceSetting();
builder.Configuration.GetSection("BackgroundServiceSetting").Bind(backgroundServiceSetting);
builder.Services.ConfigureBackground(backgroundServiceSetting);

builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //await app.InitialiseDatabaseAsync();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Saas");
        options.RoutePrefix = String.Empty;
    });
}

app.UseCors();
//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantResolver>();
app.UseMiddleware<TrackingLogMiddleware>();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
