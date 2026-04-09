# Resumen Ejecutivo — EcommerceNet

> Documento de referencia rápida para la entrevista en DaCodes.
> Fecha: 2026-04-09

---

## 1. Descripción del proyecto (3 líneas)

EcommerceNet es una tienda online fullstack construida en 5 días para demostrar dominio
del stack técnico de la vacante **Senior Fullstack .NET & Vue.js Developer en DaCodes**.
Incluye backend ASP.NET Core .NET 10 con Clean Architecture, frontend Vue.js 3, CI/CD con
GitHub Actions, y deploy en producción en AWS (Elastic Beanstalk + S3).

---

## 2. URLs de producción

| Recurso | URL |
|---------|-----|
| **API (Swagger)** | http://ecommercenet-api.eba-fxkridvp.us-east-1.elasticbeanstalk.com/swagger |
| **Frontend** | http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com |
| **GitHub** | https://github.com/Ramiro671/EcommerceNet |
| **GitHub Actions** | https://github.com/Ramiro671/EcommerceNet/actions |
| **Admin demo** | admin@ecommercenet.com / Admin123! |
| **Cliente demo** | demo@ecommercenet.com / Demo123! |

---

## 3. Stack tecnológico real (con versiones verificadas)

| Capa | Tecnología | Versión |
|------|-----------|---------|
| Backend | ASP.NET Core Web API | .NET 10.0 |
| ORM | Entity Framework Core | 10.0.5 |
| InMemory DB (AWS) | EF Core InMemory | 10.0.5 |
| BD Relacional (local) | SQL Server LocalDB | 2022 |
| BD NoSQL | MongoDB | 7.x |
| Auth | JWT Bearer Tokens | JwtBearer 10.0.5 |
| Passwords | BCrypt.Net-Next | 4.1.0 |
| API Docs | Swashbuckle (Swagger) | 6.9.0 |
| Frontend | Vue.js 3 + Vite | 3.4 / 5.x |
| Estado global | Pinia | 2.1.7 |
| Router | Vue Router | 4.3.3 |
| HTTP | Axios | 1.7.2 |
| Frontend Legacy | jQuery | 3.7.1 |
| Tests | xUnit | 2.9.x |
| CI/CD | GitHub Actions | — |
| Contenedores | Docker (multi-stage) | — |
| Cloud Backend | AWS Elastic Beanstalk | t3.micro, Docker |
| Cloud Frontend | AWS S3 (static hosting) | — |
| AWS CLI | AWS CLI | 2.34.27 |
| EB CLI | awsebcli | 3.27.1 |
| Python (EB CLI dep) | Python | 3.12.10 |

---

## 4. Los 5 días de desarrollo

| Día | Título | Qué se construyó | Archivos nuevos | Tests |
|-----|--------|-----------------|-----------------|-------|
| **1** | Fundamentos C# | Entidades, interfaces, DTOs, CarritoServicio, pruebas unitarias | Core/ completo (~20 archivos .cs) | 23 |
| **2** | ASP.NET Core API | 5 controladores, 18 endpoints REST, JWT, Swagger, middleware de errores | API/Controllers/ + Program.cs | 23 |
| **3** | EF Core y Datos | Repositorios, UnidadDeTrabajo, migraciones, SQL Server, MongoDB | Data/ completo (~15 archivos .cs) | 23 |
| **4** | Frontend Vue.js 3 | SPA: 8 vistas, 3 stores Pinia, carrito, checkout, admin, jQuery legacy | Web/ completo (~25 archivos .vue/.js) | 23 |
| **5** | Docker + CI/CD + AWS | Dockerfile multi-stage, GitHub Actions, panel admin CRUD, deploy AWS | Dockerfile, .ebignore, ci-cd.yml, AdminView.vue | 23 |

---

## 5. Métricas del proyecto

| Métrica | Valor |
|---------|-------|
| Archivos `.cs` en `src/` | 65 |
| Archivos `.cs` en `tests/` | 10 |
| Pruebas unitarias (xUnit) | **23 de 23 pasando** |
| Endpoints REST en la API | **20+** (5 controladores) |
| Vistas Vue.js | **8** (7 cliente + 1 admin con 2 tabs) |
| Componentes Vue | **3** (NavBar, ProductoCard, CarritoItem) |
| Archivos `.vue` totales | **11** |
| Archivos `.md` en docs/ | **20** |
| Días de desarrollo | **5** |
| Commits | **10+** |

---

## 6. Inventario de documentos en docs/

| Archivo | Descripción |
|---------|-------------|
| `dia-01-fundamentos-csharp.md` | Plan del Día 1: entidades, interfaces, DTOs, tests |
| `dia-01-manual-tecnico.md` | Lo que Claude Code hizo en el Día 1 (línea por línea) |
| `dia-01-clase-programacion.md` | Conceptos teóricos del Día 1: C#, Clean Architecture |
| `dia-02-aspnet-api.md` | Plan del Día 2: controladores, JWT, Swagger |
| `dia-02-manual-tecnico.md` | Lo que Claude Code hizo en el Día 2 |
| `dia-02-clase-programacion.md` | Conceptos teóricos del Día 2: HTTP, REST, JWT |
| `dia-03-datos.md` | Plan del Día 3: EF Core, repositorios, MongoDB |
| `dia-03-manual-tecnico.md` | Lo que Claude Code hizo en el Día 3 |
| `dia-03-clase-programacion.md` | Conceptos teóricos del Día 3: EF Core, SQL |
| `dia-04-frontend.md` | Plan del Día 4: Vue.js 3, Pinia, jQuery |
| `dia-04-manual-tecnico.md` | Lo que Claude Code hizo en el Día 4 |
| `dia-04-clase-programacion.md` | Conceptos teóricos del Día 4: Vue 3, Composition API |
| `dia-05-deploy-aws.md` | Plan del Día 5: Docker, CI/CD, AWS (con errores reales y fixes) |
| `dia-05-manual-tecnico.md` | Lo que Claude Code hizo en el Día 5 (incluyendo deploy real) |
| `dia-05-clase-programacion.md` | Conceptos teóricos del Día 5: Docker, AWS, DevOps |
| `guia-configuracion-aws-paso-a-paso.md` | Configuración de cuenta AWS, IAM, MFA, CLI |
| `guia-deploy-aws.md` | Guía ejecutable de deploy (con Troubleshooting de errores reales) |
| `guia-aws-maestra.md` | Guía unificada AWS — autosuficiente, cubre todo |
| `resumen-proyecto-completo.md` | Este archivo — referencia rápida para entrevista |
| `prompt-notebooklm-dia03.md` | Prompts para NotebookLM — Día 3 |
| `prompt-notebooklm-dia04.md` | Prompts para NotebookLM — Día 4 |
| `prompt-notebooklm-dia05.md` | Prompts para NotebookLM — Día 5 |

---

## 7. "Cuéntame de tu proyecto" — respuesta de 60 segundos

> "Construí EcommerceNet, una tienda online fullstack en 5 días.
>
> El **backend** usa ASP.NET Core .NET 10 con Clean Architecture en 3 capas: Core con las
> entidades e interfaces, Data con EF Core y repositorios, y API con los controladores.
> Tiene 20+ endpoints REST, autenticación JWT con roles Admin y Cliente, SQL Server con
> migraciones, y MongoDB para historial de búsquedas. 23 pruebas unitarias con xUnit.
>
> El **frontend** es una SPA en Vue.js 3 con Composition API, Pinia para estado global,
> Vue Router con guards de autenticación, y una página legacy con jQuery.
>
> El **deploy** está en AWS — la API en Elastic Beanstalk con Docker multi-stage, el frontend
> estático en S3. El pipeline de CI/CD con GitHub Actions compila, prueba y publica artefactos
> automáticamente en cada push a main.
>
> El repositorio está público en GitHub. Puedo mostrar la app corriendo en vivo ahora mismo."

---

## 8. Las 10 preguntas más probables de DaCodes (respuesta en 2 líneas)

### P1: "¿Qué es Clean Architecture?"

Arquitectura en capas con dependencias hacia adentro: Core (sin deps) → Data → API.
Permite cambiar la BD o el framework sin tocar la lógica de negocio.

### P2: "¿Cómo funciona JWT?"

El servidor genera un token con claims (ID, rol) firmado con una clave secreta.
El frontend lo envía en cada request (`Authorization: Bearer {token}`) y el middleware lo valida.

### P3: "¿Diferencia entre IEnumerable e IQueryable?"

`IEnumerable` ejecuta en memoria (LINQ to Objects). `IQueryable` construye expresiones que
EF Core traduce a SQL — el filtro se ejecuta en la base de datos, no en la aplicación.

### P4: "¿Patrón Repository y Unit of Work?"

Repository abstrae el acceso a datos (método por entidad). Unit of Work agrupa múltiples
operaciones en una transacción — `SaveChanges()` va en UoW, no en el repositorio.

### P5: "¿Qué es Docker?"

Empaqueta la app con sus dependencias en un contenedor que corre igual en cualquier máquina.
Usamos multi-stage build: SDK (~800MB) para compilar, runtime (~200MB) para ejecutar.

### P6: "¿Qué es CI/CD?"

Continuous Integration: cada push compila y prueba automáticamente. Si falla, sabes al instante.
Continuous Deployment: si CI pasa, el código se publica. Nuestro pipeline tiene 2 jobs en paralelo.

### P7: "¿Composition API vs Options API en Vue.js?"

Options API organiza por tipo (data, methods, computed). Composition API organiza por funcionalidad.
Composition es mejor para proyectos grandes porque permite reutilizar lógica con composables.

### P8: "¿Qué es Pinia?"

Store oficial de Vue 3 que reemplazó a Vuex. Más simple: sin mutations, las acciones modifican
el estado directamente. Cada store tiene state (ref), getters (computed) y actions (funciones).

### P9: "¿Por qué Elastic Beanstalk y no EC2 directamente?"

EB es PaaS — gestiona la instancia EC2, el load balancer, el auto-scaling y el deploy automáticamente.
Con EC2 tendrías que configurar todo manualmente. EB es 2 comandos: `eb init` + `eb create`.

### P10: "¿Qué hace DaCodes?"

DaCodes tiene 4 Studios especializados: Software Engineering & QA (con Launch Pod), Cloud & DevOps
(AWS Partner, Migration Pod), AI & Data (GenAI Accelerator), y Product Strategy & Design.
Este proyecto demuestra el stack exacto que usan en el Software Engineering & Cloud Studio.

---

## 9. Comandos para limpiar AWS (cuando termine la demo)

```powershell
# Terminar el entorno de Elastic Beanstalk (elimina EC2, load balancer, todo)
C:\Users\ramir\AppData\Local\Programs\Python\Python312\Scripts\eb.exe terminate ecommercenet-api --force

# Eliminar el bucket S3 y todos los archivos
"C:\Program Files\Amazon\AWSCLIV2\aws.exe" s3 rb s3://ecommercenet-ramiro671 --force

# Verificar que no quedan recursos huérfanos
# → Ir a https://console.aws.amazon.com/billing/home → Bills
```

> **Después del Free Tier (12 meses desde la creación de la cuenta), AWS cobra.**
> Si ya no necesitas la demo, elimina los recursos con los comandos de arriba.
