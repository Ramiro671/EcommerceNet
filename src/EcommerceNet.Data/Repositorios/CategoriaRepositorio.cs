using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNet.Data.Repositorios;

/// <summary>Repositorio de categorías con EF Core</summary>
public class CategoriaRepositorio : RepositorioBase<Categoria>, ICategoriaRepositorio
{
    public CategoriaRepositorio(AppDbContext contexto) : base(contexto) { }

    public async Task<IEnumerable<Categoria>> ObtenerActivasAsync()
    {
        return await _contexto.Categorias
            .Where(c => c.Activa)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }
}
