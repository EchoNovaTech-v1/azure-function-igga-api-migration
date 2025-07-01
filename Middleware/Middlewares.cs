using System.Text.Json;
using System.Net;

namespace AzureFunctionIgga.Api.Middleware;

/// <summary>
/// Middleware de manejo de excepciones - Migrado desde Azure Function error handling
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado en la aplicación");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            Success = false,
            Message = "Error interno del servidor",
            Timestamp = DateTime.UtcNow,
            TraceId = context.TraceIdentifier
        };

        switch (exception)
        {
            case ArgumentException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = response with { Message = "Parámetros inválidos" };
                break;
            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response = response with { Message = "Acceso no autorizado" };
                break;
            case KeyNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = response with { Message = "Recurso no encontrado" };
                break;
            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Middleware de logging de requests - Migrado desde Azure Function logging
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var requestId = context.TraceIdentifier;

        // Log del request
        _logger.LogInformation(
            "Iniciando request {RequestId}: {Method} {Path} desde {RemoteIpAddress}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            context.Connection.RemoteIpAddress);

        try
        {
            await _next(context);
        }
        finally
        {
            var duration = DateTime.UtcNow - startTime;
            
            _logger.LogInformation(
                "Completado request {RequestId}: {Method} {Path} - {StatusCode} en {Duration}ms",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                duration.TotalMilliseconds);
        }
    }
}

/// <summary>
/// Middleware de validación de API Key - Migrado desde Azure Function auth
/// </summary>
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip API key validation for certain paths
        var path = context.Request.Path.Value?.ToLower();
        if (path != null && (path.Contains("swagger") || path.Contains("health") || path == "/"))
        {
            await _next(context);
            return;
        }

        // Check for API key in headers
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
        {
            _logger.LogWarning("Request sin API Key desde {RemoteIpAddress}", context.Connection.RemoteIpAddress);
            
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            
            var response = JsonSerializer.Serialize(new
            {
                Success = false,
                Message = "API Key requerida",
                Timestamp = DateTime.UtcNow
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await context.Response.WriteAsync(response);
            return;
        }

        var apiKey = apiKeyHeader.FirstOrDefault();
        var validApiKey = _configuration["ApiKey"] ?? "default-api-key";

        if (apiKey != validApiKey)
        {
            _logger.LogWarning("API Key inválida desde {RemoteIpAddress}", context.Connection.RemoteIpAddress);
            
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            
            var response = JsonSerializer.Serialize(new
            {
                Success = false,
                Message = "API Key inválida",
                Timestamp = DateTime.UtcNow
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await context.Response.WriteAsync(response);
            return;
        }

        await _next(context);
    }
}

/// <summary>
/// Middleware de rate limiting - Migrado desde Azure Function throttling
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly Dictionary<string, List<DateTime>> _requests = new();
    private static readonly object _lock = new();

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        
        if (!IsAllowed(clientId))
        {
            _logger.LogWarning("Rate limit excedido para cliente: {ClientId}", clientId);
            
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.ContentType = "application/json";
            
            var response = JsonSerializer.Serialize(new
            {
                Success = false,
                Message = "Demasiadas solicitudes. Intente más tarde.",
                Timestamp = DateTime.UtcNow,
                RetryAfter = "60 seconds"
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await context.Response.WriteAsync(response);
            return;
        }

        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Usar API Key si está disponible, sino IP
        if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
        {
            return apiKey.FirstOrDefault()?.GetHashCode().ToString() ?? "unknown";
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static bool IsAllowed(string clientId, int maxRequests = 100, TimeSpan? window = null)
    {
        window ??= TimeSpan.FromMinutes(1);

        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var windowStart = now - window.Value;

            if (!_requests.ContainsKey(clientId))
            {
                _requests[clientId] = new List<DateTime>();
            }

            var clientRequests = _requests[clientId];
            
            // Remover requests antiguos
            clientRequests.RemoveAll(r => r < windowStart);

            if (clientRequests.Count >= maxRequests)
            {
                return false;
            }

            clientRequests.Add(now);
            return true;
        }
    }
}

/// <summary>
/// Middleware de auditoría - Nueva funcionalidad para tracking
/// </summary>
public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Solo auditar operaciones de modificación
        var method = context.Request.Method.ToUpper();
        if (method is "POST" or "PUT" or "DELETE")
        {
            var auditInfo = new
            {
                Timestamp = DateTime.UtcNow,
                Method = method,
                Path = context.Request.Path,
                UserAgent = context.Request.Headers.UserAgent.FirstOrDefault(),
                RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserId = context.User?.FindFirst("sub")?.Value ?? context.User?.FindFirst("id")?.Value,
                TraceId = context.TraceIdentifier
            };

            _logger.LogInformation("Operación auditada: {@AuditInfo}", auditInfo);
        }

        await _next(context);
    }
}