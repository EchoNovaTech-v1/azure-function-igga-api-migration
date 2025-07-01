using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AzureFunctionIgga.Api.Services.Interfaces;
using AzureFunctionIgga.Api.Models.DTOs;

namespace AzureFunctionIgga.Api.Controllers;

/// <summary>
/// Controlador de usuarios - Migrado desde Azure Function HTTP Triggers
/// Mantiene la misma funcionalidad y rutas equivalentes
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los usuarios con paginación
    /// Migrado desde: GET /api/users (Azure Function)
    /// </summary>
    /// <param name="pageNumber">Número de página (por defecto: 1)</param>
    /// <param name="pageSize">Tamaño de página (por defecto: 10, máximo: 100)</param>
    /// <param name="searchTerm">Término de búsqueda opcional</param>
    /// <param name="sortBy">Campo para ordenar</param>
    /// <param name="sortDescending">Orden descendente</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de usuarios</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validar parámetros
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

            var result = await _userService.GetUsersAsync(query, cancellationToken);
            
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint GetUsers");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Obtiene un usuario por ID
    /// Migrado desde: GET /api/users/{id} (Azure Function)
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Usuario encontrado</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID de usuario inválido"
                });
            }

            var result = await _userService.GetUserByIdAsync(id, cancellationToken);
            
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint GetUser para ID: {UserId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Obtiene un usuario por email
    /// Migrado desde: GET /api/users/by-email/{email} (Azure Function)
    /// </summary>
    /// <param name="email">Email del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Usuario encontrado</returns>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Email es requerido"
                });
            }

            var result = await _userService.GetUserByEmailAsync(email, cancellationToken);
            
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint GetUserByEmail para email: {Email}", email);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Crea un nuevo usuario
    /// Migrado desde: POST /api/users (Azure Function)
    /// </summary>
    /// <param name="createUserDto">Datos del usuario a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Usuario creado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser(
        [FromBody] CreateUserDto createUserDto, 
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

            var result = await _userService.CreateUserAsync(createUserDto, cancellationToken);
            
            if (result.Success && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetUser), 
                    new { id = result.Data.Id }, 
                    result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint CreateUser");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Actualiza un usuario existente
    /// Migrado desde: PUT /api/users/{id} (Azure Function)
    /// </summary>
    /// <param name="id">ID del usuario a actualizar</param>
    /// <param name="updateUserDto">Datos del usuario a actualizar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Usuario actualizado</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(
        int id, 
        [FromBody] UpdateUserDto updateUserDto, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID de usuario inválido"
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

            var result = await _userService.UpdateUserAsync(id, updateUserDto, cancellationToken);
            
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint UpdateUser para ID: {UserId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Elimina un usuario
    /// Migrado desde: DELETE /api/users/{id} (Azure Function)
    /// </summary>
    /// <param name="id">ID del usuario a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Confirmación de eliminación</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID de usuario inválido"
                });
            }

            var result = await _userService.DeleteUserAsync(id, cancellationToken);
            
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint DeleteUser para ID: {UserId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Activa un usuario
    /// Migrado desde: POST /api/users/{id}/activate (Azure Function)
    /// </summary>
    /// <param name="id">ID del usuario a activar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Confirmación de activación</returns>
    [HttpPost("{id:int}/activate")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<bool>>> ActivateUser(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID de usuario inválido"
                });
            }

            var result = await _userService.ActivateUserAsync(id, cancellationToken);
            
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint ActivateUser para ID: {UserId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Desactiva un usuario
    /// Migrado desde: POST /api/users/{id}/deactivate (Azure Function)
    /// </summary>
    /// <param name="id">ID del usuario a desactivar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Confirmación de desactivación</returns>
    [HttpPost("{id:int}/deactivate")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<bool>>> DeactivateUser(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID de usuario inválido"
                });
            }

            var result = await _userService.DeactivateUserAsync(id, cancellationToken);
            
            return result.Success ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint DeactivateUser para ID: {UserId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }
}