using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EcommerceNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreacionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Carritos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UltimaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carritos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carritos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ordenes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroOrden = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DireccionEnvio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ordenes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ordenes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarritoItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CarritoId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarritoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarritoItems_Carritos_CarritoId",
                        column: x => x.CarritoId,
                        principalTable: "Carritos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarritoItems_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenDetalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrdenId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenDetalles_Ordenes_OrdenId",
                        column: x => x.OrdenId,
                        principalTable: "Ordenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdenDetalles_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Activa", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Gadgets, dispositivos y accesorios tecnológicos", "Electrónica" },
                    { 2, true, "Moda casual, formal y deportiva", "Ropa" },
                    { 3, true, "Muebles, decoración y electrodomésticos", "Hogar" },
                    { 4, true, "Equipamiento y ropa deportiva", "Deportes" },
                    { 5, true, "Libros físicos y digitales", "Libros" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Email", "FechaRegistro", "Nombre", "PasswordHash", "Rol" },
                values: new object[] { 1, "admin@ecommercenet.com", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Admin Tienda", "$2a$11$6UzHJUoBbxgCefygc7iWkO3B9TgR5j28FMElzcMhOG3tDHqYZaMLu", 1 });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Activo", "CategoriaId", "Descripcion", "FechaCreacion", "ImagenUrl", "Nombre", "Precio", "Stock" },
                values: new object[,]
                {
                    { 1, true, 1, "Laptop 16GB RAM, SSD 512GB, RTX 4060", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://placehold.co/400x300?text=Laptop", "Laptop Gaming Pro", 25999.99m, 15 },
                    { 2, true, 1, "Cancelación de ruido, 30 hrs batería", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://placehold.co/400x300?text=Audifonos", "Audífonos Bluetooth", 1899.50m, 50 },
                    { 3, true, 1, "IPS, 144Hz, HDR10", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://placehold.co/400x300?text=Monitor", "Monitor 4K 27 pulgadas", 8499.00m, 20 },
                    { 4, true, 1, "Switches Cherry MX, retroiluminado", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://placehold.co/400x300?text=Teclado", "Teclado Mecánico RGB", 2199.00m, 35 },
                    { 5, true, 2, "100% algodón, corte regular", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://placehold.co/400x300?text=Camiseta", "Camiseta Algodón Premium", 399.00m, 100 },
                    { 6, true, 2, "Mezclilla stretch, azul oscuro", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://placehold.co/400x300?text=Jeans", "Jeans Slim Fit", 899.00m, 60 },
                    { 7, true, 3, "Soporte lumbar, reposabrazos ajustable", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://placehold.co/400x300?text=Silla", "Silla Ergonómica de Oficina", 5999.00m, 10 },
                    { 8, true, 3, "3 tonos de luz, dimmer táctil", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://placehold.co/400x300?text=Lampara", "Lámpara LED de Escritorio", 699.00m, 40 },
                    { 9, true, 4, "Par de mancuernas con discos intercambiables", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://placehold.co/400x300?text=Mancuernas", "Mancuernas Ajustables 20kg", 1599.00m, 25 },
                    { 10, true, 4, "6mm grosor, antideslizante", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://placehold.co/400x300?text=Tapete", "Tapete de Yoga Premium", 599.00m, 45 },
                    { 11, true, 5, "Guía para escribir código limpio y mantenible", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://placehold.co/400x300?text=CleanCode", "Clean Code — Robert C. Martin", 450.00m, 30 },
                    { 12, true, 5, "Los 23 patrones de diseño clásicos", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://placehold.co/400x300?text=DesignPatterns", "Design Patterns — Gang of Four", 520.00m, 20 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarritoItems_CarritoId",
                table: "CarritoItems",
                column: "CarritoId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritoItems_ProductoId",
                table: "CarritoItems",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Carritos_UsuarioId",
                table: "Carritos",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenDetalles_OrdenId",
                table: "OrdenDetalles",
                column: "OrdenId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenDetalles_ProductoId",
                table: "OrdenDetalles",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_NumeroOrden",
                table: "Ordenes",
                column: "NumeroOrden");

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_UsuarioId",
                table: "Ordenes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Nombre",
                table: "Productos",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarritoItems");

            migrationBuilder.DropTable(
                name: "OrdenDetalles");

            migrationBuilder.DropTable(
                name: "Carritos");

            migrationBuilder.DropTable(
                name: "Ordenes");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
