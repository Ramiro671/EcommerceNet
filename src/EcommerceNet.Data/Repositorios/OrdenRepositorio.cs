using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNet.Data.Repositorios;

/// <summary>
/// Repositorio de órdenes — consultas especializadas con Include.
/// </summary>
public class OrdenRepositorio : RepositorioBase<Orden>, IOrdenRepositorio
{
    public OrdenRepositorio(AppDbContext contexto) : base(contexto) { }

    /// <summary>
    /// Historial de órdenes de un usuario, ordenado más reciente primero.
    /// </summary>
    public async Task<IEnumerable<Orden>> ObtenerPorUsuarioAsync(int usuarioId)
    {
        return await _contexto.Ordenes
            .Where(o => o.UsuarioId == usuarioId)
            .OrderByDescending(o => o.FechaCreacion)
            .ToListAsync();
    }

    /// <summary>
    /// Orden con todos sus detalles y productos — para la pantalla de detalle.
    /// </summary>
    public async Task<Orden?> ObtenerConDetallesAsync(int ordenId)
    {
        return await _contexto.Ordenes
            .Include(o => o.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(o => o.Usuario)
            .FirstOrDefaultAsync(o => o.Id == ordenId);
    }
}
