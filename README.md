# EcommerceNet

Tienda online fullstack construida para demostrar dominio del stack requerido por la vacante **Senior Fullstack .NET & Vue.js Developer**.

## Demo en vivo

| Recurso | URL |
|---------|-----|
| **API (Swagger)** | http://ecommercenet-api.eba-fxkridvp.us-east-1.elasticbeanstalk.com/swagger |
| **Frontend (Vue.js)** | http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com |
| Usuario demo | demo@ecommercenet.com / Demo123! |
| Admin demo | admin@ecommercenet.com / Admin123! |

> Desplegado en AWS: API en Elastic Beanstalk (Docker + .NET 10, t3.micro), frontend estático en S3.

---

## Tecnologías

| Capa | Tecnología | Versión |
|------|-----------|---------|
| Backend | ASP.NET Core Web API | 10.0 |
| ORM | Entity Framework Core | 10.0.5 |
| BD Relacional | SQL Server LocalDB | 2022 |
| BD NoSQL | MongoDB | 7.x |
| Auth | JWT Bearer Tokens | — |
| Frontend | Vue.js 3 + Vite | 3.4 / 5.3 |
| Estado global | Pinia | 2.1.7 |
| Router | Vue Router | 4.3.3 |
| HTTP | Axios | 1.7.2 |
| Frontend Legacy | jQuery | 3.7.1 |
| Tests | xUnit | 2.9.x |
| CI/CD | GitHub Actions | — |
| Contenedores | Docker + docker-compose | — |
| Cloud | AWS (Elastic Beanstalk + S3) | — |

---

## Arquitectura

```
[Vue.js 3 SPA]  ←──────────────→  [ASP.NET Core API]  ←──→  [SQL Server]
      ↑                                     ↑                       ↑
   Pinia Store                      JWT + Swagger              EF Core
   Vue Router                       Middleware                 Migraciones
   Axios + JWT                      Clean Architecture         Seed Data
                                           ↓
                                     [MongoDB]
                                  (historial búsquedas)

AWS:
   S3 + CloudFront → [Vue.js SPA]
   Elastic Beanstalk → [Docker: ASP.NET Core API]
   RDS SQL Server Express
```

### Capas de Clean Architecture

```
EcommerceNet.slnx
├── EcommerceNet.Core    ← Capa 0: entidades, interfaces, DTOs — SIN dependencias externas
├── EcommerceNet.Data    ← Capa 1: EF Core, repositorios, MongoDB — depende de Core
├── EcommerceNet.API     ← Capa 2: controladores, JWT, Swagger — depende de Data
└── EcommerceNet.Web     ← Capa 3: Vue.js 3 SPA (Node.js independiente, consume API via HTTP)
```

---

## Ejecución local

### Prerequisitos

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Node.js 20+](https://nodejs.org/)
- [MongoDB](https://www.mongodb.com/try/download/community) (opcional)

### Backend

```bash
cd src/EcommerceNet.API
dotnet run
# API: http://localhost:5152
# Swagger: http://localhost:5152/swagger
```

La primera ejecución crea la BD automáticamente con 5 categorías, 12 productos y 1 admin.

### Frontend

```bash
cd src/EcommerceNet.Web
npm install
npm run dev
# App: http://localhost:5173
# jQuery legacy: http://localhost:5173/legacy.html
```

### Pruebas

```bash
dotnet test
# Resultado esperado: Passed! 23 of 23 tests
```

---

## Ejecución con Docker

```bash
# Levanta API + SQL Server + MongoDB juntos
docker-compose up --build
# API: http://localhost:5000/swagger

# Detener
docker-compose down

# Detener y eliminar datos
docker-compose down -v
```

---

## Endpoints de la API

### Productos (público)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/productos` | Listar todos los productos activos |
| GET | `/api/productos/{id}` | Detalle de un producto |
| GET | `/api/productos/buscar?termino=x` | Búsqueda |
| GET | `/api/productos/categoria/{id}` | Por categoría |

### Productos (Admin)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/productos` | Crear producto |
| PUT | `/api/productos/{id}` | Actualizar |
| DELETE | `/api/productos/{id}` | Eliminar |

### Carrito (autenticado)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/carrito` | Ver carrito |
| POST | `/api/carrito/agregar` | Agregar producto |
| PUT | `/api/carrito/{productoId}` | Actualizar cantidad |
| DELETE | `/api/carrito/{productoId}` | Quitar producto |
| DELETE | `/api/carrito` | Vaciar |
| POST | `/api/carrito/checkout` | Procesar compra |

### Órdenes (autenticado)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/ordenes` | Mis órdenes |
| GET | `/api/ordenes/{id}` | Detalle |
| PUT | `/api/ordenes/{id}/cancelar` | Cancelar |

### Auth (público)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/auth/registrar` | Crear cuenta → retorna JWT |
| POST | `/api/auth/login` | Iniciar sesión → retorna JWT |

---

## CI/CD con GitHub Actions

El pipeline se dispara en cada push a `main` o `desarrollo`:

```
push a main/desarrollo
         ↓
┌────────────────────┐    ┌─────────────────────┐
│  Job: backend       │    │  Job: frontend        │
│  dotnet restore     │    │  npm ci               │
│  dotnet build       │    │  npm run build        │
│  dotnet test (23)   │    │  upload artefact      │
│  dotnet publish     │    └─────────────────────┘
│  upload artefact    │
└────────────────────┘
```

---

## Historial de desarrollo (5 días)

| Día | Qué se construyó |
|-----|-----------------|
| **1** | Entidades, interfaces, DTOs, CarritoServicio, 23 tests xUnit |
| **2** | 5 controladores, 18 endpoints REST, JWT, Swagger, middleware |
| **3** | EF Core, 5 repositorios, migraciones, SQL Server, MongoDB |
| **4** | Vue.js 3 SPA: 3 stores Pinia, 7 vistas, jQuery legacy |
| **5** | Docker, CI/CD GitHub Actions, README, deploy AWS |

---

## Sobre el proyecto

Proyecto personal fullstack que consolida 9+ años de experiencia con C#/.NET en un stack moderno: backend con Clean Architecture, frontend SPA, bases de datos relacionales y NoSQL, autenticación JWT, contenerización y deploy en nube.

---

Licencia MIT — Proyecto de demostración técnica para proceso de entrevista.
