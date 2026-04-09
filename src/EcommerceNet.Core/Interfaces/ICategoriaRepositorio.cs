namespace EcommerceNet.Core.Interfaces;

/// <summary>Repositorio de categorías — hereda CRUD genérico</summary>
public interface ICategoriaRepositorio : IRepositorio<Core.Entidades.Categoria>
{
    Task<IEnumerable<Core.Entidades.Categoria>> ObtenerActivasAsync();
}
