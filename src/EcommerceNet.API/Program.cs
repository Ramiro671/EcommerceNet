using System.Text;
using EcommerceNet.API.Middleware;
using EcommerceNet.Core.Interfaces;
using EcommerceNet.Core.Servicios;
using EcommerceNet.Data;
using EcommerceNet.Data.MongoDB;
using EcommerceNet.Data.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// SECCIÓN 1: REGISTRO DE SERVICIOS (Inyección de Dependencias)
// ============================================================

// Controladores ASP.NET Core
builder.Services.AddControllers();

// Swagger — documentación interactiva de la API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EcommerceNet API",
        Version = "v1",
        Description = "API REST para tienda en línea — Proyecto de estudio DaCodes"
    });

    // Botón "Authorize" en Swagger para pegar el token JWT
    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa tu token JWT. Ejemplo: eyJhbGciOi..."
    });

    opciones.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS — permite peticiones desde el frontend Vue.js y desde S3 en producción
builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy("PermitirVue", politica =>
    {
        politica.WithOrigins(
                    "http://localhost:5173",
                    "http://localhost:5000",
                    "http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com"
                )
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Autenticación JWT
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(opciones =>
{
    opciones.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opciones.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opciones =>
{
    opciones.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// ----------------------------------------------------------
// Día 3: EF Core — SQL Server en desarrollo, InMemory en producción (AWS demo)
// AddDbContext = Scoped (una conexión por request HTTP)
// ----------------------------------------------------------
if (builder.Configuration.GetValue<bool>("UseInMemoryDatabase"))
{
    // Base de datos en memoria para la demo en AWS (sin necesidad de RDS)
    builder.Services.AddDbContext<AppDbContext>(opciones =>
        opciones.UseInMemoryDatabase("EcommerceNetDB"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(opciones =>
        opciones.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));
}

// ----------------------------------------------------------
// Día 3: Unidad de Trabajo y servicios con BD real
// Scoped = se crea uno por request (correcto para acceso a BD)
// ----------------------------------------------------------
builder.Services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();
builder.Services.AddScoped<IAuthServicio, AuthServicio>();   // Data/Servicios/AuthServicio
builder.Services.AddScoped<ICarritoServicio, CarritoServicio>();

// ----------------------------------------------------------
// Día 3: MongoDB — Singleton porque MongoClient gestiona su propio pool de conexiones
// ----------------------------------------------------------
builder.Services.AddSingleton<HistorialBusquedaServicio>();

var app = builder.Build();

// ============================================================
// SECCIÓN 2: PIPELINE DE MIDDLEWARE
// El ORDEN importa — cada petición HTTP pasa por estos en secuencia
// ============================================================

// 1. Manejo global de errores — SIEMPRE al principio para atrapar todo lo demás
app.UseMiddleware<ManejadorErroresMiddleware>();

// 2. Swagger en todos los entornos (necesario para demo en AWS)
app.UseSwagger();
app.UseSwaggerUI(opciones =>
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "EcommerceNet API v1");
    opciones.RoutePrefix = "swagger";
});

// 3. HTTPS redirect desactivado en producción (EB maneja HTTP en el puerto 80)
if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

// 4. CORS — antes de auth para que las peticiones preflight (OPTIONS) pasen
app.UseCors("PermitirVue");

// 5. Autenticación — lee y valida el token JWT del header Authorization
app.UseAuthentication();

// 6. Autorización — DEBE ir después de UseAuthentication
app.UseAuthorization();

// 7. Mapear controladores
app.MapControllers();

// ----------------------------------------------------------
// Seed data para InMemory DB (EnsureCreated aplica el HasData de OnModelCreating)
// En SQL Server esto se hace con migraciones; en InMemory usamos EnsureCreated
// ----------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
