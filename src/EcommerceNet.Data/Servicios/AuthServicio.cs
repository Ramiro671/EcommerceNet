using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EcommerceNet.Core.DTOs;
using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Servicios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EcommerceNet.Data.Servicios;

/// <summary>
/// Servicio de autenticación con acceso directo a la BD (AppDbContext).
/// Va en Data (no en Core) porque necesita EF Core y BCrypt, ambos paquetes externos.
/// Va en Data (no en API) porque encapsula lógica de persistencia de usuarios.
///
/// Registro: hashea password con BCrypt y guarda usuario en SQL Server.
/// Login: busca usuario por email, verifica password y genera token JWT.
/// </summary>
public class AuthServicio : IAuthServicio
{
    private readonly AppDbContext _contexto;
    private readonly IConfiguration _config;

    public AuthServicio(AppDbContext contexto, IConfiguration config)
    {
        _contexto = contexto;
        _config = config;
    }

    public async Task<Resultado<AuthRespuestaDto>> RegistrarAsync(RegistroDto dto)
    {
        // 🔴 BP-03: ¿Email ya existe? Inspeccionar: existe (bool), dto.Email
        // Verificar que el email no esté registrado
        var existe = await _contexto.Usuarios.AnyAsync(u => u.Email == dto.Email);
        if (existe)
            return Resultado<AuthRespuestaDto>.Error("Ya existe un usuario con ese email");

        // Crear usuario con password hasheado — NUNCA guardar en texto plano
        var usuario = new Usuario
        {
            Nombre = dto.Nombre,
            Email = dto.Email,
            // 🔴 BP-04: Hash generado. Inspeccionar: PasswordHash (comparar con dto.Password — son distintos)
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        await _contexto.Usuarios.AddAsync(usuario);
        // 🔴 BP-05: Guardar usuario en BD. Inspeccionar: usuario.Id (debe ser 0 antes, >0 después)
        await _contexto.SaveChangesAsync();

        // Generar JWT y retornar
        var respuesta = GenerarJwt(usuario);
        return Resultado<AuthRespuestaDto>.Ok(respuesta, "Usuario registrado exitosamente");
    }

    public async Task<Resultado<AuthRespuestaDto>> LoginAsync(LoginDto dto)
    {
        // 🔴 BP-07: Buscar usuario por email. Inspeccionar: usuario (null = no existe)
        // Buscar usuario — mensaje genérico para evitar enumeración de usuarios
        var usuario = await _contexto.Usuarios
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (usuario == null)
            return Resultado<AuthRespuestaDto>.Error("Credenciales incorrectas");

        // 🔴 BP-08: Verificación password. Inspeccionar: resultado de Verify (true/false)
        // BCrypt.Verify compara el password ingresado contra el hash almacenado
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            return Resultado<AuthRespuestaDto>.Error("Credenciales incorrectas");

        var respuesta = GenerarJwt(usuario);
        return Resultado<AuthRespuestaDto>.Ok(respuesta, "Login exitoso");
    }

    /// <summary>
    /// Genera un token JWT firmado con la clave secreta del servidor.
    /// El token contiene claims (datos) del usuario que se pueden leer en los controladores.
    /// </summary>
    private AuthRespuestaDto GenerarJwt(Usuario usuario)
    {
        // 🔴 BP-09: Claims del JWT. Inspeccionar: claims[] (NameIdentifier, Name, Email, Role)
        // Claims = datos que se incrustan dentro del token
        // El receptor puede leerlos sin consultar la BD (stateless)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),  // ID del usuario
            new Claim(ClaimTypes.Name, usuario.Nombre),                   // Nombre
            new Claim(ClaimTypes.Email, usuario.Email),                   // Email
            new Claim(ClaimTypes.Role, usuario.Rol.ToString())            // Rol: Admin / Cliente
        };

        // Clave secreta para firmar el token (debe tener al menos 256 bits = 32 chars)
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiraMinutos = int.Parse(_config["Jwt:ExpireMinutes"] ?? "60");
        var expira = DateTime.UtcNow.AddMinutes(expiraMinutos);

        // Construir el token con todos los parámetros
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expira,
            signingCredentials: credenciales);

        // 🔴 BP-10: Token generado. Inspeccionar: token string (pegar en jwt.io), expira DateTime
        return new AuthRespuestaDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Nombre = usuario.Nombre,
            Email = usuario.Email,
            Rol = usuario.Rol.ToString(),
            Expira = expira
        };
    }
}
