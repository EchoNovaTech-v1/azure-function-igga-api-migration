using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AzureFunctionIgga.Api.Services.Interfaces;
using AzureFunctionIgga.Api.Models.DTOs;

namespace AzureFunctionIgga.Api.Controllers;

/// <summary>
/// Controlador de procesamiento de datos - Migrado desde Azure Function Queue/Timer Triggers
/// Mantiene la funcionalidad de procesamiento asíncrono y batch
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DataController : ControllerBase
{
    private readonly IDataProcessingService _dataProcessingService;
    private readonly ILogger<DataController> _logger;

    public DataController(IDataProcessingService dataProcessingService, ILogger<DataController> logger)
    {
        _dataProcessingService = dataProcessingService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los registros de datos con paginación
    /// Migrado desde: GET /api/data (Azure Function)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DataRecordDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<PagedResult<DataRecordDto>>>> GetDataRecords(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = new PaginationQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _dataProcessingService.GetDataRecordsAsync(query, cancellationToken);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint GetDataRecords");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Obtiene un registro de datos por ID
    /// Migrado desde: GET /api/data/{id} (Azure Function)
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DataRecordDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<DataRecordDto>>> GetDataRecord(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID de registro inválido"
                });
            }

            var result = await _dataProcessingService.GetDataRecordByIdAsync(id, cancellationToken);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint GetDataRecord para ID: {RecordId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Obtiene registros de datos por usuario
    /// Migrado desde: GET /api/data/user/{userId} (Azure Function)
    /// </summary>
    [HttpGet("user/{userId:int}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DataRecordDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<PagedResult<DataRecordDto>>>> GetDataRecordsByUser(
        int userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (userId <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID de usuario inválido"
                });
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = new PaginationQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _dataProcessingService.GetDataRecordsByUserAsync(userId, query, cancellationToken);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint GetDataRecordsByUser para usuario: {UserId}", userId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Crea un nuevo registro de datos
    /// Migrado desde: POST /api/data (Azure Function)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DataRecordDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<DataRecordDto>>> CreateDataRecord(
        [FromBody] CreateDataRecordDto createDataRecordDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    )
                });
            }

            var result = await _dataProcessingService.CreateDataRecordAsync(createDataRecordDto, cancellationToken);
            
            if (result.Success && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetDataRecord),
                    new { id = result.Data.Id },
                    result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint CreateDataRecord");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Actualiza un registro de datos existente
    /// Migrado desde: PUT /api/data/{id} (Azure Function)
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DataRecordDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<DataRecordDto>>> UpdateDataRecord(
        int id,
        [FromBody] UpdateDataRecordDto updateDataRecordDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID de registro inválido"
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    )
                });
            }

            var result = await _dataProcessingService.UpdateDataRecordAsync(id, updateDataRecordDto, cancellationToken);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint UpdateDataRecord para ID: {RecordId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Elimina un registro de datos
    /// Migrado desde: DELETE /api/data/{id} (Azure Function)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDataRecord(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID de registro inválido"
                });
            }

            var result = await _dataProcessingService.DeleteDataRecordAsync(id, cancellationToken);
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint DeleteDataRecord para ID: {RecordId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Procesa datos - Migrado desde Queue Trigger
    /// Migrado desde: Azure Function Queue Trigger → REST API endpoint
    /// </summary>
    [HttpPost("process")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<object>>> ProcessData(
        [FromBody] ProcessDataRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    )
                });
            }

            _logger.LogInformation("Iniciando procesamiento de datos para registro: {DataRecordId}", request.DataRecordId);

            var result = await _dataProcessingService.ProcessDataAsync(request, cancellationToken);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint ProcessData para registro: {DataRecordId}", request.DataRecordId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Procesa múltiples datos en lote - Migrado desde Queue Trigger batch
    /// Migrado desde: Azure Function Queue Trigger batch → REST API endpoint
    /// </summary>
    [HttpPost("process-batch")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<object>>> ProcessDataBatch(
        [FromBody] IEnumerable<ProcessDataRequest> requests,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!requests.Any())
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Se requiere al menos una solicitud de procesamiento"
                });
            }

            if (requests.Count() > 50) // Limitar el batch
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "El lote no puede contener más de 50 elementos"
                });
            }

            _logger.LogInformation("Iniciando procesamiento en lote de {Count} registros", requests.Count());

            var result = await _dataProcessingService.ProcessDataBatchAsync(requests, cancellationToken);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint ProcessDataBatch");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }
}