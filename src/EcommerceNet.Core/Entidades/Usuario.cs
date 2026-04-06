using EcommerceNet.Core.Enums;

namespace EcommerceNet.Core.Entidades;

/// <summary>
/// Usuario de la tienda. Puede ser Cliente o Admin.
/// </summary>
public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // NUNCA texto plano
    public RolUsuario Rol { get; set; } = RolUsuario.Cliente;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Relaciones
    public Carrito? Carrito { get; set; }
    public List<Orden> Ordenes { get; set; } = new();

    public bool EsAdmin() => Rol == RolUsuario.Admin;
}
