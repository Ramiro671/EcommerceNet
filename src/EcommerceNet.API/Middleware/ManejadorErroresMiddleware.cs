using System.Net;
using System.Text.Json;
using EcommerceNet.Core.DTOs;

namespace EcommerceNet.API.Middleware;

/// <summary>
/// Middleware que atrapa CUALQUIER excepción no manejada en toda la aplicación
/// y la convierte en una respuesta JSON estandarizada con Resultado&lt;T&gt;.
///
/// Sin este middleware, una excepción no atrapada devolvería HTML de error del servidor,
/// lo cual es incorrecto para una API REST (que siempre debe devolver JSON).
///
/// Se registra al INICIO del pipeline para que envuelva todos los demás middlewares.
/// </summary>
public class ManejadorErroresMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ManejadorErroresMiddleware> _logger;

    /// <summary>
    /// Constructor — recibe el siguiente middleware en el pipeline y el logger.
    /// RequestDelegate es un delegado que representa el siguiente componente del pipeline.
    /// </summary>
    public ManejadorErroresMiddleware(RequestDelegate next, ILogger<ManejadorErroresMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Método que ASP.NET Core invoca por cada petición HTTP.
    /// El patrón es: intentar → si falla → manejar el error.
    /// </summary>
    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            // 🔴 BP-01: ENTRY — toda petición pasa por aquí. Inspeccionar: contexto.Request.Path, contexto.Request.Method
            // Pasar la petición al siguiente middleware/controlador
            await _next(contexto);
        }
        catch (Exception ex)
        {
            // Si CUALQUIER parte del pipeline lanza una excepción, llegamos aquí
            // 🔴 BP-41: EXCEPCIÓN ATRAPADA. Inspeccionar: ex.GetType().Name, ex.Message, ex.StackTrace
            _logger.LogError(ex, "Error no manejado en la petición: {Mensaje}", ex.Message);
            await ManejarExcepcionAsync(contexto, ex);
        }
    }

    /// <summary>
    /// Construye la respuesta de error según el tipo de excepción.
    /// Usa switch expression (C# 8+) para mapear tipo de excepción → código HTTP.
    /// </summary>
    private static async Task ManejarExcepcionAsync(HttpContext contexto, Exception ex)
    {
        contexto.Response.ContentType = "application/json";

        // 🔴 BP-42: Mapeo excepción→HTTP. Inspeccionar: codigo (401? 400? 404? 500?), mensaje
        // Switch expression: cada tipo de excepción tiene su código HTTP correspondiente
        var (codigo, mensaje) = ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "No autorizado"),
            InvalidOperationException   => (HttpStatusCode.BadRequest, ex.Message),
            ArgumentException           => (HttpStatusCode.BadRequest, ex.Message),
            KeyNotFoundException        => (HttpStatusCode.NotFound, "Recurso no encontrado"),
            _                           => (HttpStatusCode.InternalServerError, "Error interno del servidor")
        };

        contexto.Response.StatusCode = (int)codigo;

        // Serializar como JSON usando Resultado<T> para consistencia con el resto de la API
        var resultado = Resultado<string>.Error(mensaje);
        var opciones = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // camelCase en JSON
        };

        var json = JsonSerializer.Serialize(resultado, opciones);
        await contexto.Response.WriteAsync(json);
    }
}
