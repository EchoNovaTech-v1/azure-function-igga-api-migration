using AzureFunctionIgga.Api.Services.Interfaces;

namespace AzureFunctionIgga.Api.Services;

/// <summary>
/// Servicio de procesamiento en background - Migrado desde Azure Function Timer Trigger
/// Ejecuta tareas programadas que antes eran Timer Triggers
/// </summary>
public class DataProcessingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataProcessingBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Equivalente a Timer Trigger cada 5 minutos

    public DataProcessingBackgroundService(IServiceProvider serviceProvider, ILogger<DataProcessingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio de procesamiento en background iniciado - Migrado desde Timer Trigger");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingJobs(stoppingToken);
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Servicio de procesamiento en background detenido");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en servicio de procesamiento en background");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Esperar antes de reintentar
            }
        }
    }

    /// <summary>
    /// Procesa trabajos pendientes - Migrado desde Azure Function Timer Trigger
    /// </summary>
    private async Task ProcessPendingJobs(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var processingJobService = scope.ServiceProvider.GetRequiredService<IProcessingJobService>();

        try
        {
            _logger.LogInformation("Iniciando procesamiento de trabajos pendientes - {Timestamp}", DateTime.UtcNow);
            
            await processingJobService.ProcessPendingJobsAsync(cancellationToken);
            
            _logger.LogInformation("Procesamiento de trabajos completado - {Timestamp}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar trabajos pendientes");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deteniendo servicio de procesamiento en background");
        await base.StopAsync(cancellationToken);
    }
}

/// <summary>
/// Servicio de generación de reportes en background - Migrado desde Azure Function Timer Trigger
/// Genera reportes programados que antes eran Timer Triggers
/// </summary>
public class ReportGenerationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReportGenerationBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Equivalente a Timer Trigger cada hora

    public ReportGenerationBackgroundService(IServiceProvider serviceProvider, ILogger<ReportGenerationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio de generación de reportes iniciado - Migrado desde Timer Trigger");

        // Esperar un poco antes de comenzar
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await GenerateScheduledReports(stoppingToken);
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Servicio de generación de reportes detenido");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en servicio de generación de reportes");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Esperar antes de reintentar
            }
        }
    }

    /// <summary>
    /// Genera reportes programados - Migrado desde Azure Function Timer Trigger
    /// </summary>
    private async Task GenerateScheduledReports(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();

        try
        {
            _logger.LogInformation("Iniciando generación de reportes programados - {Timestamp}", DateTime.UtcNow);
            
            // Generar reporte de dashboard diario
            await GenerateDailyDashboard(reportService, cancellationToken);
            
            // Generar reportes de analytics si es lunes
            if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday)
            {
                await GenerateWeeklyAnalytics(reportService, cancellationToken);
            }

            // Generar reportes mensuales si es el primer día del mes
            if (DateTime.UtcNow.Day == 1)
            {
                await GenerateMonthlyReports(reportService, cancellationToken);
            }
            
            _logger.LogInformation("Generación de reportes programados completada - {Timestamp}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reportes programados");
        }
    }

    private async Task GenerateDailyDashboard(IReportService reportService, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Generando dashboard diario");
            
            // Generar dashboard para usuarios administradores (ID = 1)
            var dashboardResult = await reportService.GetDashboardDataAsync(1, cancellationToken);
            
            if (dashboardResult.Success)
            {
                _logger.LogInformation("Dashboard diario generado exitosamente");
            }
            else
            {
                _logger.LogWarning("Error al generar dashboard diario: {Message}", dashboardResult.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar dashboard diario");
        }
    }

    private async Task GenerateWeeklyAnalytics(IReportService reportService, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Generando analytics semanales");
            
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-7);
            
            var analyticsResult = await reportService.GetAnalyticsDataAsync("weekly", startDate, endDate, cancellationToken);
            
            if (analyticsResult.Success)
            {
                _logger.LogInformation("Analytics semanales generados exitosamente");
            }
            else
            {
                _logger.LogWarning("Error al generar analytics semanales: {Message}", analyticsResult.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar analytics semanales");
        }
    }

    private async Task GenerateMonthlyReports(IReportService reportService, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Generando reportes mensuales");
            
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddMonths(-1);
            
            var analyticsResult = await reportService.GetAnalyticsDataAsync("monthly", startDate, endDate, cancellationToken);
            
            if (analyticsResult.Success)
            {
                _logger.LogInformation("Reportes mensuales generados exitosamente");
            }
            else
            {
                _logger.LogWarning("Error al generar reportes mensuales: {Message}", analyticsResult.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reportes mensuales");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deteniendo servicio de generación de reportes");
        await base.StopAsync(cancellationToken);
    }
}

/// <summary>
/// Servicio de limpieza en background - Migrado desde Azure Function Timer Trigger
/// Ejecuta tareas de mantenimiento y limpieza
/// </summary>
public class CleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CleanupBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24); // Ejecutar diariamente

    public CleanupBackgroundService(IServiceProvider serviceProvider, ILogger<CleanupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio de limpieza iniciado - Migrado desde Timer Trigger");

        // Esperar hasta medianoche para empezar
        await WaitUntilMidnight(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformCleanupTasks(stoppingToken);
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Servicio de limpieza detenido");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en servicio de limpieza");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Esperar antes de reintentar
            }
        }
    }

    private async Task WaitUntilMidnight(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var midnight = now.Date.AddDays(1); // Próxima medianoche
        var timeToWait = midnight - now;

        if (timeToWait.TotalHours > 1) // Si falta más de una hora, esperar solo una hora
        {
            timeToWait = TimeSpan.FromHours(1);
        }

        _logger.LogInformation("Esperando {Hours} horas hasta la próxima ejecución de limpieza", timeToWait.TotalHours);
        await Task.Delay(timeToWait, cancellationToken);
    }

    /// <summary>
    /// Ejecuta tareas de limpieza - Migrado desde Azure Function Timer Trigger
    /// </summary>
    private async Task PerformCleanupTasks(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        try
        {
            _logger.LogInformation("Iniciando tareas de limpieza - {Timestamp}", DateTime.UtcNow);
            
            await CleanupExpiredNotifications(scope, cancellationToken);
            await CleanupOldAuditLogs(scope, cancellationToken);
            await CleanupCompletedJobs(scope, cancellationToken);
            
            _logger.LogInformation("Tareas de limpieza completadas - {Timestamp}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar tareas de limpieza");
        }
    }

    private async Task CleanupExpiredNotifications(IServiceScope scope, CancellationToken cancellationToken)
    {
        try
        {
            // Limpiar notificaciones expiradas (implementación simplificada)
            _logger.LogInformation("Limpiando notificaciones expiradas");
            
            // Aquí iría la lógica para eliminar notificaciones expiradas
            // usando el NotificationService
            
            await Task.CompletedTask; // Placeholder
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar notificaciones expiradas");
        }
    }

    private async Task CleanupOldAuditLogs(IServiceScope scope, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Limpiando logs de auditoría antiguos (>90 días)");
            
            // Aquí iría la lógica para eliminar logs de auditoría antiguos
            // usando el AuditService
            
            await Task.CompletedTask; // Placeholder
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar logs de auditoría antiguos");
        }
    }

    private async Task CleanupCompletedJobs(IServiceScope scope, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Limpiando trabajos completados antiguos (>30 días)");
            
            // Aquí iría la lógica para eliminar trabajos completados antiguos
            // usando el ProcessingJobService
            
            await Task.CompletedTask; // Placeholder
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar trabajos completados");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deteniendo servicio de limpieza");
        await base.StopAsync(cancellationToken);
    }
}