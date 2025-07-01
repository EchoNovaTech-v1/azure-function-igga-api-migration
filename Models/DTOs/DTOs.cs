using System.ComponentModel.DataAnnotations;

namespace AzureFunctionIgga.Api.Models.DTOs;

/// <summary>
/// DTO de Usuario para respuestas - Migrado desde Azure Function
/// </summary>
public record UserDto
{
    public int Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public string? ProfileImageUrl { get; init; }
}

/// <summary>
/// DTO para crear usuario - Migrado desde Azure Function
/// </summary>
public record CreateUserDto
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string FirstName { get; init; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido")]
    [MaxLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    public string LastName { get; init; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    [MaxLength(255, ErrorMessage = "El email no puede exceder 255 caracteres")]
    public string Email { get; init; } = string.Empty;

    [MaxLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? PhoneNumber { get; init; }

    [Required(ErrorMessage = "El rol es requerido")]
    [MaxLength(50, ErrorMessage = "El rol no puede exceder 50 caracteres")]
    public string Role { get; init; } = "User";

    [MaxLength(500, ErrorMessage = "La URL de imagen no puede exceder 500 caracteres")]
    public string? ProfileImageUrl { get; init; }

    [MaxLength(1000, ErrorMessage = "Las notas no pueden exceder 1000 caracteres")]
    public string? Notes { get; init; }
}

/// <summary>
/// DTO para actualizar usuario - Migrado desde Azure Function
/// </summary>
public record UpdateUserDto
{
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string? FirstName { get; init; }

    [MaxLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    public string? LastName { get; init; }

    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    [MaxLength(255, ErrorMessage = "El email no puede exceder 255 caracteres")]
    public string? Email { get; init; }

    [MaxLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? PhoneNumber { get; init; }

    [MaxLength(50, ErrorMessage = "El rol no puede exceder 50 caracteres")]
    public string? Role { get; init; }

    public bool? IsActive { get; init; }

    [MaxLength(500, ErrorMessage = "La URL de imagen no puede exceder 500 caracteres")]
    public string? ProfileImageUrl { get; init; }

    [MaxLength(1000, ErrorMessage = "Las notas no pueden exceder 1000 caracteres")]
    public string? Notes { get; init; }
}

/// <summary>
/// DTO de Registro de Datos para respuestas - Migrado desde Azure Function
/// </summary>
public record DataRecordDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal Value { get; init; }
    public DateTime RecordDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public int UserId { get; init; }
    public string? DocumentUrl { get; init; }
    public UserDto? User { get; init; }
}

/// <summary>
/// DTO para crear registro de datos - Migrado desde Azure Function
/// </summary>
public record CreateDataRecordDto
{
    [Required(ErrorMessage = "El título es requerido")]
    [MaxLength(255, ErrorMessage = "El título no puede exceder 255 caracteres")]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres")]
    public string? Description { get; init; }

    [Required(ErrorMessage = "La categoría es requerida")]
    [MaxLength(100, ErrorMessage = "La categoría no puede exceder 100 caracteres")]
    public string Category { get; init; } = string.Empty;

    [Required(ErrorMessage = "El valor es requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El valor debe ser mayor a 0")]
    public decimal Value { get; init; }

    [Required(ErrorMessage = "La fecha de registro es requerida")]
    public DateTime RecordDate { get; init; }

    [Required(ErrorMessage = "El ID de usuario es requerido")]
    public int UserId { get; init; }

    [MaxLength(500, ErrorMessage = "La URL del documento no puede exceder 500 caracteres")]
    public string? DocumentUrl { get; init; }

    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// DTO para actualizar registro de datos - Migrado desde Azure Function
/// </summary>
public record UpdateDataRecordDto
{
    [MaxLength(255, ErrorMessage = "El título no puede exceder 255 caracteres")]
    public string? Title { get; init; }

    [MaxLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres")]
    public string? Description { get; init; }

    [MaxLength(100, ErrorMessage = "La categoría no puede exceder 100 caracteres")]
    public string? Category { get; init; }

    [MaxLength(50, ErrorMessage = "El estado no puede exceder 50 caracteres")]
    public string? Status { get; init; }

    [Range(0, double.MaxValue, ErrorMessage = "El valor debe ser mayor a 0")]
    public decimal? Value { get; init; }

    public DateTime? RecordDate { get; init; }

    [MaxLength(500, ErrorMessage = "La URL del documento no puede exceder 500 caracteres")]
    public string? DocumentUrl { get; init; }

    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// DTO de Notificación - Migrado desde Azure Function
/// </summary>
public record NotificationDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReadAt { get; init; }
    public int UserId { get; init; }
    public string? ActionUrl { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

/// <summary>
/// DTO para crear notificación - Migrado desde Azure Function
/// </summary>
public record CreateNotificationDto
{
    [Required(ErrorMessage = "El título es requerido")]
    [MaxLength(255, ErrorMessage = "El título no puede exceder 255 caracteres")]
    public string Title { get; init; } = string.Empty;

    [Required(ErrorMessage = "El mensaje es requerido")]
    [MaxLength(2000, ErrorMessage = "El mensaje no puede exceder 2000 caracteres")]
    public string Message { get; init; } = string.Empty;

    [Required(ErrorMessage = "El tipo es requerido")]
    [MaxLength(50, ErrorMessage = "El tipo no puede exceder 50 caracteres")]
    public string Type { get; init; } = "Info";

    [Required(ErrorMessage = "El ID de usuario es requerido")]
    public int UserId { get; init; }

    [MaxLength(500, ErrorMessage = "La URL de acción no puede exceder 500 caracteres")]
    public string? ActionUrl { get; init; }

    public DateTime? ExpiresAt { get; init; }
}

/// <summary>
/// DTO genérico de respuesta API - Migrado desde Azure Function
/// </summary>
public record ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// DTO para resultados paginados - Migrado desde Azure Function
/// </summary>
public record PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// DTO para parámetros de paginación - Migrado desde Azure Function
/// </summary>
public record PaginationQuery
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = false;
}

/// <summary>
/// DTO para procesamiento de datos - Migrado desde Azure Function Queue Trigger
/// </summary>
public record ProcessDataRequest
{
    [Required(ErrorMessage = "El ID del registro de datos es requerido")]
    public int DataRecordId { get; init; }

    [Required(ErrorMessage = "El ID de usuario es requerido")]
    public int UserId { get; init; }

    public Dictionary<string, object>? Parameters { get; init; }
}

/// <summary>
/// DTO para generar reportes - Migrado desde Azure Function
/// </summary>
public record GenerateReportRequest
{
    [Required(ErrorMessage = "El tipo de reporte es requerido")]
    public string ReportType { get; init; } = string.Empty;

    [Required(ErrorMessage = "El ID de usuario es requerido")]
    public int UserId { get; init; }

    public Dictionary<string, object>? Parameters { get; init; }
}