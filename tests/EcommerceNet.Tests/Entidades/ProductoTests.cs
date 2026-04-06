using EcommerceNet.Core.Entidades;

namespace EcommerceNet.Tests.Entidades;

public class ProductoTests
{
    [Fact]
    public void TieneStockSuficiente_ConStock_Verdadero()
    {
        var p = new Producto { Stock = 10, Activo = true };
        Assert.True(p.TieneStockSuficiente(5));
    }

    [Fact]
    public void TieneStockSuficiente_SinStock_Falso()
    {
        var p = new Producto { Stock = 2, Activo = true };
        Assert.False(p.TieneStockSuficiente(5));
    }

    [Fact]
    public void TieneStockSuficiente_Inactivo_Falso()
    {
        var p = new Producto { Stock = 100, Activo = false };
        Assert.False(p.TieneStockSuficiente(1));
    }

    [Fact]
    public void ReducirStock_Suficiente_Reduce()
    {
        var p = new Producto { Stock = 10, Activo = true };
        p.ReducirStock(3);
        Assert.Equal(7, p.Stock);
    }

    [Fact]
    public void ReducirStock_Insuficiente_LanzaExcepcion()
    {
        var p = new Producto { Stock = 2, Activo = true, Nombre = "Test" };
        Assert.Throws<InvalidOperationException>(() => p.ReducirStock(5));
    }

    [Fact]
    public void AumentarStock_Positivo_Aumenta()
    {
        var p = new Producto { Stock = 5 };
        p.AumentarStock(10);
        Assert.Equal(15, p.Stock);
    }

    [Fact]
    public void AumentarStock_Cero_LanzaExcepcion()
    {
        var p = new Producto { Stock = 5 };
        Assert.Throws<ArgumentException>(() => p.AumentarStock(0));
    }
}
