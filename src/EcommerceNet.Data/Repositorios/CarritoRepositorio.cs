using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNet.Data.Repositorios;

/// <summary>
/// Repositorio del carrito — siempre carga Items y Productos (eager loading).
/// No hereda RepositorioBase porque solo necesita 3 métodos específicos.
/// </summary>
public class CarritoRepositorio : ICarritoRepositorio
{
    private readonly AppDbContext _contexto;

    public CarritoRepositorio(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    /// <summary>
    /// Obtener carrito con TODOS sus datos anidados usando Include + ThenInclude.
    ///
    /// SQL generado aproximado:
    /// SELECT * FROM Carritos c
    /// INNER JOIN CarritoItems ci ON ci.CarritoId = c.Id
    /// INNER JOIN Productos p ON p.Id = ci.ProductoId
    /// INNER JOIN Categorias cat ON cat.Id = p.CategoriaId
    /// WHERE c.UsuarioId = @usuarioId
    ///
    /// Include(c => c.Items)         = JOIN a CarritoItems
    ///   .ThenInclude(i => i.Producto)   = luego JOIN a Productos
    ///     .ThenInclude(p => p.Categoria) = luego JOIN a Categorias
    /// </summary>
    public async Task<Carrito?> ObtenerPorUsuarioAsync(int usuarioId)
    {
        return await _contexto.Carritos
            .Include(c => c.Items)
                .ThenInclude(i => i.Producto)
                    .ThenInclude(p => p!.Categoria)  // p! = p no es null (el compilador lo sabe por el Include)
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
    }

    public async Task AgregarAsync(Carrito carrito)
    {
        await _contexto.Carritos.AddAsync(carrito);
    }

    public void Actualizar(Carrito carrito)
    {
        _contexto.Carritos.Update(carrito);
    }
}
