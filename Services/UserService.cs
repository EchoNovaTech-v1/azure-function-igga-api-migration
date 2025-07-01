using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AzureFunctionIgga.Api.Data;
using AzureFunctionIgga.Api.Models.Entities;
using AzureFunctionIgga.Api.Models.DTOs;
using AzureFunctionIgga.Api.Services.Interfaces;

namespace AzureFunctionIgga.Api.Services;

/// <summary>
/// Implementación del servicio de usuarios - Migrado desde Azure Function
/// </summary>
public class UserService : IUserService
{
    private readonly IggaDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(IggaDbContext context, IMapper mapper, ILogger<UserService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene un usuario por ID - Migrado desde GET /api/users/{id}
    /// </summary>
    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo usuario con ID: {UserId}", id);

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Usuario con ID {UserId} no encontrado", id);
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Usuario no encontrado",
                    Data = null
                };
            }

            var userDto = _mapper.Map<UserDto>(user);
            
            _logger.LogInformation("Usuario con ID {UserId} obtenido exitosamente", id);
            return new ApiResponse<UserDto>
            {
                Success = true,
                Message = "Usuario obtenido exitosamente",
                Data = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario con ID {UserId}", id);
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = "Error interno del servidor",
                Data = null
            };
        }
    }

    /// <summary>
    /// Obtiene un usuario por email - Migrado desde Azure Function
    /// </summary>
    public async Task<ApiResponse<UserDto>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo usuario con email: {Email}", email);

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Usuario con email {Email} no encontrado", email);
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Usuario no encontrado",
                    Data = null
                };
            }

            var userDto = _mapper.Map<UserDto>(user);
            
            _logger.LogInformation("Usuario con email {Email} obtenido exitosamente", email);
            return new ApiResponse<UserDto>
            {
                Success = true,
                Message = "Usuario obtenido exitosamente",
                Data = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario con email {Email}", email);
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = "Error interno del servidor",
                Data = null
            };
        }
    }

    /// <summary>
    /// Obtiene usuarios paginados - Migrado desde GET /api/users
    /// </summary>
    public async Task<ApiResponse<PagedResult<UserDto>>> GetUsersAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo usuarios con paginación: Página {PageNumber}, Tamaño {PageSize}", 
                query.PageNumber, query.PageSize);

            var usersQuery = _context.Users.AsNoTracking();

            // Aplicar filtro de búsqueda
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var searchTerm = query.SearchTerm.ToLower();
                usersQuery = usersQuery.Where(u => 
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm));
            }

            // Aplicar ordenamiento
            usersQuery = query.SortBy?.ToLower() switch
            {
                "firstname" => query.SortDescending ? usersQuery.OrderByDescending(u => u.FirstName) : usersQuery.OrderBy(u => u.FirstName),
                "lastname" => query.SortDescending ? usersQuery.OrderByDescending(u => u.LastName) : usersQuery.OrderBy(u => u.LastName),
                "email" => query.SortDescending ? usersQuery.OrderByDescending(u => u.Email) : usersQuery.OrderBy(u => u.Email),
                "createdat" => query.SortDescending ? usersQuery.OrderByDescending(u => u.CreatedAt) : usersQuery.OrderBy(u => u.CreatedAt),
                _ => usersQuery.OrderBy(u => u.Id)
            };

            var totalCount = await usersQuery.CountAsync(cancellationToken);
            
            var users = await usersQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            var userDtos = _mapper.Map<List<UserDto>>(users);

            var pagedResult = new PagedResult<UserDto>
            {
                Items = userDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            _logger.LogInformation("Obtenidos {Count} usuarios de un total de {TotalCount}", 
                userDtos.Count, totalCount);

            return new ApiResponse<PagedResult<UserDto>>
            {
                Success = true,
                Message = "Usuarios obtenidos exitosamente",
                Data = pagedResult
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios");
            return new ApiResponse<PagedResult<UserDto>>
            {
                Success = false,
                Message = "Error interno del servidor",
                Data = null
            };
        }
    }

    /// <summary>
    /// Crea un nuevo usuario - Migrado desde POST /api/users
    /// </summary>
    public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creando usuario con email: {Email}", createUserDto.Email);

            // Verificar si el email ya existe
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == createUserDto.Email.ToLower(), cancellationToken);

            if (existingUser != null)
            {
                _logger.LogWarning("Intento de crear usuario con email duplicado: {Email}", createUserDto.Email);
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Ya existe un usuario con este email",
                    Data = null
                };
            }

            var user = _mapper.Map<User>(createUserDto);
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            var userDto = _mapper.Map<UserDto>(user);

            _logger.LogInformation("Usuario creado exitosamente con ID {UserId}", user.Id);

            return new ApiResponse<UserDto>
            {
                Success = true,
                Message = "Usuario creado exitosamente",
                Data = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario");
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = "Error interno del servidor",
                Data = null
            };
        }
    }

    /// <summary>
    /// Actualiza un usuario - Migrado desde PUT /api/users/{id}
    /// </summary>
    public async Task<ApiResponse<UserDto>> UpdateUserAsync(int id, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Actualizando usuario con ID: {UserId}", id);

            var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Usuario con ID {UserId} no encontrado para actualización", id);
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Usuario no encontrado",
                    Data = null
                };
            }

            // Verificar email único si se está actualizando
            if (!string.IsNullOrEmpty(updateUserDto.Email) && updateUserDto.Email != user.Email)
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == updateUserDto.Email.ToLower() && u.Id != id, cancellationToken);

                if (existingUser != null)
                {
                    _logger.LogWarning("Intento de actualizar a email duplicado: {Email}", updateUserDto.Email);
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "Ya existe un usuario con este email",
                        Data = null
                    };
                }
            }

            // Aplicar actualizaciones usando AutoMapper
            _mapper.Map(updateUserDto, user);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var userDto = _mapper.Map<UserDto>(user);

            _logger.LogInformation("Usuario actualizado exitosamente con ID {UserId}", user.Id);

            return new ApiResponse<UserDto>
            {
                Success = true,
                Message = "Usuario actualizado exitosamente",
                Data = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario con ID {UserId}", id);
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = "Error interno del servidor",
                Data = null
            };
        }
    }

    /// <summary>
    /// Elimina un usuario - Migrado desde DELETE /api/users/{id}
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteUserAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Eliminando usuario con ID: {UserId}", id);

            var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Usuario con ID {UserId} no encontrado para eliminación", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Usuario no encontrado",
                    Data = false
                };
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Usuario eliminado exitosamente con ID {UserId}", id);

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Usuario eliminado exitosamente",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario con ID {UserId}", id);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Error interno del servidor",
                Data = false
            };
        }
    }

    /// <summary>
    /// Activa un usuario - Migrado desde Azure Function
    /// </summary>
    public async Task<ApiResponse<bool>> ActivateUserAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Activando usuario con ID: {UserId}", id);

            var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Usuario con ID {UserId} no encontrado para activación", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Usuario no encontrado",
                    Data = false
                };
            }

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Usuario activado exitosamente con ID {UserId}", id);

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Usuario activado exitosamente",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar usuario con ID {UserId}", id);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Error interno del servidor",
                Data = false
            };
        }
    }

    /// <summary>
    /// Desactiva un usuario - Migrado desde Azure Function
    /// </summary>
    public async Task<ApiResponse<bool>> DeactivateUserAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Desactivando usuario con ID: {UserId}", id);

            var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Usuario con ID {UserId} no encontrado para desactivación", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Usuario no encontrado",
                    Data = false
                };
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Usuario desactivado exitosamente con ID {UserId}", id);

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Usuario desactivado exitosamente",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar usuario con ID {UserId}", id);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Error interno del servidor",
                Data = false
            };
        }
    }
}