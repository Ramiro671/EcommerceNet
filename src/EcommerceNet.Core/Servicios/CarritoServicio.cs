using EcommerceNet.Core.DTOs;
using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Enums;
using EcommerceNet.Core.Interfaces;

namespace EcommerceNet.Core.Servicios;

/// <summary>
/// Servicio de carrito — toda la lógica de negocio:
/// agregar, quitar, actualizar y el proceso de checkout completo.
/// </summary>
public class CarritoServicio : ICarritoServicio
{
    private readonly IUnidadDeTrabajo _uow;

    public CarritoServicio(IUnidadDeTrabajo uow)
    {
        _uow = uow;
    }

    public async Task<Resultado<CarritoDto>> ObtenerCarritoAsync(int usuarioId)
    {
        var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
        if (carrito == null) return Resultado<CarritoDto>.Ok(new CarritoDto());
        return Resultado<CarritoDto>.Ok(MapearCarrito(carrito));
    }

    public async Task<Resultado<CarritoDto>> AgregarProductoAsync(
        int usuarioId, AgregarAlCarritoDto dto)
    {
        var producto = await _uow.Productos.ObtenerPorIdAsync(dto.ProductoId);
        if (producto == null)
            return Resultado<CarritoDto>.Error("Producto no encontrado");
        if (!producto.TieneStockSuficiente(dto.Cantidad))
            return Resultado<CarritoDto>.Error($"Stock insuficiente. Disponible: {producto.Stock}");

        var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
        if (carrito == null)
        {
            carrito = new Carrito { UsuarioId = usuarioId };
            await _uow.Carritos.AgregarAsync(carrito);
        }

        carrito.AgregarProducto(producto, dto.Cantidad);
        _uow.Carritos.Actualizar(carrito);
        await _uow.GuardarCambiosAsync();

        return Resultado<CarritoDto>.Ok(MapearCarrito(carrito), $"'{producto.Nombre}' agregado");
    }

    public async Task<Resultado<CarritoDto>> ActualizarCantidadAsync(
        int usuarioId, int productoId, int cantidad)
    {
        var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
        if (carrito == null) return Resultado<CarritoDto>.Error("Carrito no encontrado");

        try
        {
            carrito.ActualizarCantidad(productoId, cantidad);
            _uow.Carritos.Actualizar(carrito);
            await _uow.GuardarCambiosAsync();
            return Resultado<CarritoDto>.Ok(MapearCarrito(carrito));
        }
        catch (InvalidOperationException ex)
        {
            return Resultado<CarritoDto>.Error(ex.Message);
        }
    }

    public async Task<Resultado<CarritoDto>> EliminarProductoAsync(
        int usuarioId, int productoId)
    {
        var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
        if (carrito == null) return Resultado<CarritoDto>.Error("Carrito no encontrado");

        try
        {
            carrito.EliminarProducto(productoId);
            _uow.Carritos.Actualizar(carrito);
            await _uow.GuardarCambiosAsync();
            return Resultado<CarritoDto>.Ok(MapearCarrito(carrito), "Producto eliminado");
        }
        catch (InvalidOperationException ex)
        {
            return Resultado<CarritoDto>.Error(ex.Message);
        }
    }

    public async Task<Resultado<CarritoDto>> VaciarCarritoAsync(int usuarioId)
    {
        var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
        if (carrito == null) return Resultado<CarritoDto>.Error("Carrito no encontrado");

        carrito.Vaciar();
        _uow.Carritos.Actualizar(carrito);
        await _uow.GuardarCambiosAsync();
        return Resultado<CarritoDto>.Ok(new CarritoDto(), "Carrito vaciado");
    }

    /// <summary>
    /// CHECKOUT — el proceso más crítico:
    /// 1. Validar carrito no vacío
    /// 2. Verificar stock de cada producto
    /// 3. Crear orden con detalles
    /// 4. Reducir stock
    /// 5. Vaciar carrito
    /// Todo en UNA transacción (si falla uno, falla todo)
    /// </summary>
    public async Task<Resultado<OrdenDto>> CheckoutAsync(
        int usuarioId, CrearOrdenDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DireccionEnvio))
            return Resultado<OrdenDto>.Error("La dirección de envío es obligatoria");

        var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
        if (carrito == null || carrito.EstaVacio())
            return Resultado<OrdenDto>.Error("El carrito está vacío");

        // Verificar stock de cada producto
        var errores = new List<string>();
        foreach (var item in carrito.Items)
        {
            var prod = await _uow.Productos.ObtenerPorIdAsync(item.ProductoId);
            if (prod == null || !prod.TieneStockSuficiente(item.Cantidad))
                errores.Add($"'{item.Producto?.Nombre}': stock insuficiente");
        }
        if (errores.Count > 0)
            return Resultado<OrdenDto>.ErrorValidacion(errores);

        // Crear la orden
        var orden = new Orden
        {
            UsuarioId = usuarioId,
            DireccionEnvio = dto.DireccionEnvio,
            Estado = EstadoOrden.Pendiente
        };

        foreach (var item in carrito.Items)
        {
            var prod = await _uow.Productos.ObtenerPorIdAsync(item.ProductoId);
            if (prod == null) continue;

            var detalle = new OrdenDetalle
            {
                ProductoId = item.ProductoId,
                Producto = prod,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.PrecioUnitario
            };
            detalle.CalcularSubtotal();
            orden.Detalles.Add(detalle);

            prod.ReducirStock(item.Cantidad);
            _uow.Productos.Actualizar(prod);
        }

        orden.RecalcularTotal();
        await _uow.Ordenes.AgregarAsync(orden);

        carrito.Vaciar();
        _uow.Carritos.Actualizar(carrito);

        await _uow.GuardarCambiosAsync();

        orden.GenerarNumeroOrden();
        _uow.Ordenes.Actualizar(orden);
        await _uow.GuardarCambiosAsync();

        return Resultado<OrdenDto>.Ok(MapearOrden(orden), $"Orden {orden.NumeroOrden} creada");
    }

    // --- Mapeos privados ---

    private static CarritoDto MapearCarrito(Carrito c) => new()
    {
        Id = c.Id,
        Total = c.CalcularTotal(),
        TotalProductos = c.TotalProductos(),
        Items = c.Items.Select(i => new CarritoItemDto
        {
            ProductoId = i.ProductoId,
            ProductoNombre = i.Producto?.Nombre ?? "",
            ImagenUrl = i.Producto?.ImagenUrl ?? "",
            PrecioUnitario = i.PrecioUnitario,
            Cantidad = i.Cantidad,
            Subtotal = i.CalcularSubtotal()
        }).ToList()
    };

    private static OrdenDto MapearOrden(Orden o) => new()
    {
        Id = o.Id,
        NumeroOrden = o.NumeroOrden,
        FechaCreacion = o.FechaCreacion,
        Estado = o.Estado.ToString(),
        Total = o.Total,
        Detalles = o.Detalles.Select(d => new OrdenDetalleDto
        {
            ProductoNombre = d.Producto?.Nombre ?? "",
            Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario,
            Subtotal = d.Subtotal
        }).ToList()
    };
}
