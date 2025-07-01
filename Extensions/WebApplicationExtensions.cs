using Microsoft.EntityFrameworkCore;
using AzureFunctionIgga.Api.Data;
using AzureFunctionIgga.Api.Middleware;
using Serilog;

namespace AzureFunctionIgga.Api.Extensions;

/// <summary>
/// Extensiones para configurar el pipeline de la aplicación
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configura el pipeline de middleware de la aplicación
    /// </summary>
    public static void ConfigurePipeline(this WebApplication app)
    {
        // Configurar Swagger en desarrollo
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Azure Function IGGA API v1");
                c.RoutePrefix = string.Empty; // Swagger en la raíz
                c.DisplayRequestDuration();
                c.EnableTryItOutByDefault();
            });
        }

        // Configurar manejo de errores
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
        }

        // Middleware personalizado para logging de requests
        app.UseMiddleware<RequestLoggingMiddleware>();

        // Middleware personalizado para manejo de errores
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Configurar HTTPS
        app.UseHttpsRedirection();

        // Configurar CORS
        app.UseCors();

        // Configurar Serilog request logging
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = (httpContext, elapsed, ex) => ex != null 
                ? LogEventLevel.Error 
                : httpContext.Response.StatusCode > 499 
                    ? LogEventLevel.Error 
                    : LogEventLevel.Information;
        });

        // Configurar autenticación y autorización
        app.UseAuthentication();
        app.UseAuthorization();

        // Configurar enrutamiento
        app.UseRouting();

        // Configurar controladores
        app.MapControllers();

        // Configurar Health Checks
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(x => new
                    {
                        name = x.Key,
                        status = x.Value.Status.ToString(),
                        exception = x.Value.Exception?.Message,
                        duration = x.Value.Duration.ToString()
                    }),
                    totalDuration = report.TotalDuration.ToString()
                };
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
            }
        });

        // Endpoint de información de la API
        app.MapGet("/", () => new
        {
            Service = "Azure Function IGGA API",
            Version = "1.0.0",
            Environment = app.Environment.EnvironmentName,
            Timestamp = DateTime.UtcNow,
            Status = "Running",
            Migration = "Migrated from Azure Function to ASP.NET Core API",
            Documentation = app.Environment.IsDevelopment() ? "/swagger" : "Contact administrator",
            HealthCheck = "/health"
        })
        .WithName("GetApiInfo")
        .WithOpenApi()
        .Produces(200);

        // Endpoint de error para manejo centralizado
        app.Map("/error", (HttpContext context) =>
        {
            var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            Log.Error(error?.Error, "Unhandled exception occurred");
            
            return Results.Problem(
                title: "An error occurred",
                statusCode: 500,
                detail: app.Environment.IsDevelopment() ? error?.Error?.Message : "Internal server error"
            );
        });

        Log.Information("✅ Pipeline configurado correctamente");
    }

    /// <summary>
    /// Aplica migraciones de base de datos pendientes
    /// </summary>
    public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IggaDbContext>();
        
        try
        {
            Log.Information("🔍 Verificando migraciones de base de datos...");
            
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            
            if (pendingMigrations.Any())
            {
                Log.Information("📊 Aplicando {Count} migraciones pendientes", pendingMigrations.Count());
                await dbContext.Database.MigrateAsync();
                Log.Information("✅ Migraciones aplicadas exitosamente");
            }
            else
            {
                Log.Information("✅ Base de datos actualizada, no hay migraciones pendientes");
            }

            // Verificar conexión
            await dbContext.Database.CanConnectAsync();
            Log.Information("✅ Conexión a base de datos verificada");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Error al aplicar migraciones de base de datos");
            throw;
        }
    }
}