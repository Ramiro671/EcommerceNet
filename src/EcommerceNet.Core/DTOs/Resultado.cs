namespace EcommerceNet.Core.DTOs;

/// <summary>Envuelve toda respuesta de la API en un formato estándar</summary>
public class Resultado<T>
{
    public bool Exito { get; set; }
    public T? Datos { get; set; }
    public string? Mensaje { get; set; }
    public List<string> Errores { get; set; } = new();

    public static Resultado<T> Ok(T datos, string? mensaje = null)
        => new() { Exito = true, Datos = datos, Mensaje = mensaje };

    public static Resultado<T> Error(string mensaje)
        => new() { Exito = false, Mensaje = mensaje };

    public static Resultado<T> ErrorValidacion(List<string> errores)
        => new() { Exito = false, Errores = errores, Mensaje = "Error de validación" };
}
