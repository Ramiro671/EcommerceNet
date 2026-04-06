using EcommerceNet.Core.Entidades;
using EcommerceNet.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNet.Data;

/// <summary>
/// Contexto de base de datos — la puerta de entrada a SQL Server.
/// Cada DbSet<T> se convierte en una tabla.
/// Se registra como Scoped: una instancia por petición HTTP.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Cada DbSet<T> = una tabla en la BD
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Carrito> Carritos => Set<Carrito>();
    public DbSet<CarritoItem> CarritoItems => Set<CarritoItem>();
    public DbSet<Orden> Ordenes => Set<Orden>();
    public DbSet<OrdenDetalle> OrdenDetalles => Set<OrdenDetalle>();

    /// <summary>
    /// Configuración de las tablas: relaciones, índices, tipos de columna, seed data.
    /// EF Core lee esto al crear las migraciones y al inicializar el modelo.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurarCategoria(modelBuilder);
        ConfigurarProducto(modelBuilder);
        ConfigurarUsuario(modelBuilder);
        ConfigurarCarrito(modelBuilder);
        ConfigurarCarritoItem(modelBuilder);
        ConfigurarOrden(modelBuilder);
        ConfigurarOrdenDetalle(modelBuilder);

        AgregarDatosIniciales(modelBuilder);
    }

    // ------------------------------------------------------------------
    // CONFIGURACIONES FLUENT API
    // ------------------------------------------------------------------

    private static void ConfigurarCategoria(ModelBuilder mb)
    {
        mb.Entity<Categoria>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
            e.Property(c => c.Descripcion).HasMaxLength(500);
        });
    }

    private static void ConfigurarProducto(ModelBuilder mb)
    {
        mb.Entity<Producto>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
            e.Property(p => p.Descripcion).HasMaxLength(1000);
            // decimal(18,2) = hasta 16 dígitos enteros + 2 decimales (standard para precios)
            e.Property(p => p.Precio).HasColumnType("decimal(18,2)");
            e.Property(p => p.ImagenUrl).HasMaxLength(500);

            // Índice en Nombre para búsquedas por LIKE rápidas
            e.HasIndex(p => p.Nombre);

            // Relación: Producto pertenece a Categoria
            // Restrict = no se puede borrar la categoría si tiene productos
            e.HasOne(p => p.Categoria)
             .WithMany(c => c.Productos)
             .HasForeignKey(p => p.CategoriaId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurarUsuario(ModelBuilder mb)
    {
        mb.Entity<Usuario>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.Property(u => u.PasswordHash).IsRequired();

            // Email único — no puede haber dos usuarios con el mismo email
            e.HasIndex(u => u.Email).IsUnique();
        });
    }

    private static void ConfigurarCarrito(ModelBuilder mb)
    {
        mb.Entity<Carrito>(e =>
        {
            e.HasKey(c => c.Id);

            // Relación 1:1 — un usuario tiene un solo carrito
            // Cascade = borrar carrito automáticamente si se borra el usuario
            e.HasOne(c => c.Usuario)
             .WithOne(u => u.Carrito)
             .HasForeignKey<Carrito>(c => c.UsuarioId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurarCarritoItem(ModelBuilder mb)
    {
        mb.Entity<CarritoItem>(e =>
        {
            e.HasKey(ci => ci.Id);
            e.Property(ci => ci.PrecioUnitario).HasColumnType("decimal(18,2)");

            // Relación: CarritoItem pertenece a Carrito (Cascade = borrar items si se borra el carrito)
            e.HasOne(ci => ci.Carrito)
             .WithMany(c => c.Items)
             .HasForeignKey(ci => ci.CarritoId)
             .OnDelete(DeleteBehavior.Cascade);

            // Relación: CarritoItem referencia un Producto (Restrict = no borrar producto si está en un carrito)
            e.HasOne(ci => ci.Producto)
             .WithMany()
             .HasForeignKey(ci => ci.ProductoId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurarOrden(ModelBuilder mb)
    {
        mb.Entity<Orden>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.NumeroOrden).HasMaxLength(50);
            e.Property(o => o.Total).HasColumnType("decimal(18,2)");
            e.Property(o => o.DireccionEnvio).IsRequired().HasMaxLength(500);

            // Índice para buscar órdenes por número
            e.HasIndex(o => o.NumeroOrden);

            // Relación: Orden pertenece a Usuario (Restrict = no borrar usuario con órdenes activas)
            e.HasOne(o => o.Usuario)
             .WithMany(u => u.Ordenes)
             .HasForeignKey(o => o.UsuarioId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurarOrdenDetalle(ModelBuilder mb)
    {
        mb.Entity<OrdenDetalle>(e =>
        {
            e.HasKey(od => od.Id);
            e.Property(od => od.PrecioUnitario).HasColumnType("decimal(18,2)");
            e.Property(od => od.Subtotal).HasColumnType("decimal(18,2)");

            // Cascade = borrar detalles si se borra la orden
            e.HasOne(od => od.Orden)
             .WithMany(o => o.Detalles)
             .HasForeignKey(od => od.OrdenId)
             .OnDelete(DeleteBehavior.Cascade);

            // Restrict = no borrar producto si tiene detalles de orden (historial de ventas)
            e.HasOne(od => od.Producto)
             .WithMany()
             .HasForeignKey(od => od.ProductoId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // ------------------------------------------------------------------
    // HASH DEL ADMIN — constante pre-computada (BCrypt hash de "Admin123!" con work factor 11).
    // DEBE ser una constante fija. Si fuera static readonly con BCrypt.HashPassword(),
    // se generaría un nuevo salt en cada ejecución de EF Tools y EF Core detectaría
    // "pending model changes" en cada dotnet ef database update.
    // Para regenerar: ejecutar BCrypt.Net.BCrypt.HashPassword("Admin123!", 11) una vez y pegar.
    // ------------------------------------------------------------------
    private const string HashAdminFijo =
        "$2a$11$6UzHJUoBbxgCefygc7iWkO3B9TgR5j28FMElzcMhOG3tDHqYZaMLu";

    // ------------------------------------------------------------------
    // SEED DATA — datos iniciales insertados con la primera migración
    // ------------------------------------------------------------------

    private static void AgregarDatosIniciales(ModelBuilder mb)
    {
        // 5 categorías
        mb.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Electrónica", Descripcion = "Gadgets, dispositivos y accesorios tecnológicos", Activa = true },
            new Categoria { Id = 2, Nombre = "Ropa", Descripcion = "Moda casual, formal y deportiva", Activa = true },
            new Categoria { Id = 3, Nombre = "Hogar", Descripcion = "Muebles, decoración y electrodomésticos", Activa = true },
            new Categoria { Id = 4, Nombre = "Deportes", Descripcion = "Equipamiento y ropa deportiva", Activa = true },
            new Categoria { Id = 5, Nombre = "Libros", Descripcion = "Libros físicos y digitales", Activa = true }
        );

        // Fecha fija para evitar que EF Core detecte cambios en migraciones subsecuentes
        var fechaSeed = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // 12 productos — distribuidos en las 5 categorías
        mb.Entity<Producto>().HasData(
            // Electrónica
            new Producto { Id = 1, Nombre = "Laptop Gaming Pro", Descripcion = "Laptop 16GB RAM, SSD 512GB, RTX 4060", Precio = 25999.99m, Stock = 15, CategoriaId = 1, ImagenUrl = "https://placehold.co/400x300?text=Laptop", Activo = true, FechaCreacion = fechaSeed },
            new Producto { Id = 2, Nombre = "Audífonos Bluetooth", Descripcion = "Cancelación de ruido, 30 hrs batería", Precio = 1899.50m, Stock = 50, CategoriaId = 1, ImagenUrl = "https://placehold.co/400x300?text=Audifonos", Activo = true, FechaCreacion = fechaSeed },
            new Producto { Id = 3, Nombre = "Monitor 4K 27 pulgadas", Descripcion = "IPS, 144Hz, HDR10", Precio = 8499.00m, Stock = 20, CategoriaId = 1, ImagenUrl = "https://placehold.co/400x300?text=Monitor", Activo = true, FechaCreacion = fechaSeed },
            new Producto { Id = 4, Nombre = "Teclado Mecánico RGB", Descripcion = "Switches Cherry MX, retroiluminado", Precio = 2199.00m, Stock = 35, CategoriaId = 1, ImagenUrl = "https://placehold.co/400x300?text=Teclado", Activo = true, FechaCreacion = fechaSeed },
            // Ropa
            new Producto { Id = 5, Nombre = "Camiseta Algodón Premium", Descripcion = "100% algodón, corte regular", Precio = 399.00m, Stock = 100, CategoriaId = 2, ImagenUrl = "https://placehold.co/400x300?text=Camiseta", Activo = true, FechaCreacion = fechaSeed },
            new Producto { Id = 6, Nombre = "Jeans Slim Fit", Descripcion = "Mezclilla stretch, azul oscuro", Precio = 899.00m, Stock = 60, CategoriaId = 2, ImagenUrl = "https://placehold.co/400x300?text=Jeans", Activo = true, FechaCreacion = fechaSeed },
            // Hogar
            new Producto { Id = 7, Nombre = "Silla Ergonómica de Oficina", Descripcion = "Soporte lumbar, reposabrazos ajustable", Precio = 5999.00m, Stock = 10, CategoriaId = 3, ImagenUrl = "https://placehold.co/400x300?text=Silla", Activo = true, FechaCreacion = fechaSeed },
            new Producto { Id = 8, Nombre = "Lámpara LED de Escritorio", Descripcion = "3 tonos de luz, dimmer táctil", Precio = 699.00m, Stock = 40, CategoriaId = 3, ImagenUrl = "https://placehold.co/400x300?text=Lampara", Activo = true, FechaCreacion = fechaSeed },
            // Deportes
            new Producto { Id = 9, Nombre = "Mancuernas Ajustables 20kg", Descripcion = "Par de mancuernas con discos intercambiables", Precio = 1599.00m, Stock = 25, CategoriaId = 4, ImagenUrl = "https://placehold.co/400x300?text=Mancuernas", Activo = true, FechaCreacion = fechaSeed },
            new Producto { Id = 10, Nombre = "Tapete de Yoga Premium", Descripcion = "6mm grosor, antideslizante", Precio = 599.00m, Stock = 45, CategoriaId = 4, ImagenUrl = "https://placehold.co/400x300?text=Tapete", Activo = true, FechaCreacion = fechaSeed },
            // Libros
            new Producto { Id = 11, Nombre = "Clean Code — Robert C. Martin", Descripcion = "Guía para escribir código limpio y mantenible", Precio = 450.00m, Stock = 30, CategoriaId = 5, ImagenUrl = "https://placehold.co/400x300?text=CleanCode", Activo = true, FechaCreacion = fechaSeed },
            new Producto { Id = 12, Nombre = "Design Patterns — Gang of Four", Descripcion = "Los 23 patrones de diseño clásicos", Precio = 520.00m, Stock = 20, CategoriaId = 5, ImagenUrl = "https://placehold.co/400x300?text=DesignPatterns", Activo = true, FechaCreacion = fechaSeed }
        );

        // Usuario admin — password: Admin123!
        // Nota: BCrypt genera un salt aleatorio cada vez que se llama. Para evitar que
        // EF Core detecte cambios en la migración, usamos la constante definida abajo.
        mb.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Nombre = "Admin Tienda",
                Email = "admin@ecommercenet.com",
                PasswordHash = HashAdminFijo,
                Rol = RolUsuario.Admin,
                FechaRegistro = fechaSeed
            }
        );
    }
}
