namespace EcommerceNet.Core.DTOs;

/// <summary>Datos necesarios para registrar un nuevo usuario</summary>
public class RegistroDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>Datos necesarios para iniciar sesión</summary>
public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>Respuesta del login — contiene el token JWT y los datos del usuario</summary>
public class AuthRespuestaDto
{
    public string Token { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
}
