using EcommerceNet.Core.Entidades;

namespace EcommerceNet.Tests.Entidades;

public class CarritoTests
{
    private Producto CrearProducto(int id = 1, decimal precio = 100m, int stock = 10)
        => new() { Id = id, Nombre = $"Producto {id}", Precio = precio, Stock = stock, Activo = true };

    [Fact]
    public void AgregarProducto_Nuevo_SeAgrega()
    {
        var c = new Carrito();
        c.AgregarProducto(CrearProducto(), 2);
        Assert.Single(c.Items);
        Assert.Equal(2, c.Items[0].Cantidad);
    }

    [Fact]
    public void AgregarProducto_Existente_IncrementaCantidad()
    {
        var c = new Carrito();
        var p = CrearProducto();
        c.AgregarProducto(p, 2);
        c.AgregarProducto(p, 3);
        Assert.Single(c.Items);
        Assert.Equal(5, c.Items[0].Cantidad);
    }

    [Fact]
    public void AgregarProducto_SinStock_LanzaExcepcion()
    {
        var c = new Carrito();
        Assert.Throws<InvalidOperationException>(() =>
            c.AgregarProducto(CrearProducto(stock: 2), 5));
    }

    [Fact]
    public void CalcularTotal_VariosItems_SumaCorrecto()
    {
        var c = new Carrito();
        c.AgregarProducto(CrearProducto(1, 100m), 2);  // 200
        c.AgregarProducto(CrearProducto(2, 50m), 3);   // 150
        Assert.Equal(350m, c.CalcularTotal());
    }

    [Fact]
    public void TotalProductos_VariosItems_Correcto()
    {
        var c = new Carrito();
        c.AgregarProducto(CrearProducto(1), 2);
        c.AgregarProducto(CrearProducto(2), 3);
        Assert.Equal(5, c.TotalProductos());
    }

    [Fact]
    public void ActualizarCantidad_Existente_Actualiza()
    {
        var c = new Carrito();
        c.AgregarProducto(CrearProducto(), 2);
        c.ActualizarCantidad(1, 5);
        Assert.Equal(5, c.Items[0].Cantidad);
    }

    [Fact]
    public void ActualizarCantidad_Cero_EliminaItem()
    {
        var c = new Carrito();
        c.AgregarProducto(CrearProducto(), 2);
        c.ActualizarCantidad(1, 0);
        Assert.Empty(c.Items);
    }

    [Fact]
    public void EliminarProducto_Existente_Elimina()
    {
        var c = new Carrito();
        c.AgregarProducto(CrearProducto(1), 2);
        c.AgregarProducto(CrearProducto(2), 3);
        c.EliminarProducto(1);
        Assert.Single(c.Items);
    }

    [Fact]
    public void EliminarProducto_NoExiste_LanzaExcepcion()
    {
        var c = new Carrito();
        Assert.Throws<InvalidOperationException>(() => c.EliminarProducto(999));
    }

    [Fact]
    public void Vaciar_ConItems_QuedaVacio()
    {
        var c = new Carrito();
        c.AgregarProducto(CrearProducto(1), 2);
        c.AgregarProducto(CrearProducto(2), 3);
        c.Vaciar();
        Assert.True(c.EstaVacio());
    }
}
