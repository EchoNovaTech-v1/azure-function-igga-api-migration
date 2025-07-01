using AzureFunctionIgga.Api.Models.DTOs;

namespace AzureFunctionIgga.Api.Services.Interfaces;

/// <summary>
/// Interfaz de servicio de usuarios - Migrada desde Azure Function
/// </summary>
public interface IUserService
{
    Task<ApiResponse<UserDto>> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<UserDto>>> GetUsersAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> UpdateUserAsync(int id, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteUserAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ActivateUserAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeactivateUserAsync(int id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interfaz de servicio de procesamiento de datos - Migrada desde Azure Function Queue/Timer Triggers
/// </summary>
public interface IDataProcessingService
{
    Task<ApiResponse<DataRecordDto>> GetDataRecordByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<DataRecordDto>>> GetDataRecordsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<DataRecordDto>>> GetDataRecordsByUserAsync(int userId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<ApiResponse<DataRecordDto>> CreateDataRecordAsync(CreateDataRecordDto createDataRecordDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<DataRecordDto>> UpdateDataRecordAsync(int id, UpdateDataRecordDto updateDataRecordDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteDataRecordAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> ProcessDataAsync(ProcessDataRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> ProcessDataBatchAsync(IEnumerable<ProcessDataRequest> requests, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interfaz de servicio de notificaciones - Migrada desde Azure Function
/// </summary>
public interface INotificationService
{
    Task<ApiResponse<NotificationDto>> GetNotificationByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<NotificationDto>>> GetNotificationsByUserAsync(int userId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<ApiResponse<NotificationDto>> CreateNotificationAsync(CreateNotificationDto createNotificationDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> MarkAsReadAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteNotificationAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<int>> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default);
    Task SendRealTimeNotificationAsync(int userId, string title, string message, string type = "Info");
}

/// <summary>
/// Interfaz de servicio de reportes - Migrada desde Azure Function
/// </summary>
public interface IReportService
{
    Task<ApiResponse<byte[]>> GeneratePdfReportAsync(GenerateReportRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<byte[]>> GenerateExcelReportAsync(GenerateReportRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> GenerateChartDataAsync(string chartType, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> GetDashboardDataAsync(int userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> GetAnalyticsDataAsync(string analyticsType, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<object>>> GetReportsAsync(int userId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> GetReportStatusAsync(int reportId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interfaz de servicio de almacenamiento de archivos - Migrada desde Azure Function Blob Triggers
/// </summary>
public interface IFileStorageService
{
    Task<ApiResponse<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<ApiResponse<Stream>> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> GetFileUrlAsync(string fileName, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> GetFileMetadataAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<object>>> GetFilesAsync(PaginationQuery query, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interfaz de servicio de auditoría - Nueva funcionalidad migrada
/// </summary>
public interface IAuditService
{
    Task<ApiResponse<PagedResult<object>>> GetAuditLogsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<object>>> GetAuditLogsByEntityAsync(string entityType, int entityId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<object>>> GetAuditLogsByUserAsync(int userId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task LogActionAsync(string action, string entityType, int entityId, int userId, object? oldValues = null, object? newValues = null, string? ipAddress = null, string? userAgent = null);
}

/// <summary>
/// Interfaz de servicio de autenticación - Migrada desde Azure Function
/// </summary>
public interface IAuthenticationService
{
    Task<ApiResponse<AuthenticationResult>> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuthenticationResult>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ResetPasswordAsync(string email, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interfaz de servicio de trabajos de procesamiento - Migrada desde Timer Triggers
/// </summary>
public interface IProcessingJobService
{
    Task<ApiResponse<object>> GetJobByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<object>>> GetJobsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<object>>> GetJobsByUserAsync(int userId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> CreateJobAsync(CreateProcessingJobDto createJobDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> UpdateJobStatusAsync(int id, string status, int progress = 0, string? errorMessage = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> CompleteJobAsync(int id, Dictionary<string, object>? result = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> CancelJobAsync(int id, CancellationToken cancellationToken = default);
    Task ProcessPendingJobsAsync(CancellationToken cancellationToken = default);
}

// DTOs auxiliares para servicios
public record AuthenticationResult
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public UserDto User { get; init; } = null!;
}

public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool RememberMe { get; init; } = false;
}

public record CreateProcessingJobDto
{
    public string JobName { get; init; } = string.Empty;
    public int UserId { get; init; }
    public Dictionary<string, object>? Parameters { get; init; }
}

public record FileUploadResult
{
    public string FileUrl { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string ContentType { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; } = DateTime.UtcNow;
}