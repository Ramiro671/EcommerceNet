using EcommerceNet.Core.Interfaces;
using EcommerceNet.Data.Repositorios;

namespace EcommerceNet.Data;

/// <summary>
/// Agrupa todos los repositorios y controla cuándo se guardan los cambios.
/// Garantiza que múltiples operaciones se ejecuten en una sola transacción.
///
/// Patrón: Lazy initialization con el operador ??=
/// Los repositorios se crean solo cuando se necesitan (no al construir UnidadDeTrabajo).
/// Todos comparten el MISMO AppDbContext, por eso los cambios de uno son visibles en otro.
/// </summary>
public class UnidadDeTrabajo : IUnidadDeTrabajo
{
    private readonly AppDbContext _contexto;

    // Repositorios — null hasta que se acceda por primera vez
    private IProductoRepositorio? _productos;
    private ICarritoRepositorio? _carritos;
    private IOrdenRepositorio? _ordenes;
    private IUsuarioRepositorio? _usuarios;
    private ICategoriaRepositorio? _categorias;

    public UnidadDeTrabajo(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    // ??= = null-coalescing assignment:
    // Si _productos es null, créalo y asígnalo. Si ya existe, devuélvelo.
    // Equivale a: if (_productos == null) _productos = new ProductoRepositorio(_contexto);
    //             return _productos;
    public IProductoRepositorio Productos =>
        _productos ??= new ProductoRepositorio(_contexto);

    public ICarritoRepositorio Carritos =>
        _carritos ??= new CarritoRepositorio(_contexto);

    public IOrdenRepositorio Ordenes =>
        _ordenes ??= new OrdenRepositorio(_contexto);

    public IUsuarioRepositorio Usuarios =>
        _usuarios ??= new UsuarioRepositorio(_contexto);

    public ICategoriaRepositorio Categorias =>
        _categorias ??= new CategoriaRepositorio(_contexto);

    /// <summary>
    /// Persiste TODOS los cambios pendientes en una sola transacción.
    /// Si algo falla (ej: violación de constraint), NADA se guarda (atomicidad).
    /// Retorna el número de filas afectadas.
    /// </summary>
    public async Task<int> GuardarCambiosAsync()
    {
        return await _contexto.SaveChangesAsync();
    }

    public void Dispose()
    {
        _contexto.Dispose();
    }
}
