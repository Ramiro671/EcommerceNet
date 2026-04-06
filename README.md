# EcommerceNet — Tienda Fullstack .NET & Vue.js

> Proyecto de portafolio para la vacante **Senior Fullstack .NET & Vue.js Developer** en **portafolio profesional**.

## Sobre el proyecto

Tienda en línea con catálogo de productos, carrito de compras, checkout y autenticación.
Diseñada siguiendo los principios de los Studios de portafolio profesional:

- **Software Engineering & QA Studio** → Clean Architecture, pruebas unitarias, código revisable
- **Cloud & DevOps Studio** → AWS (portafolio profesional es AWS Partner), GitHub Actions CI/CD
- **Launch Pod** → MVP funcional con calidad embebida desde el día uno

## Funcionalidades

- Catálogo de productos con categorías y búsqueda
- Carrito de compras: agregar, quitar, actualizar cantidades
- Checkout: genera órdenes, valida stock, reduce inventario
- Autenticación JWT con roles (admin / cliente)
- Panel admin: CRUD de productos y categorías

## Stack técnico (alineado a la vacante)

| Capa | Tecnología |
|------|-----------|
| Backend | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core 8 |
| BD Relacional | SQL Server |
| BD NoSQL | MongoDB (historial de búsquedas) |
| Frontend | Vue.js 3 (Composition API + Pinia) |
| Frontend Legacy | jQuery 3.x |
| Auth | JWT Bearer |
| Cloud | AWS Free Tier (EC2/EB + RDS) |
| CI/CD | GitHub Actions |
| Tests | xUnit |

## Estructura

```
EcommerceNet/
├── src/
│   ├── EcommerceNet.Core/       # Entidades, interfaces, DTOs, servicios
│   ├── EcommerceNet.Data/       # EF Core, repositorios, MongoDB
│   ├── EcommerceNet.API/        # Controladores, middleware, JWT
│   └── EcommerceNet.Web/        # Vue.js 3 + jQuery legacy
├── tests/
│   └── EcommerceNet.Tests/      # Pruebas unitarias
├── docs/                        # Guías de estudio por día
├── CLAUDE.md                    # Reglas de arquitectura
└── README.md
```

## Plan de 5 días

| Día | Tema | Rama |
|-----|------|------|
| 1 | C#, Clean Architecture, entidades y tests | `dia-01/fundamentos-csharp` |
| 2 | ASP.NET Core API, JWT, Swagger | `dia-02/aspnet-api` |
| 3 | SQL Server + EF Core + MongoDB | `dia-03/datos` |
| 4 | Vue.js 3 + jQuery legacy | `dia-04/frontend` |
| 5 | AWS deploy, CI/CD, prep entrevista | `dia-05/deploy-aws` |

## Ejecutar localmente

```powershell
# Backend
cd src/EcommerceNet.API
dotnet restore; dotnet run

# Frontend
cd src/EcommerceNet.Web
npm install; npm run dev
```

## Inteligencia de entrevista (portafolio profesional - Glassdoor)

- Proceso: 3 rondas (RH → técnica con senior/líder → cliente)
- Envían prueba técnica de código
- Preguntan: frameworks JS, experiencia AWS, nivel de inglés, bases de datos
