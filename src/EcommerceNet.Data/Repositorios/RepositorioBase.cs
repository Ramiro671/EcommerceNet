using EcommerceNet.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNet.Data.Repositorios;

/// <summary>
/// Implementación genérica del CRUD con EF Core.
/// Cualquier repositorio concreto hereda estos métodos base.
/// La palabra clave virtual permite que los repositorios concretos sobreescriban
/// métodos para agregar Include() u otras consultas especializadas.
/// </summary>
public class RepositorioBase<T> : IRepositorio<T> where T : class
{
    // protected = accesible por esta clase y sus subclases (no desde fuera)
    protected readonly AppDbContext _contexto;
    protected readonly DbSet<T> _dbSet;

    public RepositorioBase(AppDbContext contexto)
    {
        _contexto = contexto;
        _dbSet = contexto.Set<T>();  // Set<T>() obtiene el DbSet<T> correspondiente
    }

    /// <summary>
    /// Buscar por PK — EF primero revisa el caché local (1er nivel), luego va a la BD.
    /// virtual = puede ser sobreescrito en subclases para agregar Include().
    /// </summary>
    public virtual async Task<T?> ObtenerPorIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Obtener todos los registros sin filtros.
    /// virtual = puede ser sobreescrito (ej: filtrar solo activos).
    /// </summary>
    public virtual async Task<IEnumerable<T>> ObtenerTodosAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>Agregar un nuevo registro (no guarda hasta SaveChanges)</summary>
    public async Task AgregarAsync(T entidad)
    {
        // 🔴 BP-16: EF Core Add. Inspeccionar: entidad (Id=0 antes de SaveChanges, >0 después)
        await _dbSet.AddAsync(entidad);
    }

    /// <summary>Marcar como modificado (EF genera UPDATE en SaveChanges)</summary>
    public void Actualizar(T entidad)
    {
        _dbSet.Update(entidad);
    }

    /// <summary>Marcar como eliminado (EF genera DELETE en SaveChanges)</summary>
    public void Eliminar(T entidad)
    {
        _dbSet.Remove(entidad);
    }
}
