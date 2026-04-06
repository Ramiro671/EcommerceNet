# CLAUDE.md — Reglas de Arquitectura para EcommerceNet

> Este archivo es la guía de diseño del proyecto. Claude Code debe leerlo antes de generar código.
> **NO contiene permisos de ejecución automática.** Cada comando debe ser confirmado por el usuario.

---

## Contexto del proyecto

EcommerceNet es una tienda en línea fullstack construida para demostrar dominio del stack
requerido por la vacante Senior Fullstack .NET & Vue.js Developer en DaCodes.

**DaCodes es AWS Partner.** El despliegue va en AWS, no en Azure.

---

## Arquitectura: Clean Architecture

```
EcommerceNet.sln
├── src/
│   ├── EcommerceNet.Core/         # Capa 0: Entidades, interfaces, DTOs, servicios de negocio
│   ├── EcommerceNet.Data/         # Capa 1: EF Core, repositorios, MongoDB, migraciones
│   ├── EcommerceNet.API/          # Capa 2: Controladores ASP.NET Core, middleware, auth JWT
│   └── EcommerceNet.Web/          # Capa 3: Vue.js 3 SPA + página jQuery legacy
├── tests/
│   └── EcommerceNet.Tests/        # xUnit: pruebas unitarias de entidades y servicios
├── docs/                          # Documentación de estudio (.md por día)
├── CLAUDE.md                      # Este archivo
├── .gitignore
└── README.md
```

### Reglas de dependencia (estrictas)

- `Core` NO depende de nada (cero paquetes NuGet externos excepto abstracciones)
- `Data` depende de `Core` solamente
- `API` depende de `Data` (y transitivamente de Core)
- `Tests` depende de `Core` solamente
- El frontend (`Web`) es un proyecto Node.js independiente que consume la API vía HTTP

---

## Stack técnico obligatorio

| Capa | Tecnología | Notas |
|------|-----------|-------|
| Backend | ASP.NET Core 8 Web API | Controladores con atributos `[ApiController]` |
| ORM | Entity Framework Core 8 | Code First, Fluent API, migraciones |
| BD Relacional | SQL Server (LocalDB) | Índices, relaciones, seed data |
| BD NoSQL | MongoDB | Solo para historial de búsquedas (deseable) |
| Auth | JWT Bearer tokens | Roles: Admin y Cliente |
| Frontend | Vue.js 3 | Composition API, Pinia, Vue Router |
| Frontend Legacy | jQuery 3.x | Una página standalone que consume la API |
| JavaScript | ES6+ | async/await, destructuring, modules |
| Tests | xUnit | Patrón Arrange-Act-Assert |
| Cloud | AWS Free Tier | EC2 o Elastic Beanstalk + RDS |
| CI/CD | GitHub Actions | Build, test, deploy |

---

## Entidades del dominio

```
Categoria (Id, Nombre, Descripcion, Activa)
Producto (Id, Nombre, Descripcion, Precio, Stock, ImagenUrl, Activo, CategoriaId)
Usuario (Id, Nombre, Email, PasswordHash, Rol, FechaRegistro)
Carrito (Id, UsuarioId, UltimaModificacion)
CarritoItem (Id, CarritoId, ProductoId, Cantidad, PrecioUnitario)
Orden (Id, NumeroOrden, UsuarioId, Total, Estado, DireccionEnvio, FechaCreacion)
OrdenDetalle (Id, OrdenId, ProductoId, Cantidad, PrecioUnitario, Subtotal)
```

### Relaciones

- Categoria 1:N Producto
- Usuario 1:1 Carrito
- Carrito 1:N CarritoItem
- CarritoItem N:1 Producto
- Usuario 1:N Orden
- Orden 1:N OrdenDetalle
- OrdenDetalle N:1 Producto

---

## Reglas de código

### C# Backend

- Todos los comentarios y nombres de variables en **español**
- Usar `namespaces` con file-scoped (`namespace X;`)
- Propiedades auto-implementadas siempre
- Async/await en todos los métodos de acceso a datos
- Nunca exponer entidades directamente en la API — siempre usar DTOs
- Inyección de dependencias por constructor
- Patrón Repository + Unit of Work
- Clase `Resultado<T>` para envolver todas las respuestas

### Vue.js Frontend

- Composition API (`<script setup>`) en todos los componentes
- Pinia para estado global (carritoStore, authStore, productoStore)
- Axios con interceptores para JWT
- Comentarios en español

### Git

- Rama `main` — producción
- Rama `desarrollo` — integración
- Ramas por día: `dia-01/fundamentos-csharp`, etc.
- Commits en español con prefijos: `feat:`, `fix:`, `docs:`, `test:`, `refactor:`

---

## Endpoints de la API

### Productos (público)
- `GET /api/productos` — listar activos
- `GET /api/productos/{id}` — detalle
- `GET /api/productos/buscar?termino=x` — búsqueda
- `GET /api/productos/categoria/{id}` — por categoría

### Productos (admin)
- `POST /api/productos` — crear
- `PUT /api/productos/{id}` — actualizar
- `DELETE /api/productos/{id}` — eliminar

### Carrito (autenticado)
- `GET /api/carrito` — ver carrito
- `POST /api/carrito/agregar` — agregar producto
- `PUT /api/carrito/{productoId}` — actualizar cantidad
- `DELETE /api/carrito/{productoId}` — quitar producto
- `DELETE /api/carrito` — vaciar
- `POST /api/carrito/checkout` — procesar compra

### Órdenes (autenticado)
- `GET /api/ordenes` — mis órdenes
- `GET /api/ordenes/{id}` — detalle
- `PUT /api/ordenes/{id}/cancelar` — cancelar

### Auth (público)
- `POST /api/auth/registrar` — crear cuenta
- `POST /api/auth/login` — obtener JWT
