using System.Security.Claims;
using EcommerceNet.Core.DTOs;
using EcommerceNet.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNet.API.Controllers;

/// <summary>
/// Controlador de órdenes de compra.
/// Todos los endpoints requieren autenticación.
/// Un usuario solo puede ver y cancelar SUS PROPIAS órdenes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdenesController : ControllerBase
{
    private readonly IUnidadDeTrabajo _uow;

    public OrdenesController(IUnidadDeTrabajo uow)
    {
        _uow = uow;
    }

    /// <summary>Obtener el ID del usuario autenticado desde el token JWT</summary>
    private int ObtenerUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : 0;
    }

    /// <summary>Listar todas las órdenes del usuario autenticado</summary>
    [HttpGet]
    public async Task<IActionResult> MisOrdenes()
    {
        var usuarioId = ObtenerUsuarioId();
        var ordenes = await _uow.Ordenes.ObtenerPorUsuarioAsync(usuarioId);

        // Mapeo: entidades → DTOs (nunca exponer entidades directamente)
        var dtos = ordenes.Select(o => new OrdenDto
        {
            Id = o.Id,
            NumeroOrden = o.NumeroOrden,
            FechaCreacion = o.FechaCreacion,
            Estado = o.Estado.ToString(),
            Total = o.Total
        });

        return Ok(Resultado<IEnumerable<OrdenDto>>.Ok(dtos));
    }

    /// <summary>Obtener el detalle completo de una orden</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var orden = await _uow.Ordenes.ObtenerConDetallesAsync(id);

        if (orden == null)
            return NotFound(Resultado<OrdenDto>.Error("Orden no encontrada"));

        // Verificar que la orden pertenece al usuario autenticado
        // Si no es suya → 403 Forbidden (no 404, para no revelar que existe)
        if (orden.UsuarioId != ObtenerUsuarioId())
            return Forbid();

        var dto = new OrdenDto
        {
            Id = orden.Id,
            NumeroOrden = orden.NumeroOrden,
            FechaCreacion = orden.FechaCreacion,
            Estado = orden.Estado.ToString(),
            Total = orden.Total,
            Detalles = orden.Detalles.Select(d => new OrdenDetalleDto
            {
                ProductoNombre = d.Producto?.Nombre ?? "Producto eliminado",
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal
            }).ToList()
        };

        return Ok(Resultado<OrdenDto>.Ok(dto));
    }

    /// <summary>
    /// Cancelar una orden.
    /// Solo se pueden cancelar órdenes en estado Pendiente o Pagada.
    /// Al cancelar, el stock de los productos se restaura automáticamente.
    /// </summary>
    [HttpPut("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        // 🔴 BP-36: Request cancelar. Inspeccionar: id, ObtenerUsuarioId(), orden.Estado
        var orden = await _uow.Ordenes.ObtenerConDetallesAsync(id);

        if (orden == null)
            return NotFound(Resultado<bool>.Error("Orden no encontrada"));

        // 🔴 BP-37: ¿Orden pertenece al usuario? Inspeccionar: orden.UsuarioId vs ObtenerUsuarioId()
        // Seguridad: verificar que la orden pertenece al usuario autenticado
        if (orden.UsuarioId != ObtenerUsuarioId())
            return Forbid();

        try
        {
            // La lógica de cancelación vive en la entidad Orden (Día 1)
            // El controlador solo delega — no tiene lógica de negocio
            orden.Cancelar();  // ← F11 para entrar a BP-38 en Orden.cs
            _uow.Ordenes.Actualizar(orden);
            // 🔴 BP-40: Save cancelación. Inspeccionar: orden.Estado DESPUÉS (debe ser Cancelada)
            await _uow.GuardarCambiosAsync();

            return Ok(Resultado<bool>.Ok(true, "Orden cancelada. Stock restaurado."));
        }
        catch (InvalidOperationException ex)
        {
            // La entidad lanza esta excepción si el estado no permite cancelación
            return BadRequest(Resultado<bool>.Error(ex.Message));
        }
    }
}
