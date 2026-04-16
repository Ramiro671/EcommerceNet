using EcommerceNet.Core.DTOs;
using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Interfaces;
using EcommerceNet.Data.MongoDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceNet.API.Controllers;

/// <summary>
/// Controlador de productos.
/// GET endpoints son públicos.
/// POST, PUT, DELETE requieren JWT con rol Admin.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IUnidadDeTrabajo _uow;
    private readonly HistorialBusquedaServicio _historial;

    public ProductosController(IUnidadDeTrabajo uow, HistorialBusquedaServicio historial)
    {
        // 🔴 BP-11: DI — UoW e historial inyectados. Inspeccionar: uow (no debe ser null)
        _uow = uow;
        _historial = historial;
    }

    // ======================================================
    // ENDPOINTS PÚBLICOS (no requieren token)
    // ======================================================

    /// <summary>Obtener todos los productos activos</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        // 🔴 BP-12: Lista productos. Inspeccionar: productos.Count(), si Categoria está cargada
        var productos = await _uow.Productos.ObtenerActivosAsync();
        var dtos = productos.Select(MapearADto);
        return Ok(Resultado<IEnumerable<ProductoDto>>.Ok(dtos));
    }

    /// <summary>Obtener un producto por su ID</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var producto = await _uow.Productos.ObtenerPorIdAsync(id);

        if (producto == null)
            return NotFound(Resultado<ProductoDto>.Error("Producto no encontrado"));

        return Ok(Resultado<ProductoDto>.Ok(MapearADto(producto)));
    }

    /// <summary>Buscar productos por nombre — registra búsqueda en MongoDB</summary>
    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar([FromQuery] string termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
            return BadRequest(Resultado<string>.Error("El término de búsqueda es obligatorio"));

        var productos = await _uow.Productos.BuscarPorNombreAsync(termino);
        var lista = productos.ToList();

        // Registrar búsqueda en MongoDB (historial analytics)
        // ObtenerUsuarioId() puede ser null si el usuario no está autenticado
        var usuarioId = ObtenerUsuarioId();
        // Fire-and-forget intencionalmente — no queremos que un fallo de MongoDB
        // bloquee la respuesta al usuario
        _ = _historial.RegistrarBusquedaAsync(termino, usuarioId, lista.Count);

        var dtos = lista.Select(MapearADto);
        return Ok(Resultado<IEnumerable<ProductoDto>>.Ok(dtos));
    }

    /// <summary>Obtener productos por categoría</summary>
    [HttpGet("categoria/{categoriaId}")]
    public async Task<IActionResult> ObtenerPorCategoria(int categoriaId)
    {
        var productos = await _uow.Productos.ObtenerPorCategoriaAsync(categoriaId);
        var dtos = productos.Select(MapearADto);
        return Ok(Resultado<IEnumerable<ProductoDto>>.Ok(dtos));
    }

    // ======================================================
    // ENDPOINTS DE ADMINISTRACIÓN (requieren JWT con rol Admin)
    // ======================================================

    /// <summary>Crear un producto nuevo (solo Admin)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Crear([FromBody] CrearProductoDto dto)
    {
        // 🔴 BP-14: Crear producto. Inspeccionar: User.Claims, User.IsInRole("Admin"), dto
        // 🔴 BP-15: Validaciones. Inspeccionar: errores list, dto.Nombre, dto.Precio, dto.Stock
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            errores.Add("El nombre es obligatorio");
        if (dto.Precio <= 0)
            errores.Add("El precio debe ser mayor a cero");
        if (dto.Stock < 0)
            errores.Add("El stock no puede ser negativo");
        if (dto.CategoriaId <= 0)
            errores.Add("Debe especificar una categoría válida");

        if (errores.Count > 0)
            return BadRequest(Resultado<ProductoDto>.ErrorValidacion(errores));

        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            Stock = dto.Stock,
            ImagenUrl = dto.ImagenUrl,
            CategoriaId = dto.CategoriaId
        };

        await _uow.Productos.AgregarAsync(producto);
        await _uow.GuardarCambiosAsync();

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = producto.Id },
            Resultado<ProductoDto>.Ok(MapearADto(producto), "Producto creado exitosamente"));
    }

    /// <summary>Actualizar un producto existente (solo Admin)</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] CrearProductoDto dto)
    {
        var producto = await _uow.Productos.ObtenerPorIdAsync(id);

        if (producto == null)
            return NotFound(Resultado<ProductoDto>.Error("Producto no encontrado"));

        producto.Nombre = dto.Nombre;
        producto.Descripcion = dto.Descripcion;
        producto.Precio = dto.Precio;
        producto.Stock = dto.Stock;
        producto.ImagenUrl = dto.ImagenUrl;
        producto.CategoriaId = dto.CategoriaId;

        _uow.Productos.Actualizar(producto);
        await _uow.GuardarCambiosAsync();

        return Ok(Resultado<ProductoDto>.Ok(MapearADto(producto), "Producto actualizado"));
    }

    /// <summary>Eliminar un producto (solo Admin)</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var producto = await _uow.Productos.ObtenerPorIdAsync(id);

        if (producto == null)
            return NotFound(Resultado<bool>.Error("Producto no encontrado"));

        _uow.Productos.Eliminar(producto);
        await _uow.GuardarCambiosAsync();

        return Ok(Resultado<bool>.Ok(true, "Producto eliminado exitosamente"));
    }

    // ======================================================
    // MÉTODOS PRIVADOS
    // ======================================================

    /// <summary>Obtener el ID del usuario autenticado (null si no hay token)</summary>
    private int? ObtenerUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    // 🔴 BP-18: Mapeo entidad→DTO. Inspeccionar: p (entidad) vs resultado (DTO — qué campos se exponen)
    private static ProductoDto MapearADto(Producto p) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Descripcion = p.Descripcion,
        Precio = p.Precio,
        Stock = p.Stock,
        ImagenUrl = p.ImagenUrl,
        CategoriaNombre = p.Categoria?.Nombre ?? "Sin categoría",
        Disponible = p.Activo && p.Stock > 0
    };
}
