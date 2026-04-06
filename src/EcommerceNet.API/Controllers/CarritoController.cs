using System.Security.Claims;
using EcommerceNet.Core.DTOs;
using EcommerceNet.Core.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNet.API.Controllers;

/// <summary>
/// Controlador del carrito de compras.
/// TODOS los endpoints requieren autenticación JWT.
/// La lógica de negocio está en CarritoServicio — el controlador SOLO traduce HTTP.
/// Responsabilidad del controlador: extraer datos del request → llamar servicio → devolver HTTP code.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]  // aplica [Authorize] a todos los métodos del controlador
public class CarritoController : ControllerBase
{
    private readonly ICarritoServicio _carritoServicio;

    public CarritoController(ICarritoServicio carritoServicio)
    {
        _carritoServicio = carritoServicio;
    }

    /// <summary>
    /// Extrae el ID del usuario autenticado desde los claims del token JWT.
    /// El claim NameIdentifier contiene el ID que se puso al generar el token en AuthServicio.
    /// </summary>
    private int ObtenerUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");

        if (claim == null || !int.TryParse(claim.Value, out var id))
            throw new UnauthorizedAccessException("Token inválido — no contiene el ID de usuario");

        return id;
    }

    /// <summary>Ver el carrito del usuario autenticado</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerCarrito()
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _carritoServicio.ObtenerCarritoAsync(usuarioId);
        return Ok(resultado);
    }

    /// <summary>Agregar un producto al carrito</summary>
    [HttpPost("agregar")]
    public async Task<IActionResult> Agregar([FromBody] AgregarAlCarritoDto dto)
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _carritoServicio.AgregarProductoAsync(usuarioId, dto);

        if (!resultado.Exito)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    /// <summary>Actualizar la cantidad de un producto en el carrito</summary>
    [HttpPut("{productoId}")]
    public async Task<IActionResult> ActualizarCantidad(
        int productoId, [FromBody] ActualizarCantidadDto dto)
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _carritoServicio.ActualizarCantidadAsync(
            usuarioId, productoId, dto.Cantidad);

        if (!resultado.Exito)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    /// <summary>Quitar un producto específico del carrito</summary>
    [HttpDelete("{productoId}")]
    public async Task<IActionResult> EliminarProducto(int productoId)
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _carritoServicio.EliminarProductoAsync(usuarioId, productoId);

        if (!resultado.Exito)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    /// <summary>Vaciar el carrito completamente</summary>
    [HttpDelete]
    public async Task<IActionResult> Vaciar()
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _carritoServicio.VaciarCarritoAsync(usuarioId);
        return Ok(resultado);
    }

    /// <summary>
    /// Checkout — procesar la compra.
    /// Crea la orden, reduce el stock de cada producto y vacía el carrito.
    /// </summary>
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CrearOrdenDto dto)
    {
        var usuarioId = ObtenerUsuarioId();
        var resultado = await _carritoServicio.CheckoutAsync(usuarioId, dto);

        if (!resultado.Exito)
            return BadRequest(resultado);

        return Ok(resultado);
    }
}

/// <summary>DTO auxiliar para actualizar la cantidad de un producto en el carrito</summary>
public class ActualizarCantidadDto
{
    public int Cantidad { get; set; }
}
