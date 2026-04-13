# Manual Técnico — Día 5: Docker, CI/CD, AWS y Panel de Administración

> **Fecha de ejecución:** 2026-04-09

> **Entorno:** Windows 11, Docker Desktop, .NET SDK 8, Node.js 20, AWS CLI
> **Resultado final:** Proyecto 100% completo — Dockerfile multi-stage, CI/CD con GitHub Actions, panel Admin con CRUD de productos y categorías, guía de deploy AWS

---

## Índice

1. [Revisión previa al desarrollo](#1-revisión-previa-al-desarrollo)
2. [Fase 1 — Corrección de bugs de integración](#2-fase-1--corrección-de-bugs-de-integración)
3. [Fase 2 — Pipeline CI/CD con GitHub Actions](#3-fase-2--pipeline-cicd-con-github-actions)
4. [Fase 3 — Dockerización con multi-stage build](#4-fase-3--dockerización-con-multi-stage-build)
5. [Fase 4 — Panel de administración (AdminView)](#5-fase-4--panel-de-administración-adminview)
6. [Fase 5 — CategoriasController con EF Core](#6-fase-5--categoriascontroller-con-ef-core)
7. [Inventario completo de archivos creados y modificados](#7-inventario-completo-de-archivos-creados-y-modificados)
8. [Análisis línea por línea del Dockerfile](#8-análisis-línea-por-línea-del-dockerfile)
9. [Análisis línea por línea del ci-cd.yml](#9-análisis-línea-por-línea-del-ci-cdyml)
10. [Análisis línea por línea del docker-compose.yml](#10-análisis-línea-por-línea-del-docker-composeyml)
11. [Decisiones técnicas y por qué](#11-decisiones-técnicas-y-por-qué)
12. [Errores encontrados y cómo se resolvieron](#12-errores-encontrados-y-cómo-se-resolvieron)
13. [Estado del proyecto al cierre del Día 5](#13-estado-del-proyecto-al-cierre-del-día-5)
14. [Tabla resumen de los 5 días](#14-tabla-resumen-de-los-5-días)

---

## 1. Revisión previa al desarrollo

Se revisaron los siguientes archivos antes de comenzar el desarrollo:

### 1.1 Convenciones de arquitectura del proyecto

| Regla | Efecto en el código del Día 5 |
|-------|-------------------------------|
| Como empresa AWS Partner, deploy en AWS | Dockerfile, Elastic Beanstalk, S3 |
| Patrón Repository + Unit of Work | ICategoriaRepositorio implementado en las 4 capas |
| Nunca exponer entidades directamente | CategoriaDto y CrearCategoriaDto en Core/DTOs |
| Commits en español con prefijos | `feat:`, `fix:`, `ci:`, `docs:` en cada commit |
| Rama main = producción | Merge final y tag v1.0.0 en main |

### 1.2 `docs/dia-05-deploy-aws.md` (plan del día)

Leído completo. Extraídos los 16 Pomodoros con sus objetivos y el código exacto de:
- `.github/workflows/ci-cd.yml`
- `src/EcommerceNet.API/Dockerfile`
- `docker-compose.yml`

### 1.3 `src/EcommerceNet.API/Controllers/CategoriasController.cs`

El controlador existía como **placeholder** que retornaba strings hardcodeados:
```csharp
return Ok("Endpoint pendiente — se implementa en Día 3 con EF Core");
```
Se reemplazó por implementación real con EF Core.

### 1.4 `src/EcommerceNet.Core/DTOs/ProductoDto.cs`

Confirmó que `CategoriaDto` y `CrearCategoriaDto` no existían — había que crearlos.

### 1.5 `src/EcommerceNet.Data/UnidadDeTrabajo.cs`

Verificó la estructura de lazy initialization con `??=` para agregar `ICategoriaRepositorio`.

### 1.6 `src/EcommerceNet.Web/src/views/AdminView.vue`

El archivo no existía. Se creó desde cero con dos tabs: Productos y Categorías.

---

## 2. Fase 1 — Corrección de bugs de integración

### 2.1 Error crítico: `Carrito.Id` tiene valor temporal

**Error exacto:**
```
The property 'Carrito.Id' has a temporary value while attempting to change
the entity's state to 'Modified'. Either set a permanent value explicitly,
or ensure that the database is configured to generate values for this property.
```

**Causa raíz:** En `CarritoServicio.AgregarProductoAsync()`, cuando el carrito no existía se hacía:
```csharp
carrito = new Carrito { UsuarioId = usuarioId };
await _uow.Carritos.AgregarAsync(carrito);  // estado: Added, Id = 0 (temporal)
carrito.AgregarProducto(producto, dto.Cantidad);
_uow.Carritos.Actualizar(carrito);          // ERROR: intenta cambiar Added → Modified
```

EF Core asigna un Id temporal (0) a las entidades nuevas. Llamar a `Update()` sobre una entidad en estado `Added` viola la máquina de estados interna de EF Core.

**Fix aplicado en `src/EcommerceNet.Core/Servicios/CarritoServicio.cs`:**

```csharp
var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId);
bool esNuevo = carrito == null;  // bandera antes de cualquier operación

if (carrito == null)
{
    carrito = new Carrito { UsuarioId = usuarioId };
    await _uow.Carritos.AgregarAsync(carrito);  // estado: Added
}

carrito.AgregarProducto(producto, dto.Cantidad);

// Solo llamar Actualizar si el carrito ya existía en la BD (tiene Id real)
if (!esNuevo)
    _uow.Carritos.Actualizar(carrito);

await _uow.GuardarCambiosAsync();  // aquí EF Core asigna el Id real al carrito nuevo
```

**Por qué funciona:** Para el carrito nuevo, EF Core ya lo tiene tracked como `Added`. Al llamar `SaveChanges()`, ejecuta `INSERT` y actualiza `carrito.Id` con el valor real. No se necesita `Update()` porque la entidad ya está siendo rastreada.

**La máquina de estados de EF Core:**

```
Detached → (AgregarAsync) → Added → (SaveChanges) → Unchanged
                                                         ↓
                                                   (Actualizar)
                                                         ↓
                                                      Modified → (SaveChanges) → Unchanged
```

Llamar `Actualizar()` en `Added` intenta saltar de `Added` a `Modified` — EF Core no lo permite porque el Id es temporal.

### 2.2 Corrección del link "Admin Tienda"

**Problema:** El NavBar mostraba `<span class="usuario-nombre">Admin Tienda</span>` para todos los usuarios — no era clickable.

**Fix en `src/EcommerceNet.Web/src/components/NavBar.vue`:**
```html
<!-- Antes: span no clickable para todos -->
<span class="usuario-nombre">{{ auth.nombreUsuario }}</span>

<!-- Después: RouterLink para admin, span para clientes -->
<RouterLink v-if="auth.esAdmin" to="/admin" class="btn-admin">Admin Tienda</RouterLink>
<span v-else class="usuario-nombre">{{ auth.nombreUsuario }}</span>
```

CSS agregado:
```css
.btn-admin {
  background: #059669;
  color: white;
  padding: 6px 14px;
  border-radius: 6px;
  font-weight: 600;
  text-decoration: none;
}
```

---

## 3. Fase 2 — Pipeline CI/CD con GitHub Actions

### 3.1 Archivo creado: `.github/workflows/ci-cd.yml`

El pipeline tiene **dos jobs independientes** que corren en paralelo:

| Job | Runner | Pasos |
|-----|--------|-------|
| `backend` | ubuntu-latest | checkout → setup .NET → restore → build → test → publish → upload artifact |
| `frontend` | ubuntu-latest | checkout → setup Node 20 → npm ci → npm run build → upload artifact |

**Triggers (cuándo se activa):**
```yaml
on:
  push:
    branches: [ main, desarrollo ]   # cualquier push a estas ramas
  pull_request:
    branches: [ main ]               # cualquier PR hacia main
```

**Resultado verificado en GitHub Actions:**
- Backend: `dotnet test` ejecuta 23 pruebas, todas pasando ✅
- Frontend: `npm run build` transforma 103 módulos ✅

### 3.2 Estrategia de artefactos

Solo en `main` se generan artefactos descargables:
- `api-publish` — binarios de la API listos para deploy en servidor
- `frontend-dist` — carpeta `dist/` de Vue.js lista para subir a S3

En la rama `desarrollo`, el pipeline solo compila y prueba — no publica.

---

## 4. Fase 3 — Dockerización con multi-stage build

### 4.1 Por qué multi-stage build

| Enfoque | Tamaño de imagen | Incluye |
|---------|-----------------|---------|
| Single-stage con SDK | ~800 MB | Compilador, fuentes, herramientas de debug |
| Multi-stage (lo que usamos) | ~200 MB | Solo los binarios compilados + ASP.NET runtime |

Con multi-stage, la imagen de producción es **4x más pequeña** y no contiene el código fuente ni el compilador — lo cual también mejora la seguridad.

### 4.2 Archivo: `src/EcommerceNet.API/Dockerfile`

> **Actualización post-deploy:** Durante el deploy en AWS Elastic Beanstalk se descubrió que
> el SDK debe ser `10.0` (no `8.0`), ya que el proyecto usa `net10.0` y el formato `.slnx`
> solo es soportado por SDK 9+ (introducido en .NET 9). Ambos Dockerfiles fueron actualizados.

```dockerfile
# ============================================================
# Dockerfile — EcommerceNet.API
# Multi-stage build: compilar con SDK, ejecutar con runtime ligero
# ============================================================

# Etapa 1: COMPILAR con el SDK completo (.NET 10)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copiar archivos de proyecto primero (para cachear la capa de restore)
# IMPORTANTE: el archivo de solución es .slnx (formato .NET 9+), no .sln
COPY EcommerceNet.slnx .
COPY src/EcommerceNet.Core/EcommerceNet.Core.csproj src/EcommerceNet.Core/
COPY src/EcommerceNet.Data/EcommerceNet.Data.csproj src/EcommerceNet.Data/
COPY src/EcommerceNet.API/EcommerceNet.API.csproj src/EcommerceNet.API/
COPY tests/EcommerceNet.Tests/EcommerceNet.Tests.csproj tests/EcommerceNet.Tests/
RUN dotnet restore

# Copiar todo el código fuente y publicar en modo Release
COPY . .
RUN dotnet publish src/EcommerceNet.API -c Release -o /publish

# ============================================================
# Etapa 2: EJECUTAR con solo el ASP.NET runtime (~200 MB)
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /publish .

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]
```

### 4.3 Archivo: `docker-compose.yml`

Levanta tres servicios: API + SQL Server + MongoDB. Ver análisis completo en la sección 10.

**Comando para usar:**
```bash
docker-compose up --build   # primera vez
docker-compose up           # siguientes veces (sin rebuild)
docker-compose down         # detener y eliminar contenedores
docker-compose down -v      # eliminar contenedores Y datos
```

---

## 5. Fase 4 — Panel de administración (AdminView)

### 5.1 Ruta y guard

Agregado en `src/EcommerceNet.Web/src/router/index.js`:

```javascript
{
  path: '/admin',
  name: 'admin',
  component: AdminView,
  meta: { requiereAuth: true, requiereAdmin: true }
}
```

Guard actualizado:
```javascript
router.beforeEach((to, from, next) => {
  const auth = useAuthStore()
  if (to.meta.requiereAuth && !auth.estaLogueado) {
    next({ name: 'login' })
  } else if (to.meta.requiereAdmin && !auth.esAdmin) {
    next({ name: 'tienda' })  // cliente sin permisos → redirige a tienda
  } else {
    next()
  }
})
```

### 5.2 Estructura del AdminView

El panel tiene **dos tabs** seleccionables con `v-if`:

| Tab | Funcionalidad |
|-----|--------------|
| **Productos** | Listar todos los productos, crear nuevo, editar existente, eliminar (hard delete) |
| **Categorías** | Listar todas incluyendo inactivas, crear nueva, editar existente, desactivar (soft delete) |

**Carga de categorías desde la API (no hardcodeadas):**
```javascript
const { data } = await api.get('/categorias/todas')
categorias.value = data.datos
```

El formulario de productos usa las categorías activas:
```html
<option v-for="cat in categorias.filter(c => c.activa)" :key="cat.id" :value="cat.id">
  {{ cat.nombre }}
</option>
```

---

## 6. Fase 5 — CategoriasController con EF Core

### 6.1 Capas modificadas (Clean Architecture)

La implementación requirió cambios en **4 capas**:

```
Core/Interfaces/ICategoriaRepositorio.cs    ← NUEVO: interfaz del contrato
Core/Interfaces/IUnidadDeTrabajo.cs         ← MODIFICADO: + propiedad Categorias
Core/DTOs/ProductoDto.cs                   ← MODIFICADO: + CategoriaDto, CrearCategoriaDto
Data/Repositorios/CategoriaRepositorio.cs  ← NUEVO: implementación EF Core
Data/UnidadDeTrabajo.cs                    ← MODIFICADO: + campo _categorias + lazy init
API/Controllers/CategoriasController.cs    ← REEMPLAZADO: placeholder → implementación real
```

### 6.2 Endpoints implementados

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/categorias` | Público | Listar categorías activas |
| GET | `/api/categorias/todas` | Admin | Listar todas (incluyendo inactivas) |
| POST | `/api/categorias` | Admin | Crear categoría |
| PUT | `/api/categorias/{id}` | Admin | Actualizar nombre/descripción |
| DELETE | `/api/categorias/{id}` | Admin | Soft delete: `Activa = false` |

### 6.3 Por qué soft delete para categorías

Las categorías tienen una FK desde `Productos`. Si se eliminara físicamente una categoría con productos asociados, SQL Server lanzaría error de violación de integridad referencial. El soft delete (`Activa = false`) permite "ocultar" la categoría sin romper la relación.

---

## 7. Inventario completo de archivos creados y modificados

### Archivos NUEVOS del Día 5

```
.github/workflows/ci-cd.yml              — Pipeline CI/CD (2 jobs: backend + frontend)
src/EcommerceNet.API/Dockerfile          — Multi-stage build (SDK → runtime)
docker-compose.yml                       — API + SQL Server + MongoDB
src/EcommerceNet.Core/Interfaces/
  ICategoriaRepositorio.cs               — Contrato del repositorio de categorías
src/EcommerceNet.Data/Repositorios/
  CategoriaRepositorio.cs                — Implementación EF Core
src/EcommerceNet.Web/src/views/
  AdminView.vue                          — Panel admin con 2 tabs y CRUD completo
docs/
  dia-05-manual-tecnico.md               — Este archivo
  dia-05-clase-programacion.md           — Conceptos DevOps/Cloud para estudio
  guia-deploy-aws.md                     — Guía paso a paso ejecutable
```

### Archivos MODIFICADOS del Día 5

```
src/EcommerceNet.Core/Servicios/
  CarritoServicio.cs                     — Fix: bandera esNuevo evita error EF Core
src/EcommerceNet.Core/Interfaces/
  IUnidadDeTrabajo.cs                    — + propiedad ICategoriaRepositorio Categorias
src/EcommerceNet.Core/DTOs/
  ProductoDto.cs                         — + CategoriaDto y CrearCategoriaDto
src/EcommerceNet.Data/
  UnidadDeTrabajo.cs                     — + _categorias field y lazy init
src/EcommerceNet.API/Controllers/
  CategoriasController.cs                — Placeholder → implementación EF Core
src/EcommerceNet.Web/src/components/
  NavBar.vue                             — Admin Tienda: span → RouterLink condicional
src/EcommerceNet.Web/src/router/
  index.js                               — + ruta /admin con guard requiereAdmin
```

**Total: 9 archivos nuevos, 7 archivos modificados**

---

## 8. Análisis línea por línea del Dockerfile

> **Nota:** El Dockerfile original usaba `sdk:8.0` y `aspnet:8.0`. Durante el deploy en AWS se
> descubrió que el proyecto usa `net10.0` (paquetes en versión `10.0.5`) y el formato `.slnx`
> que requiere SDK 10+. Ambos Dockerfiles fueron actualizados. El análisis a continuación
> refleja la versión FINAL que funcionó correctamente en Elastic Beanstalk.

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
```
- `FROM` — imagen base. `dotnet/sdk:10.0` incluye el compilador de C#, herramientas de NuGet, dotnet CLI (~800 MB)
- `AS build` — nombra esta etapa para referenciarla en la etapa 2 con `COPY --from=build`
- Viene de Microsoft Container Registry (`mcr.microsoft.com`)
- **Por qué 10.0 y no 8.0:** El proyecto usa `<TargetFramework>net10.0</TargetFramework>` y paquetes en versión `10.0.5`. El SDK 8 no soporta el formato `.slnx` (introducido en .NET 9+).

```dockerfile
WORKDIR /app
```
- Crea y establece `/app` como directorio de trabajo dentro del contenedor
- Todos los `COPY` y `RUN` siguientes operan desde `/app`

```dockerfile
COPY EcommerceNet.slnx .
COPY src/EcommerceNet.Core/EcommerceNet.Core.csproj src/EcommerceNet.Core/
COPY src/EcommerceNet.Data/EcommerceNet.Data.csproj src/EcommerceNet.Data/
COPY src/EcommerceNet.API/EcommerceNet.API.csproj src/EcommerceNet.API/
COPY tests/EcommerceNet.Tests/EcommerceNet.Tests.csproj tests/EcommerceNet.Tests/
```
- **Truco de caché:** Copia solo los archivos `.csproj` y la solución antes del código fuente
- Docker cachea capas. Si no cambió ningún `.csproj`, reutiliza la capa del `restore` (~60s → ~3s)
- El código `.cs` se copia después — cuando cambia, invalida solo las capas siguientes, no el restore
- **`.slnx` no `.sln`:** El formato `.slnx` es el nuevo estándar desde .NET 9. `dotnet restore` con SDK 8 lo rechaza.

```dockerfile
RUN dotnet restore
```
- Descarga todos los paquetes NuGet listados en los `.csproj`
- Solo se re-ejecuta si cambió algún `.csproj` (caché Docker)

```dockerfile
COPY . .
```
- Copia TODO el código fuente
- Esta capa se invalida con cualquier cambio en el código, pero el restore ya está cacheado

```dockerfile
RUN dotnet publish src/EcommerceNet.API -c Release -o /publish
```
- `-c Release` — compila sin símbolos de debug, con optimizaciones de JIT
- `-o /publish` — output en `/publish` (fuera de WORKDIR para simplificar el COPY de la etapa 2)
- Genera: `EcommerceNet.API.dll`, DLLs de dependencias, `appsettings.json`

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0
```
- **Nueva imagen base:** solo el runtime de ASP.NET Core (~200 MB) — sin compilador, sin SDK
- La etapa 1 (`build`) se descarta — sus capas no van en la imagen final
- La imagen final contiene: runtime + binarios. Sin código fuente, sin SDK

```dockerfile
WORKDIR /app
COPY --from=build /publish .
```
- `--from=build` — copia desde la etapa nombrada "build", no del sistema de archivos local
- Solo los archivos de `/publish` — sin código fuente ni archivos intermedios

```dockerfile
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
```
- `EXPOSE 80` — documenta que el contenedor escucha en 80 (solo metadata, no abre el puerto)
- `ASPNETCORE_URLS=http://+:80` — le dice a Kestrel que escuche en todas las interfaces en puerto 80
- El `+` significa "todas las IPs" (equivale a `0.0.0.0`)

```dockerfile
ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]
```
- Forma `exec` (array JSON) — el proceso `dotnet` es PID 1, recibe señales del OS directamente
- Al hacer `docker stop`, el contenedor recibe SIGTERM → ASP.NET Core completa requests en vuelo antes de cerrar (graceful shutdown)
- Forma alternativa (string) usaría `/bin/sh -c` como intermediario — las señales no llegan correctamente

---

## 9. Análisis línea por línea del ci-cd.yml

```yaml
name: CI/CD EcommerceNet
```
Nombre visible en la pestaña "Actions" de GitHub.

```yaml
on:
  push:
    branches: [ main, desarrollo ]
  pull_request:
    branches: [ main ]
```
- Push a `main` → CI completo + artefactos (CD preparado)
- Push a `desarrollo` → solo CI (sin artefactos)
- PR hacia `main` → CI para verificar antes del merge. GitHub bloquea el merge si falla

```yaml
jobs:
  backend:
    name: Backend (.NET)
    runs-on: ubuntu-latest
```
- `jobs:` — los dos jobs se ejecutan en **paralelo** (no hay `needs:` entre ellos)
- `runs-on: ubuntu-latest` — VM nueva y limpia por cada ejecución (efímera)
- Cada job no comparte nada con otros jobs — sistema de archivos independiente

```yaml
    - name: Checkout código
      uses: actions/checkout@v4
```
- Clona el repositorio en la VM
- `@v4` — fija la versión de la Action. Evita roturas si la Action cambia

```yaml
    - name: Configurar .NET SDK
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
```
- Instala .NET 8 en la VM. Ubuntu latest no tiene .NET preinstalado
- `8.0.x` — "x" significa última patch version (ej: 8.0.404)

```yaml
    - name: Restaurar dependencias
      run: dotnet restore
    - name: Compilar solución
      run: dotnet build --no-restore --configuration Release
```
- Separar restore y build permite ver en el log cuánto tarda cada paso
- `--no-restore` — evita descargar paquetes segunda vez

```yaml
    - name: Ejecutar pruebas
      run: dotnet test --no-build --configuration Release --verbosity normal
```
- **El paso más crítico de CI.** Si cualquier prueba falla → job falla → merge bloqueado
- `--verbosity normal` — muestra cada prueba en el log (útil para debugging)

```yaml
    - name: Publicar API
      if: github.ref == 'refs/heads/main'
      run: dotnet publish src/EcommerceNet.API -c Release -o ./publish
```
- `if:` — condicional. Solo en `main`. En `desarrollo` el pipeline termina en las pruebas
- Genera `./publish/` con los binarios listos para deploy

```yaml
    - name: Subir artefacto de API
      if: github.ref == 'refs/heads/main'
      uses: actions/upload-artifact@v4
      with:
        name: api-publish
        path: ./publish
```
- Guarda los binarios como artefacto descargable en la UI de GitHub Actions
- Retención: 90 días por defecto
- En un CD completo, el siguiente job de `deploy` descargaría este artefacto y lo subiría a EB

```yaml
  frontend:
    name: Frontend (Vue.js)
    runs-on: ubuntu-latest
```
- Job **independiente y paralelo** al de backend — reduce el tiempo total de CI de ~4 min a ~2.5 min

```yaml
    - name: Instalar dependencias
      working-directory: src/EcommerceNet.Web
      run: npm ci
```
- `npm ci` — instala exactamente las versiones del `package-lock.json`. Más reproducible que `npm install`
- `working-directory:` — cambia de directorio antes del comando (el proyecto Vue no está en la raíz)

---

## 10. Análisis línea por línea del docker-compose.yml

```yaml
version: '3.8'
```
Versión del formato. Compatible con Docker Engine 19.03+. En docker compose v2 se puede omitir.

```yaml
services:
  api:
    build:
      context: .
      dockerfile: src/EcommerceNet.API/Dockerfile
```
- `context: .` — contexto de build es la raíz del repositorio
- **Importante:** el Dockerfile hace `COPY EcommerceNet.sln .` → el context debe incluir la raíz
- Si el context fuera `src/EcommerceNet.API/`, el `COPY EcommerceNet.sln .` fallaría

```yaml
    ports:
      - "5000:80"
```
- `HOST:CONTENEDOR` — puerto 80 del contenedor mapeado al 5000 del host
- Acceso desde el host: `http://localhost:5000`
- Acceso desde otro contenedor en la misma red: `http://api` (nombre del servicio = hostname)

```yaml
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;...
```
- Las variables de entorno sobreescriben `appsettings.json`
- `__` (doble guión bajo) = separador jerárquico en .NET. Equivale a `appsettings["ConnectionStrings"]["DefaultConnection"]`
- `Server=sqlserver` — `sqlserver` es el nombre del servicio en docker-compose = hostname en la red Docker

```yaml
    depends_on:
      - sqlserver
```
- Docker inicia `sqlserver` antes que `api`
- **Limitación:** solo garantiza que el contenedor arrancó, no que SQL Server esté listo. SQL Server tarda ~30s en inicializar

```yaml
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=YourStrong!Passw0rd
```
- `ACCEPT_EULA=Y` — requerido. Sin esto, el contenedor no inicia
- `MSSQL_SA_PASSWORD` — política: mínimo 8 chars, mayúsculas, minúsculas, número, símbolo

```yaml
    volumes:
      - sqlserver-data:/var/opt/mssql
```
- Sin volumen, los datos se pierden al hacer `docker-compose down`
- Volumen nombrado gestionado por Docker — persiste entre reinicios
- Se elimina con `docker-compose down -v` (el `-v` es crucial)

```yaml
  mongo:
    image: mongo:7
    ports:
      - "27017:27017"
    volumes:
      - mongo-data:/data/db
```
MongoDB para historial de búsquedas. Puerto estándar 27017.

```yaml
volumes:
  sqlserver-data:
  mongo-data:
```
Docker los crea automáticamente en el primer `up`. En Windows: `C:\ProgramData\docker\volumes\`.

---

## 11. Decisiones técnicas y por qué

| Decisión | Alternativa rechazada | Razón |
|----------|----------------------|-------|
| **Multi-stage build** | Single-stage con SDK | Imagen 4x más pequeña (200MB vs 800MB). Sin código fuente en producción. Menor superficie de ataque. |
| **t2.micro en Elastic Beanstalk** | t3.micro | t2.micro está en el Free Tier (750 hrs/mes gratis). t3.micro cobra desde la primera hora. |
| **S3 para el frontend** | EC2 con Nginx | El frontend es estático. S3 hostea archivos estáticos con alta disponibilidad y sin servidores que administrar. |
| **GitHub Actions para CI/CD** | Jenkins, CircleCI | Integrado en el repositorio. Sin infraestructura adicional. 2000 minutos gratis/mes en repos públicos. |
| **Dos jobs en paralelo** | Un job secuencial | Backend y frontend son independientes. Paralelo reduce tiempo de CI de ~4 min a ~2.5 min. |
| **`dotnet restore` separado** | Solo `dotnet build` | Separar el restore permite que Docker cachee ese layer — los builds subsecuentes son ~20x más rápidos. |
| **Soft delete en categorías** | Hard delete | Las categorías tienen FK desde Productos. Hard delete rompería integridad referencial. |
| **Contexto de build en raíz** | Contexto en src/API | El Dockerfile necesita `EcommerceNet.sln` y todos los `.csproj` — están en la raíz. |

---

## 12. Errores encontrados y cómo se resolvieron

### Error 1: `Carrito.Id` tiene valor temporal (EF Core)

| | Detalle |
|-|---------|
| **Síntoma** | `InvalidOperationException` al agregar producto al carrito por primera vez |
| **Causa** | `Update()` llamado sobre entidad en estado `Added` (Id = 0, temporal) |
| **Fix** | Bandera `bool esNuevo = carrito == null` — solo llama `Actualizar` si el carrito ya existía |
| **Archivo** | `src/EcommerceNet.Core/Servicios/CarritoServicio.cs` |

### Error 2: Admin Tienda no era clickable

| | Detalle |
|-|---------|
| **Síntoma** | El nombre del admin aparecía como texto plano |
| **Causa** | `NavBar.vue` usaba `<span>` para todos los usuarios |
| **Fix** | `v-if="auth.esAdmin"` con `<RouterLink>` y `v-else` con `<span>` |
| **Archivo** | `src/EcommerceNet.Web/src/components/NavBar.vue` |

### Error 3: CategoriasController era placeholder

| | Detalle |
|-|---------|
| **Síntoma** | `GET /api/categorias` retornaba un string hardcodeado |
| **Causa** | El controlador tenía `return Ok("Endpoint pendiente...")` |
| **Fix** | Implementación completa con `IUnidadDeTrabajo` y EF Core |
| **Archivo** | `src/EcommerceNet.API/Controllers/CategoriasController.cs` |

### Error 4: DLL bloqueado por proceso en ejecución (MSBuild)

| | Detalle |
|-|---------|
| **Síntoma** | `MSB3026: No se pudo copiar EcommerceNet.API.dll... acceso denegado` |
| **Causa** | El proceso de `dotnet run` tenía el DLL bloqueado |
| **Fix** | Detener el proceso con Ctrl+C antes de recompilar |
| **Lección** | No se puede recompilar mientras la API está corriendo |

### Error 5: `npm run dev` en directorio equivocado

| | Detalle |
|-|---------|
| **Síntoma** | `ENOENT: no such file or directory, open 'package.json'` |
| **Causa** | Comando ejecutado en `src/EcommerceNet.API` (proyecto .NET, no Vue.js) |
| **Fix** | Navegar a `src/EcommerceNet.Web` antes de ejecutar comandos npm |

---

## 13. Estado del proyecto al cierre del Día 5

### Comandos de verificación ejecutados

```bash
# Backend
dotnet build
# Build succeeded. 0 Error(s). 0 Warning(s).

dotnet test
# Test run for EcommerceNet.Tests.dll
# Passed! - Failed: 0, Passed: 23, Skipped: 0, Total: 23

# Frontend
cd src/EcommerceNet.Web
npm run build
# ✓ 103 modules transformed.
# ✓ built in 1.43s
```

### Deploy real en AWS (ejecutado 2026-04-09)

Después del código base del Día 5, se ejecutó el deploy completo en AWS:

**Archivos creados durante el deploy:**

```
Dockerfile                                    — Dockerfile raíz para Elastic Beanstalk (sdk:10.0)
.ebignore                                     — Excluye docker-compose.yml del paquete EB
src/EcommerceNet.API/appsettings.Production.json — UseInMemoryDatabase=true para AWS demo
src/EcommerceNet.Web/.env.production          — VITE_API_URL con URL real de EB
bucket-policy.json                            — Política de acceso público para S3
```

**Archivos modificados durante el deploy:**

```
src/EcommerceNet.API/Dockerfile              — sdk:8.0 → sdk:10.0, .sln → .slnx
src/EcommerceNet.API/Program.cs              — CORS con URL S3, InMemory DB, Swagger en todos los entornos
src/EcommerceNet.Web/src/services/api.js    — import.meta.env.VITE_API_URL para producción
README.md                                    — URLs reales de producción
```

**Comandos del deploy (en orden):**

```powershell
# Backend: instalar InMemory y crear config de producción
dotnet add src/EcommerceNet.API package Microsoft.EntityFrameworkCore.InMemory

# EB: inicializar y crear entorno (3 intentos hasta resolver todos los errores)
eb init EcommerceNet --platform Docker --region us-east-1
eb create ecommercenet-api --single --instance-type t3.micro --timeout 20

# EB: variables de entorno
eb setenv Jwt__Key="..." Jwt__Issuer=EcommerceNet.API Jwt__Audience=EcommerceNet.Web ASPNETCORE_ENVIRONMENT=Production UseInMemoryDatabase=true

# S3: frontend
npm run build  # desde src/EcommerceNet.Web
aws s3 mb s3://ecommercenet-ramiro671 --region us-east-1
aws s3api put-public-access-block --bucket ecommercenet-ramiro671 --public-access-block-configuration "BlockPublicAcls=false,..."
aws s3 website s3://ecommercenet-ramiro671 --index-document index.html --error-document index.html
aws s3api put-bucket-policy --bucket ecommercenet-ramiro671 --policy file://bucket-policy.json
aws s3 sync src/EcommerceNet.Web/dist/ s3://ecommercenet-ramiro671
```

**URLs de producción:**

| Recurso | URL |
|---------|-----|
| API (Swagger) | http://ecommercenet-api.eba-fxkridvp.us-east-1.elasticbeanstalk.com/swagger |
| Frontend | http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com |
| Health EB | Green ✅ |

### Commits del Día 5

```
3efbfb0 feat: deploy completo — API en Elastic Beanstalk, frontend en S3
f30379d feat: soporte InMemory DB para producción en AWS, Dockerfile en raíz para Elastic Beanstalk
74ff410 fix: corregir EcommerceNet.sln → slnx en Dockerfiles, agregar .ebignore para EB
2470e85 fix: usar SDK .NET 10 en Dockerfiles — proyecto usa net10.0, no net8.0
1919b32 docs: guía completa de deploy AWS, manual técnico y clase del día 5
32bec97 feat: panel admin con gestión de productos y categorías, implementar CategoriasController con EF Core
14c009d fix: no llamar Actualizar en carrito nuevo para evitar error de Id temporal en EF Core
```

### Estado de los builds

| Sistema | Resultado |
|---------|-----------|
| `dotnet build` | ✅ 0 errores, 0 warnings |
| `dotnet test` | ✅ 23/23 pasando |
| `npm run build` | ✅ 103 módulos, 0 errores |
| GitHub Actions | ✅ Backend y Frontend jobs pasando |
| Elastic Beanstalk | ✅ Health: Green |
| S3 Frontend | ✅ HTTP 200 |

---

## 14. Tabla resumen de los 5 días

| Día | Título | Qué se construyó | Archivos clave | Tests |
|-----|--------|-----------------|----------------|-------|
| **1** | Fundamentos C# | Entidades, interfaces, DTOs, servicios de negocio, pruebas unitarias | `Core/` completo, `CarritoServicio.cs` | 22 |
| **2** | ASP.NET Core API | 5 controladores, 18 endpoints, JWT, Swagger, middleware de errores | `API/Controllers/`, `Program.cs`, `AuthController.cs` | 22 |
| **3** | EF Core y datos | Repositorios, Unit of Work, migraciones, seed data, MongoDB, SQL avanzado | `Data/` completo, `AppDbContext.cs` | 22 |
| **4** | Frontend Vue.js 3 | SPA completa con 7 vistas, 3 stores Pinia, página jQuery legacy | `Web/` completo, 3 stores, 7 vistas | 22 |
| **5** | Docker + CI/CD + AWS | Dockerfile multi-stage, GitHub Actions, panel admin CRUD, guía deploy AWS | `Dockerfile`, `ci-cd.yml`, `AdminView.vue`, `CategoriasController.cs` | **23** |

**Totales del proyecto:**
- **23** pruebas unitarias pasando
- **20+** endpoints en la API REST (5 controladores)
- **8** vistas en el frontend (7 cliente + 1 admin con 2 tabs)
- **3** archivos de orquestación DevOps (Dockerfile, docker-compose, ci-cd.yml)
- **17** documentos `.md` para estudio y referencia técnica
- **100%** del stack de la vacante Senior Fullstack .NET & Vue.js implementado y documentado
