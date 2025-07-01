using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using AzureFunctionIgga.Api.Data;
using AzureFunctionIgga.Api.Services;
using AzureFunctionIgga.Api.Services.Interfaces;
using AzureFunctionIgga.Api.Mappings;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Reflection;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace AzureFunctionIgga.Api.Extensions;

/// <summary>
/// Extensiones para configurar servicios de la aplicación
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configura todos los servicios de la aplicación
    /// </summary>
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // Configurar servicios base
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = false;
            });

        // Configurar OpenAPI/Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Azure Function IGGA API",
                Version = "v1",
                Description = "API REST migrada desde Azure Function - Sistema de gestión empresarial IGGA",
                Contact = new OpenApiContact
                {
                    Name = "Luis Gabriel Ahumada",
                    Email = "luis.ahumada@echonovatech.com",
                    Url = new Uri("https://github.com/luisgabrielahumada")
                }
            });

            // Incluir comentarios XML
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Configurar autenticación JWT en Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: 'Bearer {token}'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Configurar Entity Framework
        services.AddDbContext<IggaDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(120);
            });
            
            options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
            options.EnableDetailedErrors(builder.Environment.IsDevelopment());
        });

        // Configurar AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));

        // Configurar FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Registrar servicios de dominio
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDataProcessingService, DataProcessingService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IAuditService, AuditService>();

        // Configurar servicios de Background (equivalente a Timer Triggers)
        services.AddHostedService<DataProcessingBackgroundService>();
        services.AddHostedService<ReportGenerationBackgroundService>();

        // Configurar Azure Key Vault
        if (!string.IsNullOrEmpty(configuration["KeyVault:VaultUrl"]))
        {
            services.AddSingleton<SecretClient>(provider =>
            {
                var vaultUrl = configuration["KeyVault:VaultUrl"];
                return new SecretClient(new Uri(vaultUrl!), new DefaultAzureCredential());
            });
        }

        // Configurar autenticación JWT
        var jwtSettings = configuration.GetSection("JwtSettings");
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? "default-secret-key")),
                    ClockSkew = TimeSpan.Zero
                };
            });

        // Configurar autorización
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrator"));
            options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Administrator"));
        });

        // Configurar CORS
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Configurar Health Checks
        services.AddHealthChecks()
            .AddDbContext<IggaDbContext>()
            .AddCheck("api", () => HealthCheckResult.Healthy("API está funcionando"))
            .AddCheck("database", () => HealthCheckResult.Healthy("Base de datos conectada"));

        // Configurar HttpClient para servicios externos
        services.AddHttpClient();

        // Configurar options pattern
        services.Configure<AzureStorageOptions>(configuration.GetSection("AzureStorage"));
        services.Configure<EmailOptions>(configuration.GetSection("Email"));
        services.Configure<NotificationOptions>(configuration.GetSection("Notifications"));
    }
}

/// <summary>
/// Opciones de configuración para Azure Storage
/// </summary>
public class AzureStorageOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
}

/// <summary>
/// Opciones de configuración para Email
/// </summary>
public class EmailOptions
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

/// <summary>
/// Opciones de configuración para Notificaciones
/// </summary>
public class NotificationOptions
{
    public bool EnableRealTime { get; set; } = true;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 5;
}