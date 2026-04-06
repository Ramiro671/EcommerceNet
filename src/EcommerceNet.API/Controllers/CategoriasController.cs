using EcommerceNet.Core.DTOs;
using EcommerceNet.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNet.API.Controllers;

/// <summary>
/// Controlador de categorías.
/// Estructura completa — la implementación real se conectará con EF Core en el Día 3
/// cuando agreguemos ICategoriaRepositorio al sistema.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly IUnidadDeTrabajo _uow;

    public CategoriasController(IUnidadDeTrabajo uow)
    {
        _uow = uow;
    }

    /// <summary>
    /// Listar todas las categorías.
    /// TODO (Día 3): implementar con ICategoriaRepositorio real.
    /// </summary>
    [HttpGet]
    public IActionResult ObtenerTodas()
    {
        // Placeholder hasta que agreguemos ICategoriaRepositorio en Día 3
        return Ok(Resultado<string>.Ok("Endpoint pendiente — se implementa en Día 3 con EF Core"));
    }

    /// <summary>
    /// Crear una categoría nueva (solo Admin).
    /// TODO (Día 3): implementar con repositorio real.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult Crear([FromBody] CrearCategoriaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(Resultado<string>.Error("El nombre de la categoría es obligatorio"));

        // Placeholder hasta Día 3
        return Ok(Resultado<string>.Ok("Categoría creada (pendiente EF Core en Día 3)"));
    }
}

/// <summary>
/// DTO para crear una categoría.
/// Se moverá a Core/DTOs/ cuando se implemente con EF Core en Día 3.
/// </summary>
public class CrearCategoriaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}
