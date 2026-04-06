using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EcommerceNet.Data.MongoDB;

/// <summary>
/// Documento MongoDB — representa una búsqueda que hizo un usuario.
///
/// A diferencia de las entidades relacionales, los documentos MongoDB son flexibles:
/// no tienen esquema fijo, pueden tener campos opcionales, y se almacenan como JSON (BSON).
///
/// [BsonId] = marca la propiedad como el _id de MongoDB (equivalente a PK en SQL)
/// [BsonRepresentation(BsonType.ObjectId)] = el string se serializa como ObjectId en MongoDB
/// </summary>
public class BusquedaHistorial
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>Término buscado (normalizado a minúsculas)</summary>
    public string Termino { get; set; } = string.Empty;

    /// <summary>ID del usuario que buscó (null si no estaba autenticado)</summary>
    public int? UsuarioId { get; set; }

    /// <summary>Cuántos productos se encontraron con este término</summary>
    public int ResultadosEncontrados { get; set; }

    /// <summary>Cuándo se realizó la búsqueda</summary>
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
