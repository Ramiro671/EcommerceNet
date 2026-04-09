using EcommerceNet.Core.DTOs;
using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNet.API.Controllers;

/// <summary>
/// Controlador de categorías.
/// GET es público. POST, PUT, DELETE requieren JWT con rol Admin.
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

    /// <summary>Listar todas las categorías activas (público)</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerTodas()
    {
        var categorias = await _uow.Categorias.ObtenerActivasAsync();
        var dtos = categorias.Select(MapearADto);
        return Ok(Resultado<IEnumerable<CategoriaDto>>.Ok(dtos));
    }

    /// <summary>Listar TODAS las categorías incluyendo inactivas (Admin)</summary>
    [HttpGet("todas")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ObtenerTodasAdmin()
    {
        var categorias = await _uow.Categorias.ObtenerTodosAsync();
        var dtos = categorias.Select(MapearADto);
        return Ok(Resultado<IEnumerable<CategoriaDto>>.Ok(dtos));
    }

    /// <summary>Crear una categoría nueva (solo Admin)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear([FromBody] CrearCategoriaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(Resultado<CategoriaDto>.Error("El nombre de la categoría es obligatorio"));

        var categoria = new Categoria
        {
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim() ?? string.Empty,
            Activa = true
        };

        await _uow.Categorias.AgregarAsync(categoria);
        await _uow.GuardarCambiosAsync();

        return Ok(Resultado<CategoriaDto>.Ok(MapearADto(categoria), "Categoría creada exitosamente"));
    }

    /// <summary>Actualizar una categoría (solo Admin)</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] CrearCategoriaDto dto)
    {
        var categoria = await _uow.Categorias.ObtenerPorIdAsync(id);
        if (categoria == null)
            return NotFound(Resultado<CategoriaDto>.Error("Categoría no encontrada"));

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(Resultado<CategoriaDto>.Error("El nombre es obligatorio"));

        categoria.Nombre = dto.Nombre.Trim();
        categoria.Descripcion = dto.Descripcion?.Trim() ?? string.Empty;

        _uow.Categorias.Actualizar(categoria);
        await _uow.GuardarCambiosAsync();

        return Ok(Resultado<CategoriaDto>.Ok(MapearADto(categoria), "Categoría actualizada"));
    }

    /// <summary>Desactivar una categoría (soft delete, solo Admin)</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var categoria = await _uow.Categorias.ObtenerPorIdAsync(id);
        if (categoria == null)
            return NotFound(Resultado<bool>.Error("Categoría no encontrada"));

        categoria.Activa = false;
        _uow.Categorias.Actualizar(categoria);
        await _uow.GuardarCambiosAsync();

        return Ok(Resultado<bool>.Ok(true, "Categoría desactivada"));
    }

    private static CategoriaDto MapearADto(Categoria c) => new()
    {
        Id = c.Id,
        Nombre = c.Nombre,
        Descripcion = c.Descripcion,
        Activa = c.Activa
    };
}
