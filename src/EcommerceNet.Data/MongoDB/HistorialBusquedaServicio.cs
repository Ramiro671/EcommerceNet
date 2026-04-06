using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace EcommerceNet.Data.MongoDB;

/// <summary>
/// Servicio que guarda y consulta el historial de búsquedas en MongoDB.
///
/// Se registra como SINGLETON en DI porque:
/// - MongoClient está diseñado para ser reutilizado (maneja pool de conexiones internamente)
/// - Crear un MongoClient por request sería ineficiente (overhead de conexión)
/// - Diferente de EF Core donde cada request necesita su propio DbContext (problemas de concurrencia)
/// </summary>
public class HistorialBusquedaServicio
{
    // IMongoCollection<T> = equivalente a DbSet<T> en EF Core
    private readonly IMongoCollection<BusquedaHistorial> _coleccion;

    public HistorialBusquedaServicio(IConfiguration config)
    {
        var connectionString = config["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017";
        var databaseName = config["MongoDB:DatabaseName"] ?? "EcommerceNetDB";

        // MongoClient = punto de entrada al servidor MongoDB (puede manejar múltiples bases de datos)
        var cliente = new MongoClient(connectionString);

        // GetDatabase = obtiene o crea la base de datos
        var database = cliente.GetDatabase(databaseName);

        // GetCollection = obtiene o crea la colección "busquedas" (equivalente a una tabla)
        _coleccion = database.GetCollection<BusquedaHistorial>("busquedas");
    }

    /// <summary>
    /// Registrar una nueva búsqueda en MongoDB.
    /// InsertOneAsync es no bloqueante — no espera confirmación del disco.
    /// </summary>
    public async Task RegistrarBusquedaAsync(string termino, int? usuarioId, int resultados)
    {
        var busqueda = new BusquedaHistorial
        {
            Termino = termino.ToLower().Trim(),  // normalizar para agrupar bien
            UsuarioId = usuarioId,
            ResultadosEncontrados = resultados,
            Fecha = DateTime.UtcNow
        };

        await _coleccion.InsertOneAsync(busqueda);
    }

    /// <summary>
    /// Obtener los términos más buscados usando un pipeline de agregación.
    ///
    /// Equivalente SQL:
    /// SELECT Termino, COUNT(*) AS TotalBusquedas
    /// FROM busquedas
    /// GROUP BY Termino
    /// ORDER BY TotalBusquedas DESC
    /// LIMIT @top
    /// </summary>
    public async Task<List<TerminoPopular>> ObtenerMasBuscadosAsync(int top = 10)
    {
        return await _coleccion.Aggregate()
            .Group(
                b => b.Termino,  // GROUP BY Termino
                g => new TerminoPopular
                {
                    Termino = g.Key,
                    TotalBusquedas = g.Count()
                })
            .SortByDescending(t => t.TotalBusquedas)  // ORDER BY DESC
            .Limit(top)                                // LIMIT
            .ToListAsync();
    }

    /// <summary>
    /// Historial de búsquedas de un usuario específico — más recientes primero.
    /// </summary>
    public async Task<List<BusquedaHistorial>> ObtenerPorUsuarioAsync(int usuarioId, int limite = 20)
    {
        return await _coleccion
            .Find(b => b.UsuarioId == usuarioId)
            .SortByDescending(b => b.Fecha)
            .Limit(limite)
            .ToListAsync();
    }
}

/// <summary>
/// DTO de respuesta para términos populares de búsqueda.
/// </summary>
public class TerminoPopular
{
    public string Termino { get; set; } = string.Empty;
    public int TotalBusquedas { get; set; }
}
