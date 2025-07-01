using AzureFunctionIgga.Api.Extensions;
using AzureFunctionIgga.Api.Middleware;
using Serilog;

namespace AzureFunctionIgga.Api;

/// <summary>
/// Punto de entrada principal de la aplicación API REST migrada desde Azure Function
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        // Configurar Serilog temprano
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/api-log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("🚀 Iniciando API REST IGGA - Migración desde Azure Function");

            var builder = WebApplication.CreateBuilder(args);

            // Configurar servicios
            builder.ConfigureServices();
            
            // Configurar Serilog
            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            });

            var app = builder.Build();

            // Configurar pipeline
            app.ConfigurePipeline();

            // Aplicar migraciones de base de datos
            await app.ApplyDatabaseMigrationsAsync();

            Log.Information("✅ API REST IGGA iniciada exitosamente en {Environment}", 
                app.Environment.EnvironmentName);

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "❌ Error fatal al iniciar la aplicación");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}