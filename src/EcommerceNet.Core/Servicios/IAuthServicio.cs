using EcommerceNet.Core.DTOs;

namespace EcommerceNet.Core.Servicios;

/// <summary>
/// Contrato del servicio de autenticación.
/// Define las operaciones de registro y login sin conocer
/// los detalles de implementación (BCrypt, JWT, EF Core).
/// </summary>
public interface IAuthServicio
{
    /// <summary>Registrar un nuevo usuario. Retorna el token JWT si tiene éxito.</summary>
    Task<Resultado<AuthRespuestaDto>> RegistrarAsync(RegistroDto dto);

    /// <summary>Iniciar sesión con email y contraseña. Retorna el token JWT si tiene éxito.</summary>
    Task<Resultado<AuthRespuestaDto>> LoginAsync(LoginDto dto);
}
