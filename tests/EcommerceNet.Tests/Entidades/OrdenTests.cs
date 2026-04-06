using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Enums;

namespace EcommerceNet.Tests.Entidades;

public class OrdenTests
{
    [Fact]
    public void RecalcularTotal_SumaDetalles()
    {
        var o = new Orden { Detalles = new()
        {
            new() { Subtotal = 200m },
            new() { Subtotal = 150m }
        }};
        o.RecalcularTotal();
        Assert.Equal(350m, o.Total);
    }

    [Fact]
    public void SePuedeCancelar_Pendiente_Verdadero()
    {
        Assert.True(new Orden { Estado = EstadoOrden.Pendiente }.SePuedeCancelar());
    }

    [Fact]
    public void SePuedeCancelar_Enviada_Falso()
    {
        Assert.False(new Orden { Estado = EstadoOrden.Enviada }.SePuedeCancelar());
    }

    [Fact]
    public void Cancelar_Pendiente_DevuelveStock()
    {
        var prod = new Producto { Stock = 5, Activo = true };
        var o = new Orden
        {
            Estado = EstadoOrden.Pendiente,
            Detalles = new() { new() { Cantidad = 3, Producto = prod } }
        };
        o.Cancelar();
        Assert.Equal(EstadoOrden.Cancelada, o.Estado);
        Assert.Equal(8, prod.Stock);
    }

    [Fact]
    public void Cancelar_Enviada_LanzaExcepcion()
    {
        var o = new Orden { Estado = EstadoOrden.Enviada };
        Assert.Throws<InvalidOperationException>(() => o.Cancelar());
    }
}
