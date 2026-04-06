using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNet.Data.Repositorios;

/// <summary>
/// Repositorio de usuarios.
/// Implementa IUsuarioRepositorio definido en Core desde el Día 2.
/// Agrega búsqueda por email al CRUD genérico de RepositorioBase.
/// </summary>
public class UsuarioRepositorio : RepositorioBase<Usuario>, IUsuarioRepositorio
{
    public UsuarioRepositorio(AppDbContext contexto) : base(contexto) { }

    /// <summary>
    /// Buscar usuario por email — usado en login y validación de registro.
    /// El índice UNIQUE en Email garantiza que este query sea rápido.
    /// EF genera: SELECT TOP 1 * FROM Usuarios WHERE Email = @email
    /// </summary>
    public async Task<Usuario?> BuscarPorEmailAsync(string email)
    {
        return await _contexto.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}
