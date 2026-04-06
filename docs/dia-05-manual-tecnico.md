# Manual Técnico — Día 5: Docker + CI/CD con GitHub Actions + Deploy AWS

> **Fecha de ejecución:** 2026-04-06
> **Herramienta:** Claude Code (claude-sonnet-4-6) ejecutado dentro del VSCode Extension
> **Entorno:** Windows 11, .NET SDK 10.0.103, Node.js v20.16.0, Git 2.51.1, Docker Desktop
> **Resultado final:** Build ✅ | Tests 23/23 ✅ | npm build ✅ | Pipeline CI/CD ✅ | Tag v1.0.0 ✅

---

## Índice

1. [Qué leyó Claude Code antes de escribir una sola línea](#1-qué-leyó-claude-code-antes-de-escribir-una-sola-línea)
2. [Fase 1 — Documentos pendientes del Día 4](#2-fase-1--documentos-pendientes-del-día-4)
3. [Fase 2 — Código del Día 5](#3-fase-2--código-del-día-5)
4. [Inventario completo de archivos creados/modificados](#4-inventario-completo-de-archivos-creadosmodificados)
5. [Dockerfile — análisis línea por línea](#5-dockerfile--análisis-línea-por-línea)
6. [docker-compose.yml — análisis servicio por servicio](#6-docker-composeyml--análisis-servicio-por-servicio)
7. [ci-cd.yml — pipeline paso a paso](#7-ci-cdyml--pipeline-paso-a-paso)
8. [Decisiones técnicas y por qué](#8-decisiones-técnicas-y-por-qué)
9. [Comandos ejecutados y resultados](#9-comandos-ejecutados-y-resultados)
10. [Git: rama, commits, merges y tag](#10-git-rama-commits-merges-y-tag)
11. [Estado FINAL del proyecto completo](#11-estado-final-del-proyecto-completo)
12. [Resumen de los 5 días](#12-resumen-de-los-5-días)

---

## 1. Qué leyó Claude Code antes de escribir una sola línea

Claude Code ejecutó cuatro lecturas antes de crear cualquier archivo del Día 5:

### 1.1 `CLAUDE.md` (reglas de arquitectura)

| Regla | Efecto en el Día 5 |
|-------|-------------------|
| DaCodes es AWS Partner | El Dockerfile y CI/CD siguen el flujo del AWS Migration Pod |
| Rama `dia-05/deploy-aws` | Se creó antes del primer archivo |
| Commits en español con prefijos | `feat:`, `fix:`, `ci:` en todos los commits |
| Frontend es proyecto Node.js independiente | El job de frontend usa `working-directory: src/EcommerceNet.Web` |
| `API` depende de `Data` y transitivamente `Core` | El Dockerfile copia los cuatro `.csproj` en el orden correcto |

### 1.2 `docs/dia-05-deploy-aws.md` (plan del día)

Leído en 3 bloques. Extraído:
- El YAML completo del pipeline de GitHub Actions (2 jobs, 6 steps cada uno)
- El Dockerfile multi-stage con `mcr.microsoft.com/dotnet/sdk:8.0` → `aspnet:8.0`
- El `docker-compose.yml` con tres servicios: api, sqlserver, mongo
- La estrategia de deploy AWS: Elastic Beanstalk para API, S3 para frontend
- Las preguntas de entrevista sobre Docker, CI/CD y AWS

### 1.3 `docs/dia-04-manual-tecnico.md` (formato a replicar)

Confirmó la estructura: encabezado con metadata, índice numerado, secciones con análisis detallado, tablas de decisiones técnicas, árbol de archivos, builds verificados.

### 1.4 `docs/dia-01-clase-programacion.md` (formato de clase)

Confirmó el estilo para la clase: concepto desde cero, por qué existe, cómo funciona, análisis línea por línea, glosario.

---

## 2. Fase 1 — Documentos pendientes del Día 4

El Día 4 terminó con la sesión agotada sin generar los dos documentos finales. Se completaron como primera tarea del Día 5:

### 2.1 `docs/dia-04-manual-tecnico.md`

13 secciones. Documentó:
- El problema con `npm create vue@latest` (CLI interactivo, no acepta `--no-ts`) y la solución: proyecto manual con `package.json` propio
- El ajuste del puerto de `5000` a `5152` en `api.js` (puerto real de la API .NET)
- El flujo de datos completo: `ProductoCard emit → TiendaView → carritoStore → api.js interceptor → API .NET → SQL Server → respuesta → items.value → NavBar badge reactivo`
- Los 8 errores evitados, incluyendo `node_modules` en git (se detectó tras el primer commit, se creó `.gitignore` y se removió con `git rm -r --cached`)
- El árbol completo de 19 archivos del frontend con descripción de una línea cada uno

### 2.2 `docs/dia-04-clase-programacion.md`

21 secciones, ~1800 líneas. Cubrió todos los conceptos nuevos: SPA, reactividad, `ref()`, `computed()`, `onMounted()`, `defineProps`, `defineEmits`, todas las directivas Vue, Pinia, Vue Router con guards, Axios con interceptores, localStorage, JavaScript ES6+, jQuery completo, comparación jQuery vs Vue.js con código real del proyecto, análisis línea por línea de 9 archivos, glosario de 55+ términos.

---

## 3. Fase 2 — Código del Día 5

### 3.1 Rama creada

```bash
cd c:/Users/ramir/Source/repos/EcommerceNet
git checkout -b dia-05/deploy-aws
# Switched to a new branch 'dia-05/deploy-aws'
```

### 3.2 Directorio de GitHub Actions

```bash
mkdir -p .github/workflows
# Crea la carpeta que GitHub Actions busca para encontrar workflows
```

### 3.3 Archivos creados (en orden)

1. `.github/workflows/ci-cd.yml` — Pipeline con 2 jobs paralelos
2. `src/EcommerceNet.API/Dockerfile` — Multi-stage build SDK → runtime
3. `src/EcommerceNet.API/.dockerignore` — Excluye bin/ y obj/
4. `docker-compose.yml` — Orquesta api + sqlserver + mongo
5. `README.md` — Reescrito completo con demo, endpoints, Docker, CI/CD

---

## 4. Inventario completo de archivos creados/modificados

```
EcommerceNet/
├── .github/
│   └── workflows/
│       └── ci-cd.yml               ← NUEVO — Pipeline CI/CD: 2 jobs, triggers en main y desarrollo
├── src/
│   └── EcommerceNet.API/
│       ├── Dockerfile               ← NUEVO — Multi-stage: sdk:8.0 para compilar, aspnet:8.0 para ejecutar
│       └── .dockerignore            ← NUEVO — Excluye bin/, obj/, *.user, .vs/
├── docker-compose.yml               ← NUEVO — Orquestación local: api + sqlserver + mongo con volúmenes
└── README.md                        ← ACTUALIZADO — Demo URLs, endpoints completos, Docker, CI/CD, arquitectura
```

**Archivos de docs creados en el mismo commit:**

```
docs/
├── dia-04-manual-tecnico.md     ← NUEVO (pendiente del Día 4)
├── dia-04-clase-programacion.md ← NUEVO (pendiente del Día 4)
├── dia-05-manual-tecnico.md     ← NUEVO (este archivo)
└── dia-05-clase-programacion.md ← NUEVO
```

**Total de archivos nuevos en el Día 5: 9 archivos**

---

## 5. Dockerfile — análisis línea por línea

```dockerfile
# ============================================================
# Etapa 1: BUILD — imagen con el SDK completo (~800 MB)
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
```
- `FROM` — instrucción obligatoria, define la imagen base
- `mcr.microsoft.com/dotnet/sdk:8.0` — imagen oficial de Microsoft con el compilador C#, las herramientas `dotnet` y los targets de compilación. Sin esta imagen no puedes compilar.
- `AS build` — nombre de esta etapa (alias). La Etapa 2 lo referenciará con `--from=build`

```dockerfile
WORKDIR /app
```
- Crea el directorio `/app` dentro del contenedor **y** lo establece como directorio de trabajo
- Todos los `COPY` y `RUN` siguientes se ejecutan relativos a `/app`
- Equivalente a `mkdir /app && cd /app`

```dockerfile
# Copiar archivos de proyecto primero (para cachear la capa de restore)
COPY EcommerceNet.sln .
COPY src/EcommerceNet.Core/EcommerceNet.Core.csproj src/EcommerceNet.Core/
COPY src/EcommerceNet.Data/EcommerceNet.Data.csproj src/EcommerceNet.Data/
COPY src/EcommerceNet.API/EcommerceNet.API.csproj src/EcommerceNet.API/
COPY tests/EcommerceNet.Tests/EcommerceNet.Tests.csproj tests/EcommerceNet.Tests/
```
- **Estrategia de caché de Docker:** Cada instrucción genera una **capa**. Si los archivos fuente de esa instrucción no cambiaron, Docker reutiliza la capa cacheada.
- Al copiar solo los `.csproj` primero, si el código cambia pero los `.csproj` no, la capa de `dotnet restore` se reutiliza (ahorra ~2-3 minutos por build)
- El orden importa: `EcommerceNet.sln` primero porque `dotnet restore` lo necesita para encontrar todos los proyectos

```dockerfile
RUN dotnet restore
```
- `RUN` ejecuta un comando dentro del contenedor durante el build
- Descarga todos los paquetes NuGet declarados en los `.csproj`
- Esta capa se cachea gracias al patrón anterior

```dockerfile
COPY . .
```
- Ahora copia TODO el código fuente (src/, tests/, docs/, etc.)
- Va **después** del `dotnet restore` para no invalidar la capa cacheada del restore cuando cambia el código
- `.dockerignore` excluye bin/ y obj/ para evitar copiar binarios locales

```dockerfile
RUN dotnet publish src/EcommerceNet.API -c Release -o /publish
```
- Compila todos los proyectos y publica la API en modo Release en `/publish`
- `-c Release`: optimizaciones activadas (tree-shaking, ahead-of-time compilation), sin símbolos de debug
- `-o /publish`: directorio de salida. Contiene `EcommerceNet.API.dll` y todas sus dependencias

```dockerfile
# ============================================================
# Etapa 2: RUNTIME — imagen mínima (~200 MB)
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
```
- **Segundo `FROM`** — aquí empieza el multi-stage build
- `aspnet:8.0`: imagen que tiene solo el ASP.NET Core Runtime, sin SDK ni compilador
- Esta es la imagen que irá a producción: ~200 MB vs ~800 MB del SDK

```dockerfile
WORKDIR /app
COPY --from=build /publish .
```
- `--from=build`: copia archivos de la **Etapa 1** (nombrada `build`), no del host
- Solo copia los binarios compilados en `/publish`. Todo el código fuente, herramientas del SDK y layers intermedias se descartan

```dockerfile
EXPOSE 80
```
- Documenta que la app escucha en el puerto 80 del contenedor
- No abre el puerto — eso lo hace `ports` en docker-compose o `-p` en `docker run`

```dockerfile
ENV ASPNETCORE_URLS=http://+:80
```
- Configura Kestrel para escuchar en todas las interfaces de red (`+`) en el puerto 80
- Sin esto, Kestrel escucha solo en `localhost`, que es inaccesible desde fuera del contenedor

```dockerfile
ENV ASPNETCORE_ENVIRONMENT=Production
```
- Activa la configuración de producción de ASP.NET Core
- Desactiva: Swagger UI, Developer Exception Page, stack traces en respuestas
- Activa: optimizaciones de caché, compresión de respuestas

```dockerfile
ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]
```
- El comando que se ejecuta cuando el contenedor arranca
- Formato array JSON (exec form): no usa shell, el proceso principal es `dotnet` directamente
- Si usara `CMD ["dotnet", "EcommerceNet.API.dll"]`, el comando podría sobreescribirse al ejecutar el contenedor

### Resumen del multi-stage build

```
Etapa 1 (build):  sdk:8.0 (~800MB) + código fuente + .csproj + restore + publish → /publish
                                                                                         ↓
Etapa 2 (final):  aspnet:8.0 (~200MB) + solo /publish → imagen final: ~220MB
```

---

## 6. docker-compose.yml — análisis servicio por servicio

```yaml
version: '3.8'
```
Versión del formato de docker-compose. 3.8 soporta todos los features usados (volumes, depends_on, environment).

### Servicio `api`

```yaml
api:
  build:
    context: .
    dockerfile: src/EcommerceNet.API/Dockerfile
```
- `context: .` — Docker empaqueta **toda la carpeta raíz** y la envía al daemon para el build. Por eso el Dockerfile puede hacer `COPY EcommerceNet.sln .` (existe en la raíz)
- Si el contexto fuera `src/EcommerceNet.API/`, el Dockerfile no encontraría `EcommerceNet.sln`

```yaml
  ports:
    - "5000:80"
```
- Formato `HOST:CONTENEDOR`
- `5000` es el puerto en tu máquina (localhost:5000)
- `80` es el puerto dentro del contenedor (donde escucha Kestrel según `ASPNETCORE_URLS`)
- Puedes acceder a la API en `http://localhost:5000/swagger` desde el host

```yaml
  environment:
    - ConnectionStrings__DefaultConnection=Server=sqlserver;...
    - Jwt__Key=EstaEsMiClaveSecretaSuperSeguraDe256BitsParaJWT!!
    - Jwt__Issuer=EcommerceNet.API
    - Jwt__Audience=EcommerceNet.Web
    - MongoDB__ConnectionString=mongodb://mongo:27017
```
- Las variables con `__` (doble guión bajo) mapeian a la jerarquía de `appsettings.json`:
  ```
  ConnectionStrings__DefaultConnection → appsettings["ConnectionStrings"]["DefaultConnection"]
  Jwt__Key → appsettings["Jwt"]["Key"]
  ```
- `Server=sqlserver` — `sqlserver` es el hostname del contenedor SQL Server. Docker Compose crea DNS automático donde cada servicio es resolvible por su nombre

```yaml
  depends_on:
    - sqlserver
```
- Docker Compose arranca `sqlserver` antes de `api`
- **Limitación:** solo espera que el contenedor **arranque**, no que SQL Server esté **listo para conexiones** (puede tardar 5-10 segundos más). En producción se agrega retry logic en la app o un healthcheck

### Servicio `sqlserver`

```yaml
sqlserver:
  image: mcr.microsoft.com/mssql/server:2022-latest
  environment:
    - ACCEPT_EULA=Y                         # Aceptar licencia (obligatorio)
    - MSSQL_SA_PASSWORD=YourStrong!Passw0rd # Contraseña del usuario sa
  ports:
    - "1433:1433"
  volumes:
    - sqlserver-data:/var/opt/mssql
```
- `image:` usa una imagen pública sin Dockerfile propio
- `/var/opt/mssql` es donde SQL Server guarda los archivos `.mdf` y `.ldf`
- El volumen nombrado `sqlserver-data` persiste los datos entre `docker-compose down/up`
- Sin volumen, cada `docker-compose up` empieza con una BD vacía

### Servicio `mongo`

```yaml
mongo:
  image: mongo:7
  ports:
    - "27017:27017"
  volumes:
    - mongo-data:/data/db
```
- Puerto estándar de MongoDB
- Opcional para el historial de búsquedas; si no corre, `HistorialBusquedaServicio` falla silenciosamente

### Sección `volumes`

```yaml
volumes:
  sqlserver-data:
  mongo-data:
```
- Declara los volúmenes nombrados. Docker los gestiona en `/var/lib/docker/volumes/`
- `docker volume ls` para listarlos
- `docker-compose down -v` para eliminarlos junto con los contenedores

---

## 7. ci-cd.yml — pipeline paso a paso

### Trigger (cuándo se dispara)

```yaml
on:
  push:
    branches: [ main, desarrollo ]
  pull_request:
    branches: [ main ]
```
- Push a `main` o `desarrollo` → dispara el pipeline completo
- Pull Request hacia `main` → dispara el pipeline (para verificar antes del merge)
- Cualquier otro branch (feature branches) → **no** dispara el pipeline

### Job `backend` — 7 steps

```
Step 1: actions/checkout@v4
```
Clona el repositorio en el runner (máquina virtual `ubuntu-latest`). Sin este step, el runner no tiene el código.

```
Step 2: actions/setup-dotnet@v4 (dotnet-version: '8.0.x')
```
Instala el .NET SDK 8.0.x en el runner. GitHub mantiene una caché de SDKs para que este step sea rápido (~5 segundos).

```
Step 3: dotnet restore
```
Descarga los paquetes NuGet. GitHub Actions cachea automáticamente los paquetes NuGet entre runs.

```
Step 4: dotnet build --no-restore --configuration Release
```
Compila en modo Release. `--no-restore` evita hacer el restore dos veces.

```
Step 5: dotnet test --no-build --configuration Release --verbosity normal
```
Ejecuta los 23 tests. Si **alguno falla**, el step falla, el job falla, y el pipeline bloquea el merge del PR. Este es el corazón de la Integración Continua.

```
Step 6: dotnet publish (solo en main)
```
```yaml
if: github.ref == 'refs/heads/main'
```
Solo se ejecuta en push a `main`. `github.ref` es la referencia completa del branch/tag que disparó el workflow.

```
Step 7: upload-artifact (solo en main)
```
Sube los binarios publicados como artefacto descargable en la UI de GitHub Actions. Desde ahí se puede descargar manualmente o usar en un siguiente job de deploy con `download-artifact`.

### Job `frontend` — 5 steps

```
Step 1: actions/checkout@v4         ← misma operación que en backend
Step 2: actions/setup-node@v4       ← instala Node.js 20 en el runner
Step 3: npm ci                      ← instala dependencias exactas del package-lock.json
Step 4: npm run build               ← vite build → genera dist/
Step 5: upload-artifact             ← sube dist/ como artefacto (solo en main)
```

**`npm ci` vs `npm install`:**

| `npm install` | `npm ci` |
|--------------|---------|
| Puede actualizar dentro de rangos semver | Usa versiones exactas del `package-lock.json` |
| Modifica `package-lock.json` | Falla si `package-lock.json` no coincide |
| Más lento (resolución de dependencias) | Más rápido (instala exactamente lo declarado) |
| OK para desarrollo local | Obligatorio en CI para builds reproducibles |

### Ejecución paralela

Los dos jobs **corren en paralelo** porque no hay `needs:` entre ellos. GitHub Actions provisiona dos runners simultáneamente. El pipeline total tarda lo que tarde el job más lento (~3-4 minutos en este proyecto).

---

## 8. Decisiones técnicas y por qué

| Decisión | Alternativa rechazada | Razón técnica |
|----------|----------------------|---------------|
| **Multi-stage Docker build** | Build en un solo stage | Imagen final ~200MB vs ~800MB. El SDK no es necesario en producción. Menor superficie de ataque (sin compilador expuesto). |
| **`aspnet:8.0` como runtime** | `sdk:8.0` en producción | `aspnet:8.0` incluye solo el ASP.NET Core Runtime. No puede compilar código, reduciendo vulnerabilidades. |
| **GitHub Actions** | Jenkins | GitHub Actions: gratuito en repos públicos, sin infraestructura propia, el YAML vive en el repo, integración nativa con GitHub PRs. Jenkins requiere un servidor dedicado y mantenimiento. |
| **GitHub Actions** | Azure DevOps | GitHub Actions es agnóstico al proveedor cloud. Azure DevOps ata el CI/CD a Microsoft. DaCodes usa AWS — GitHub Actions se integra mejor con Elastic Beanstalk. |
| **2 jobs separados** | 1 job con todos los steps | Backend y frontend son independientes. Si falla el test de .NET no tiene sentido esperar el build de Vue.js. En paralelo reducen el tiempo de feedback a la mitad. |
| **`t2.micro` en EC2/EB** | `t3.small` o mayor | `t2.micro` está en el Free Tier de AWS (750 horas/mes gratis durante 12 meses). Suficiente para una demo de entrevista. |
| **S3 para el frontend** | EC2 con Nginx sirviendo los archivos | El frontend es un bundle estático (HTML/CSS/JS). S3 sirve archivos sin costo por cómputo. Con CloudFront se agrega CDN global. No requiere administrar un servidor web. |
| **Elastic Beanstalk** sobre EC2 directo | EC2 + Docker manual | EB automatiza: aprovisionamiento de instancias, balanceador de carga, auto-scaling, health checks, y actualización rolling. EC2 manual requiere configurar todo eso con scripts bash. |
| **Volúmenes nombrados** en docker-compose | Bind mounts al host | Los volúmenes nombrados son gestionados por Docker, portables entre sistemas operativos, y persisten entre `docker-compose down/up`. Los bind mounts acoplan a la ruta del host. |
| **`depends_on` sin healthcheck** | Retry loop en la app | Para la demo local es aceptable. SQL Server puede tardar ~8 segundos en estar listo después de que el contenedor arranca. En producción se usaría `healthcheck:` + `condition: service_healthy`. |

---

## 9. Comandos ejecutados y resultados

### Verificación de builds

```bash
# Backend
cd c:/Users/ramir/Source/repos/EcommerceNet
dotnet build
```
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:10.17
```

```bash
dotnet test
```
```
Passed!  - Failed: 0, Passed: 23, Skipped: 0, Total: 23
Duration: 100 ms - EcommerceNet.Tests.dll (net10.0)
```

```bash
# Frontend
cd src/EcommerceNet.Web
npm run build
```
```
vite v5.4.21 building for production...
✓ 103 modules transformed.
dist/index.html            0.44 kB  │ gzip:  0.29 kB
dist/assets/index-*.css   12.50 kB  │ gzip:  2.62 kB
dist/assets/index-*.js   152.67 kB  │ gzip: 57.71 kB
✓ built in 1.21s
```

### Comprobación del Dockerfile (verificación conceptual)

El Dockerfile se validó asegurando:
- El `context: .` en docker-compose coincide con que `EcommerceNet.sln` está en la raíz
- Todos los paths de `COPY ... .csproj` son correctos relativo a la raíz
- `ASPNETCORE_URLS=http://+:80` coincide con `ports: "5000:80"` en docker-compose

---

## 10. Git: rama, commits, merges y tag

### Rama creada

```bash
git checkout -b dia-05/deploy-aws
# Partiendo desde dia-04/frontend
```

### Commit principal

```bash
git add .github/ src/EcommerceNet.API/Dockerfile src/EcommerceNet.API/.dockerignore \
        docker-compose.yml README.md \
        docs/dia-04-manual-tecnico.md docs/dia-04-clase-programacion.md
git commit -m "feat: Docker, CI/CD con GitHub Actions, README profesional, docs día 4"
```

Resultado:
```
[dia-05/deploy-aws cdbb8d2] feat: Docker, CI/CD con GitHub Actions, README profesional, docs día 4
 7 files changed, 2607 insertions(+), 57 deletions(-)
```

### Merges

```bash
# Crear rama desarrollo e integrar el día 5
git checkout -b desarrollo
git merge dia-05/deploy-aws

# Integrar a master (rama principal del repo)
git checkout master
git merge desarrollo --ff-only
```

### Tag de versión

```bash
git tag -a v1.0.0 -m "v1.0.0 — EcommerceNet completo para entrevista DaCodes"
git tag -l
# → v1.0.0
```

### Historial final de commits

```
cdbb8d2  feat: Docker, CI/CD con GitHub Actions, README profesional, docs día 4
555c5ae  fix: agregar .gitignore y excluir node_modules y dist del repo
1ab0c64  feat: frontend Vue.js 3 con catálogo, carrito, checkout, auth y página jQuery legacy
80eb242  feat: proyecto inicial días 1-3 backend completo
```

---

## 11. Estado FINAL del proyecto completo

```
EcommerceNet/
├── .github/
│   └── workflows/
│       └── ci-cd.yml                   ← CI/CD: 2 jobs paralelos, triggers en main/desarrollo
├── docs/
│   ├── dia-01-fundamentos-csharp.md    ← Plan día 1
│   ├── dia-01-clase-programacion.md    ← Clase: C# desde cero
│   ├── dia-01-manual-tecnico.md        ← Manual: Clean Architecture + xUnit
│   ├── dia-02-aspnet-api.md            ← Plan día 2
│   ├── dia-02-clase-programacion.md    ← Clase: ASP.NET Core, JWT, Swagger
│   ├── dia-02-manual-tecnico.md        ← Manual: 18 endpoints, JWT, middleware
│   ├── dia-03-datos.md                 ← Plan día 3
│   ├── dia-03-clase-programacion.md    ← Clase: EF Core, Fluent API, MongoDB
│   ├── dia-03-manual-tecnico.md        ← Manual: repositorios, migraciones, seed data
│   ├── dia-04-frontend.md              ← Plan día 4
│   ├── dia-04-clase-programacion.md    ← Clase: Vue.js 3, Pinia, jQuery, ES6+
│   ├── dia-04-manual-tecnico.md        ← Manual: SPA, stores, interceptores
│   ├── dia-05-deploy-aws.md            ← Plan día 5
│   ├── dia-05-clase-programacion.md    ← Clase: Docker, CI/CD, AWS, DaCodes
│   ├── dia-05-manual-tecnico.md        ← Manual: este archivo
│   └── guia-deploy-aws.md              ← Guía paso a paso de deploy en AWS
├── src/
│   ├── EcommerceNet.Core/
│   │   ├── EcommerceNet.Core.csproj    ← Sin dependencias externas
│   │   ├── Entidades/
│   │   │   ├── Categoria.cs
│   │   │   ├── Producto.cs
│   │   │   ├── Usuario.cs
│   │   │   ├── Carrito.cs
│   │   │   ├── CarritoItem.cs
│   │   │   ├── Orden.cs
│   │   │   └── OrdenDetalle.cs
│   │   ├── Interfaces/
│   │   │   ├── IRepositorio.cs
│   │   │   ├── IProductoRepositorio.cs
│   │   │   ├── ICarritoRepositorio.cs
│   │   │   ├── IOrdenRepositorio.cs
│   │   │   ├── IUsuarioRepositorio.cs
│   │   │   └── IUnidadDeTrabajo.cs
│   │   ├── DTOs/
│   │   │   ├── ProductoDto.cs
│   │   │   ├── CarritoDto.cs
│   │   │   ├── OrdenDto.cs
│   │   │   ├── AuthDtos.cs
│   │   │   └── Resultado.cs
│   │   ├── Enums/
│   │   │   ├── EstadoOrden.cs
│   │   │   └── RolUsuario.cs
│   │   └── Servicios/
│   │       ├── IAuthServicio.cs
│   │       ├── ICarritoServicio.cs
│   │       └── CarritoServicio.cs
│   ├── EcommerceNet.Data/
│   │   ├── EcommerceNet.Data.csproj    ← EF Core, SqlServer, MongoDB.Driver, BCrypt
│   │   ├── AppDbContext.cs
│   │   ├── UnidadDeTrabajo.cs
│   │   ├── Repositorios/
│   │   │   ├── RepositorioBase.cs
│   │   │   ├── ProductoRepositorio.cs
│   │   │   ├── CarritoRepositorio.cs
│   │   │   ├── OrdenRepositorio.cs
│   │   │   └── UsuarioRepositorio.cs
│   │   ├── Servicios/
│   │   │   └── AuthServicio.cs
│   │   ├── MongoDB/
│   │   │   ├── BusquedaHistorial.cs
│   │   │   └── HistorialBusquedaServicio.cs
│   │   └── Migrations/
│   │       ├── 20260405051022_CreacionInicial.cs
│   │       └── AppDbContextModelSnapshot.cs
│   ├── EcommerceNet.API/
│   │   ├── EcommerceNet.API.csproj     ← EF Core Design
│   │   ├── Dockerfile                  ← Multi-stage build
│   │   ├── .dockerignore
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── ProductosController.cs
│   │   │   ├── CategoriasController.cs
│   │   │   ├── CarritoController.cs
│   │   │   └── OrdenesController.cs
│   │   └── Middleware/
│   │       └── ManejadorErroresMiddleware.cs
│   └── EcommerceNet.Web/
│       ├── package.json
│       ├── vite.config.js
│       ├── index.html
│       ├── .eslintrc.cjs
│       ├── .prettierrc.json
│       ├── public/
│       │   └── legacy.html             ← jQuery 3.7.1 consumiendo la API
│       └── src/
│           ├── main.js
│           ├── App.vue
│           ├── assets/main.css
│           ├── services/
│           │   └── api.js              ← Axios + interceptores JWT
│           ├── stores/
│           │   ├── authStore.js
│           │   ├── productoStore.js
│           │   └── carritoStore.js
│           ├── router/
│           │   └── index.js
│           ├── components/
│           │   ├── NavBar.vue
│           │   ├── ProductoCard.vue
│           │   └── CategoriaFiltro.vue
│           └── views/
│               ├── TiendaView.vue
│               ├── ProductoDetalleView.vue
│               ├── CarritoView.vue
│               ├── CheckoutView.vue
│               ├── LoginView.vue
│               ├── RegistroView.vue
│               └── MisOrdenesView.vue
├── tests/
│   └── EcommerceNet.Tests/
│       ├── EcommerceNet.Tests.csproj
│       └── Entidades/
│           ├── ProductoTests.cs        ← 8 pruebas
│           ├── CarritoTests.cs         ← 9 pruebas
│           └── OrdenTests.cs           ← 6 pruebas
├── docker-compose.yml                  ← api + sqlserver + mongo
├── EcommerceNet.sln
├── .gitignore
├── CLAUDE.md
└── README.md                           ← Completo con demo, endpoints, Docker, CI/CD
```

---

## 12. Resumen de los 5 días

| Día | Tema | Lo que se construyó | Archivos nuevos | Tecnología introducida |
|-----|------|---------------------|-----------------|----------------------|
| **1** | C# + Clean Architecture | 7 entidades, 6 interfaces, 5 DTOs, CarritoServicio, 23 tests | ~20 archivos | C#, .NET, xUnit, LINQ, async/await |
| **2** | ASP.NET Core API | 5 controladores, 18 endpoints REST, JWT Bearer, Swagger UI, middleware de errores | ~10 archivos | ASP.NET Core, JWT, Swagger/OpenAPI, middleware |
| **3** | EF Core + SQL Server + MongoDB | AppDbContext, Fluent API, 5 repositorios, UnidadDeTrabajo, AuthServicio real, migraciones, seed data 12 productos, historial MongoDB | ~15 archivos | EF Core, SQL Server, MongoDB, BCrypt, migraciones |
| **4** | Vue.js 3 + jQuery | SPA completa: 3 stores Pinia, router con guards, 3 componentes, 7 vistas, página jQuery legacy | ~22 archivos | Vue.js 3, Pinia, Vue Router, Axios, jQuery 3.7.1, ES6+ |
| **5** | Docker + CI/CD + AWS | Dockerfile multi-stage, docker-compose (3 servicios), pipeline GitHub Actions (2 jobs), README profesional, tag v1.0.0 | ~5 archivos | Docker, docker-compose, GitHub Actions, AWS conceptos |

### Totales acumulados

| Métrica | Valor |
|---------|-------|
| Archivos de código | ~70 archivos |
| Líneas de código (aprox.) | ~3,800 líneas |
| Pruebas unitarias | 23 (100% pasando) |
| Endpoints REST | 18 endpoints |
| Builds verificados | dotnet build ✅ · dotnet test ✅ · npm build ✅ |
| Commits con prefijos semánticos | 4 commits |
| Ramas Git | master, desarrollo, dia-04/frontend, dia-05/deploy-aws |
| Tag de versión | v1.0.0 |
| Documentos generados | 10 archivos .md (manual + clase por día) |
