using EcommerceNet.Core.DTOs;
using EcommerceNet.Core.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNet.API.Controllers;

/// <summary>
/// Controlador de autenticación.
/// Endpoints públicos — no requieren token JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthServicio _authServicio;

    public AuthController(IAuthServicio authServicio)
    {
        _authServicio = authServicio;
    }

    /// <summary>
    /// Registrar un nuevo usuario en el sistema.
    /// Devuelve el token JWT si el registro es exitoso.
    /// </summary>
    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistroDto dto)
    {
        var resultado = await _authServicio.RegistrarAsync(dto);

        if (!resultado.Exito)
            return BadRequest(resultado);  // 400 — email ya registrado u otro error

        return Ok(resultado);  // 200 + token JWT
    }

    /// <summary>
    /// Iniciar sesión con email y contraseña.
    /// Devuelve el token JWT si las credenciales son correctas.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var resultado = await _authServicio.LoginAsync(dto);

        if (!resultado.Exito)
            return Unauthorized(resultado);  // 401 — credenciales incorrectas

        return Ok(resultado);  // 200 + token JWT
    }
}
