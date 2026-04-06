using EcommerceNet.Core.Entidades;

namespace EcommerceNet.Core.Interfaces;

/// <summary>
/// Repositorio de usuarios.
/// Extiende el repositorio genérico con operaciones específicas de usuarios.
/// </summary>
public interface IUsuarioRepositorio : IRepositorio<Usuario>
{
    /// <summary>Buscar un usuario por su email (único en el sistema)</summary>
    Task<Usuario?> BuscarPorEmailAsync(string email);
}
