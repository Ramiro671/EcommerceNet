using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNet.Data.Repositorios;

/// <summary>
/// Repositorio de productos — queries especializados con LINQ + EF Core.
/// Hereda CRUD genérico de RepositorioBase y agrega consultas propias.
/// Cada método con Include() genera un JOIN en SQL automáticamente.
/// </summary>
public class ProductoRepositorio : RepositorioBase<Producto>, IProductoRepositorio
{
    public ProductoRepositorio(AppDbContext contexto) : base(contexto) { }

    /// <summary>
    /// Override del base — agrega Include para que cada producto traiga su categoría.
    /// Sin Include, producto.Categoria sería null (lazy loading desactivado por defecto).
    /// EF genera: SELECT ... FROM Productos INNER JOIN Categorias ON ...
    /// </summary>
    public override async Task<Producto?> ObtenerPorIdAsync(int id)
    {
        return await _contexto.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Búsqueda por nombre — EF genera: WHERE Nombre LIKE '%termino%'
    /// Solo productos activos, ordenados alfabéticamente.
    /// </summary>
    public async Task<IEnumerable<Producto>> BuscarPorNombreAsync(string termino)
    {
        return await _contexto.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo && p.Nombre.Contains(termino))
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    /// <summary>
    /// Productos de una categoría específica, ordenados por precio.
    /// </summary>
    public async Task<IEnumerable<Producto>> ObtenerPorCategoriaAsync(int categoriaId)
    {
        return await _contexto.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo && p.CategoriaId == categoriaId)
            .OrderBy(p => p.Precio)
            .ToListAsync();
    }

    /// <summary>
    /// Productos con stock bajo — útil para alertas de reabastecimiento.
    /// </summary>
    public async Task<IEnumerable<Producto>> ObtenerConStockBajoAsync(int minimo = 5)
    {
        return await _contexto.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo && p.Stock < minimo)
            .OrderBy(p => p.Stock)
            .ToListAsync();
    }

    /// <summary>
    /// Todos los productos activos con categoría — ordenados por fecha (más nuevos primero).
    /// Usado por el endpoint GET /api/productos.
    /// </summary>
    public async Task<IEnumerable<Producto>> ObtenerActivosAsync()
    {
        // 🔴 BP-13: Query EF Core. Ver SQL en Output→Debug (habilitar LogTo). Inspeccionar: resultado count
        return await _contexto.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .ToListAsync();
    }
}
