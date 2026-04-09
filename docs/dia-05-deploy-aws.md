# Día 05 — Integración, despliegue en AWS y preparación para entrevista

> **Rama Git:** `dia-05/deploy-aws`  
> **Método:** 16 Pomodoros de 25 min (5 min descanso entre cada uno)  
> **Objetivo:** Pulir la app, desplegarla en AWS (DaCodes es AWS Partner), CI/CD con GitHub Actions, y simulacro completo de entrevista.

---

## Cronograma Pomodoro

| # | Bloque | Qué hacer |
|---|--------|-----------|
| 1 | Integración | Verificar flujo completo: registro → login → catálogo → carrito → checkout → orden |
| 2 | Integración | Arreglar bugs de CORS, validaciones, estados de carga, mensajes de error |
| 3 | CI/CD | Crear .github/workflows/ci-cd.yml para build + test automáticos |
| 4 | CI/CD | Push y verificar que el pipeline pasa en GitHub Actions |
| 5 | Docker | Crear Dockerfile para la API .NET |
| 6 | Docker | Crear docker-compose.yml con API + SQL Server |
| 7 | AWS | Crear cuenta AWS Free Tier, instalar AWS CLI |
| 8 | AWS | Opción A: Deploy manual en EC2 o Elastic Beanstalk |
| 9 | AWS | Configurar RDS (SQL Server) o usar SQLite para demo |
| 10 | AWS | Deploy del frontend en S3 + CloudFront (estático) |
| 11 | README | Actualizar README con capturas, URLs de deploy, instrucciones |
| 12 | Entrevista | Investigar DaCodes: Studios, Pods, clientes, cultura |
| 13 | Entrevista | Repasar preguntas técnicas C# / .NET / SQL / Vue.js |
| 14 | Entrevista | Simulacro de entrevista ronda 1: RH |
| 15 | Entrevista | Simulacro de entrevista ronda 2: técnica con Senior |
| 16 | Final | Merge final a main, crear tag v1.0.0, celebrar |

---

## Pomodoro 1-2 — Verificar integración end-to-end (50 min)

### Checklist de flujos

Ejecutar ambos servidores:

```powershell
# Terminal 1 — API
cd C:\Users\ramir\Source\repos\EcommerceNet\src\EcommerceNet.API
dotnet run

# Terminal 2 — Frontend
cd C:\Users\ramir\Source\repos\EcommerceNet\src\EcommerceNet.Web
npm run dev
```

Abrir `http://localhost:5173` y probar cada flujo:

| # | Flujo | Qué verificar |
|---|-------|--------------|
| 1 | Ver catálogo | Los 12 productos aparecen con imagen, precio, categoría |
| 2 | Buscar | Escribir "laptop" filtra en tiempo real |
| 3 | Filtrar por categoría | Click en "Electrónica" muestra solo 4 productos |
| 4 | Ver detalle | Click en un producto → imagen grande, descripción, selector +/- |
| 5 | Registrarse | Crear cuenta nueva → redirige a la tienda |
| 6 | Login | Cerrar sesión → login con las credenciales → funciona |
| 7 | Agregar al carrito | Agregar 3 productos distintos → badge muestra 3 |
| 8 | Ver carrito | Los 3 productos con precios correctos, total correcto |
| 9 | Cambiar cantidad | +/- actualiza subtotal y total en tiempo real |
| 10 | Eliminar del carrito | Quitar un producto → quedan 2 |
| 11 | Checkout | Poner dirección → confirmar → ver número de orden |
| 12 | Verificar stock | Volver a la tienda → el stock del producto se redujo |
| 13 | Mis órdenes | La orden aparece con estado "Pendiente" |
| 14 | Cancelar orden | Cancelar → estado cambia a "Cancelada" → stock se restaura |
| 15 | Admin | Login como admin@ecommercenet.com / Admin123! → crear producto |
| 16 | jQuery legacy | Abrir /legacy.html → productos cargan con AJAX |

### Problemas comunes y soluciones

| Problema | Causa | Solución |
|----------|-------|---------|
| CORS error en consola | La API no permite el origen del frontend | Verificar `WithOrigins("http://localhost:5173")` en Program.cs |
| 401 al agregar al carrito | Token no se envía | Verificar interceptor de Axios y que localStorage tenga el token |
| Productos sin categoría | Include no se ejecuta | Verificar `.Include(p => p.Categoria)` en ProductoRepositorio |
| Stock no se reduce | SaveChanges no se llama | Verificar `GuardarCambiosAsync()` en CarritoServicio.CheckoutAsync |
| Error "Failed to fetch" en jQuery | Diferente puerto | Verificar `API_URL` en legacy.html apunta al puerto correcto |

---

## Pomodoros 3-4 — CI/CD con GitHub Actions (50 min)

### Archivo: `.github/workflows/ci-cd.yml`

```yaml
name: CI/CD EcommerceNet

on:
  push:
    branches: [ main, desarrollo ]
  pull_request:
    branches: [ main ]

jobs:
  backend:
    name: Backend (.NET)
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout código
      uses: actions/checkout@v4

    - name: Configurar .NET SDK
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'

    - name: Restaurar dependencias
      run: dotnet restore

    - name: Compilar solución
      run: dotnet build --no-restore --configuration Release

    - name: Ejecutar pruebas
      run: dotnet test --no-build --configuration Release --verbosity normal

    - name: Publicar API
      if: github.ref == 'refs/heads/main'
      run: dotnet publish src/EcommerceNet.API -c Release -o ./publish

    - name: Subir artefacto
      if: github.ref == 'refs/heads/main'
      uses: actions/upload-artifact@v4
      with:
        name: api-publish
        path: ./publish

  frontend:
    name: Frontend (Vue.js)
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout código
      uses: actions/checkout@v4

    - name: Configurar Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '20'

    - name: Instalar dependencias
      working-directory: src/EcommerceNet.Web
      run: npm ci

    - name: Compilar frontend
      working-directory: src/EcommerceNet.Web
      run: npm run build

    - name: Subir artefacto
      if: github.ref == 'refs/heads/main'
      uses: actions/upload-artifact@v4
      with:
        name: frontend-dist
        path: src/EcommerceNet.Web/dist
```

### Push y verificar

```powershell
# Crear la carpeta de workflows
mkdir -p .github/workflows

# Hacer commit
git add .
git commit -m "ci: agregar pipeline CI/CD con GitHub Actions"
git push origin desarrollo

# Ir a github.com/TU-USUARIO/EcommerceNet/actions
# Verificar que el pipeline pasa: build ✅, test ✅
```

> **Concepto Senior: CI/CD**
> - **CI (Continuous Integration):** Cada push compila y ejecuta pruebas automáticamente. Si algo falla, lo sabes inmediatamente — no cuando el código llega a producción.
> - **CD (Continuous Deployment):** Si todo pasa, el código se despliega automáticamente al servidor.
>
> En DaCodes usan el modelo "Launch Pod" donde QA y DevOps están embebidos en el equipo.
> El pipeline de CI/CD es parte fundamental de ese modelo.

---

## Pomodoros 5-6 — Dockerización (50 min)

### Archivo: `src/EcommerceNet.API/Dockerfile`

> **IMPORTANTE (lección del deploy real):** El proyecto usa `net10.0` con paquetes `10.0.5`.
> El SDK debe ser `sdk:10.0`, NO `sdk:8.0` — el SDK 8 no soporta el formato `.slnx` (introducido en .NET 9+).
> El archivo de solución es `EcommerceNet.slnx`, NO `EcommerceNet.sln`.

```dockerfile
# Etapa 1: Compilar — SDK .NET 10 (el proyecto usa net10.0)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copiar archivos de solución y proyectos (para cachear restore)
COPY EcommerceNet.slnx .
COPY src/EcommerceNet.Core/EcommerceNet.Core.csproj src/EcommerceNet.Core/
COPY src/EcommerceNet.Data/EcommerceNet.Data.csproj src/EcommerceNet.Data/
COPY src/EcommerceNet.API/EcommerceNet.API.csproj src/EcommerceNet.API/
COPY tests/EcommerceNet.Tests/EcommerceNet.Tests.csproj tests/EcommerceNet.Tests/
RUN dotnet restore

# Copiar todo el código y publicar
COPY . .
RUN dotnet publish src/EcommerceNet.API -c Release -o /publish

# Etapa 2: Ejecutar (imagen ligera, sin SDK — ~200 MB)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /publish .

# Puerto de la API
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]
```

### Archivo: `docker-compose.yml` (raíz del proyecto)

```yaml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: src/EcommerceNet.API/Dockerfile
    ports:
      - "5000:80"
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=EcommerceNetDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
      - Jwt__Key=EstaEsMiClaveSecretaSuperSeguraDe256Bits!!
      - Jwt__Issuer=EcommerceNet.API
      - Jwt__Audience=EcommerceNet.Web
    depends_on:
      - sqlserver

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=YourStrong!Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql

volumes:
  sqlserver-data:
```

### Probar con Docker

```powershell
# Construir y ejecutar
docker-compose up --build

# Verificar: http://localhost:5000/swagger
# Detener: Ctrl+C o docker-compose down
```

> **¿Por qué Docker?**
> DaCodes usa Docker en su modelo "AWS Migration Pod" para containerizar aplicaciones
> antes de migrarlas a AWS. Tener un Dockerfile demuestra que entiendes el flujo DevOps completo:
> código → container → cloud.

---

## Pomodoros 7-10 — Despliegue en AWS (100 min)

### Opciones de despliegue (elige una)

| Opción | Dificultad | Costo | Ideal para |
|--------|-----------|-------|-----------|
| **A: Elastic Beanstalk** | Fácil | Free Tier | Demo rápida |
| **B: EC2 + Docker** | Media | Free Tier | Más control |
| **C: ECS Fargate** | Avanzada | ~$0.05/hr | Producción real |

### Opción A: Elastic Beanstalk — Lo que funcionó realmente

> **Nota:** El deploy requirió 3 intentos por errores del Dockerfile. Ver sección Troubleshooting abajo.

```powershell
# 1. Instalar AWS CLI (versión instalada: 2.34.27)
winget install Amazon.AWSCLI

# 2. Configurar credenciales (usuario IAM ecommercenet-deploy)
aws configure
# AWS Access Key ID: (de tu cuenta IAM)
# AWS Secret Access Key: (de tu cuenta IAM)
# Default region: us-east-1
# Default output format: json

# 3. Instalar Python y EB CLI
winget install Python.Python.3.12
pip install awsebcli
# EB CLI instalado: 3.27.1

# 4. Inicializar Elastic Beanstalk (desde la raíz del repo)
eb init EcommerceNet --platform Docker --region us-east-1

# 5. Crear .ebignore para que EB use el Dockerfile raíz (no docker-compose.yml)
# Contenido de .ebignore:
#   docker-compose.yml
#   src/EcommerceNet.Web/
#   .vs/
#   node_modules/
#   dist/
#   docs/

# 6. Crear entorno (usa el Dockerfile raíz)
eb create ecommercenet-api --single --instance-type t3.micro --timeout 20

# 7. Configurar variables de entorno
eb setenv Jwt__Key="EstaEsMiClaveSecretaSuperSeguraDe256BitsParaProduccion!!" Jwt__Issuer=EcommerceNet.API Jwt__Audience=EcommerceNet.Web ASPNETCORE_ENVIRONMENT=Production UseInMemoryDatabase=true

# 8. Verificar estado
eb status
# Health: Green ✅

# 9. Abrir en el navegador
eb open
# URL real: http://ecommercenet-api.eba-fxkridvp.us-east-1.elasticbeanstalk.com/swagger
```

### Troubleshooting — 3 errores reales durante el deploy

| # | Error | Causa | Fix |
|---|-------|-------|-----|
| 1 | `COPY EcommerceNet.sln: not found` | El archivo de solución es `.slnx` (formato .NET 9+), no `.sln` | Cambiar `COPY EcommerceNet.sln .` → `COPY EcommerceNet.slnx .` en ambos Dockerfiles |
| 2 | EB usaba `docker-compose.yml` en vez del `Dockerfile` raíz | EB prefiere `docker-compose.yml` cuando existe en el archivo | Crear `.ebignore` que excluya `docker-compose.yml` |
| 3 | `MSB1003: Specify a project or solution file` | `sdk:8.0` no soporta el formato `.slnx` (introducido en .NET 9+) | Cambiar `sdk:8.0` → `sdk:10.0` y `aspnet:8.0` → `aspnet:10.0` |

### Base de datos en AWS

```powershell
# Opción 1: RDS SQL Server Express (Free Tier)
aws rds create-db-instance \
  --db-instance-identifier ecommercenet-db \
  --db-instance-class db.t3.micro \
  --engine sqlserver-ex \
  --master-username admin \
  --master-user-password YourStrong!Passw0rd \
  --allocated-storage 20

# Opción 2: Usar SQLite para la demo (más simple)
# Cambiar el connection string a: "Data Source=ecommerce.db"
# Instalar: dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

### Frontend en S3 (hosting estático)

```powershell
# 1. Crear .env.production con la URL real de la API
# src/EcommerceNet.Web/.env.production:
# VITE_API_URL=http://ecommercenet-api.eba-fxkridvp.us-east-1.elasticbeanstalk.com/api

# 2. Build de producción
cd src/EcommerceNet.Web
npm run build

# 3. Crear bucket S3 (nombre único globalmente)
aws s3 mb s3://ecommercenet-ramiro671 --region us-east-1

# 4. Deshabilitar Block Public Access (requerido para hosting estático)
aws s3api put-public-access-block --bucket ecommercenet-ramiro671 --public-access-block-configuration "BlockPublicAcls=false,IgnorePublicAcls=false,BlockPublicPolicy=false,RestrictPublicBuckets=false"

# 5. Configurar para hosting estático
aws s3 website s3://ecommercenet-ramiro671 --index-document index.html --error-document index.html

# 6. Aplicar política de acceso público (bucket-policy.json)
aws s3api put-bucket-policy --bucket ecommercenet-ramiro671 --policy file://bucket-policy.json

# 7. Subir los archivos
aws s3 sync src/EcommerceNet.Web/dist/ s3://ecommercenet-ramiro671

# URL real del frontend:
# http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com
```

> **Para la entrevista:**
> "Desplegué la API en Elastic Beanstalk con Docker y el frontend como sitio estático en S3.
> DaCodes usa el modelo AWS Migration Pod — este es exactamente ese flujo: containerizar
> la app, configurar la infraestructura en AWS, y automatizar el deploy con CI/CD."

---

## Pomodoro 11 — README profesional (25 min)

Actualizar `README.md` con:

```markdown
## Demo en vivo

- **API (Swagger):** https://ecommercenet-env.elasticbeanstalk.com/swagger
- **Frontend:** http://ecommercenet-frontend.s3-website-us-east-1.amazonaws.com
- **Usuario demo:** demo@ecommercenet.com / Demo123!
- **Admin demo:** admin@ecommercenet.com / Admin123!

## Capturas de pantalla

| Catálogo | Carrito | Checkout |
|----------|---------|----------|
| ![catalogo](docs/img/catalogo.png) | ![carrito](docs/img/carrito.png) | ![checkout](docs/img/checkout.png) |

## Arquitectura

```
[Vue.js 3 SPA]  ←→  [ASP.NET Core API]  ←→  [SQL Server]
     ↑                      ↑                      ↑
   Pinia              JWT + Swagger           EF Core
   Router             Middleware              Migraciones
   Axios              Clean Architecture      Seed Data
                            ↓
                      [MongoDB] (búsquedas)
```
```

---

## Pomodoros 12-15 — Preparación para la entrevista (100 min)

### Pomodoro 12 — Investigar DaCodes

**Datos clave de la empresa:**

| Dato | Valor |
|------|-------|
| Fundación | 2014 |
| Sede | Houston, TX (oficinas en Mérida y CDMX) |
| Tamaño | 201-500 empleados, 233 en LinkedIn |
| Sector | Servicios de tecnología de la información |
| Certificaciones | Great Place to Work, ISO 27001 |
| Clientes | LATAM y Estados Unidos |

**Los 4 Studios:**
1. **Software Engineering & QA** — desarrollo fullstack + QA embebido + "Launch Pod"
2. **Cloud & DevOps** — AWS Partner + CI/CD + "AWS Migration Pod"
3. **AI & Data** — ML + GenAI + "GenAI Accelerator"
4. **Product Strategy & Design** — UX/UI + "Discovery Sprint"

**Proceso de entrevista (Glassdoor, 13 reportes):**
- Dificultad: Promedio
- Experiencia: 62% positiva, 15% negativa
- Rondas: RH → Técnica con Senior → Cliente (posible)
- Duración: 1-3 semanas
- Envían prueba técnica de código

### Pomodoro 13 — Preguntas técnicas completas

#### C# y .NET (10 preguntas)

**P1:** "¿Diferencia entre .NET Framework y .NET Core?"
> Framework: solo Windows, monolítico, IIS, System.Web. Core: multiplataforma, modular, Kestrel, paquetes NuGet. Core es la evolución — más rápido, más ligero, con DI nativa.

**P2:** "¿Qué es la inyección de dependencias?"
> Patrón donde las dependencias se pasan por constructor en lugar de crearlas con `new`. En .NET: AddTransient (nueva cada vez), AddScoped (una por request), AddSingleton (una para toda la app).

**P3:** "¿Qué es LINQ?"
> Language Integrated Query — permite consultar colecciones con sintaxis tipo SQL: Where, Select, OrderBy, GroupBy. Con EF Core, LINQ se traduce a SQL automáticamente.

**P4:** "¿IEnumerable vs IQueryable?"
> IEnumerable ejecuta en memoria (LINQ to Objects). IQueryable construye un árbol de expresiones que se traduce a SQL — el filtro se ejecuta en la BD, no en la app. Siempre usar IQueryable con EF Core.

**P5:** "¿Qué pasa si usas .Result en lugar de await?"
> Deadlock. El hilo se bloquea esperando el resultado, pero el resultado necesita ese mismo hilo para completarse. Siempre usar await — nunca .Result ni .Wait().

**P6:** "¿Patrón Repository y Unit of Work?"
> Repository abstrae el acceso a datos. Unit of Work agrupa múltiples operaciones en una transacción. SaveChanges va en UoW, no en el repositorio — así garantizas atomicidad.

**P7:** "¿Qué es middleware?"
> Componente en el pipeline HTTP que procesa cada request/response. El orden importa: errores → CORS → auth → autorización → controladores.

**P8:** "¿Cómo funciona JWT?"
> El usuario hace login, el servidor genera un token con claims (ID, rol). El frontend lo envía en cada request como `Authorization: Bearer {token}`. El middleware valida la firma y extrae los claims.

**P9:** "¿Fluent API vs Data Annotations?"
> Ambos configuran EF Core. Data Annotations: `[Required]` encima de la propiedad. Fluent API: `.IsRequired()` en OnModelCreating. Usamos Fluent API porque mantiene las entidades limpias y permite configuraciones más avanzadas.

**P10:** "¿Cómo migrarías un proyecto MVC a Core?"
> Gradualmente: extraer la lógica a una capa Core compartida, crear API en Core que expone JSON, reemplazar vistas Razor por SPA en Vue.js. Ambos coexisten durante la migración.

#### Vue.js (5 preguntas)

**P11:** "¿Options API vs Composition API?"
> Options organiza por tipo (data, methods, computed). Composition organiza por funcionalidad — todo lo del carrito junto. Composition es mejor para proyectos grandes, reutilización y TypeScript.

**P12:** "¿ref() vs reactive()?"
> `ref()` para primitivos y se accede con `.value`. `reactive()` para objetos y se accede directamente. En `<template>` Vue desenvuelve ref automáticamente. Uso ref por defecto.

**P13:** "¿Qué es Pinia?"
> Store oficial de Vue 3 que reemplazó a Vuex. Más simple: no tiene mutations, las acciones modifican el estado directamente. Cada store es un composable con ref, computed y funciones.

**P14:** "¿Cómo comunicas componentes padre-hijo?"
> Padre → hijo: props (defineProps). Hijo → padre: emits (defineEmits). Estado global: Pinia store. En mi proyecto, ProductoCard recibe `producto` como prop y emite `agregar` al padre.

**P15:** "¿Ciclo de vida de un componente?"
> setup → onBeforeMount → onMounted (aquí cargo datos) → onBeforeUpdate → onUpdated → onBeforeUnmount → onUnmounted. El más usado es onMounted para llamadas a API.

#### SQL Server (3 preguntas)

**P16:** "¿INNER JOIN vs LEFT JOIN?"
> INNER: solo filas con coincidencia en ambas tablas. LEFT: todas las filas de la izquierda + NULL donde no hay coincidencia. Uso LEFT JOIN cuando quiero ver categorías aunque no tengan productos.

**P17:** "¿Qué es un índice?"
> Estructura que acelera búsquedas — como el índice de un libro. Crearlos en columnas de WHERE, JOIN y ORDER BY frecuentes. No en tablas pequeñas ni columnas que cambian mucho.

**P18:** "¿Code First vs Database First?"
> Code First: escribes clases → EF crea tablas (proyectos nuevos). Database First: BD existente → EF genera clases (legacy). Mi proyecto usa Code First con migraciones.

### Pomodoros 14-15 — Simulacro de entrevista

#### Ronda 1: RH (Pomodoro 14)

**"Cuéntame de ti y tu experiencia."**
> Soy desarrollador con experiencia en el stack .NET y Vue.js. He trabajado con ASP.NET Core para APIs REST, Entity Framework para acceso a datos con SQL Server, y Vue.js 3 con Composition API para frontends interactivos. También tengo experiencia con jQuery en contextos de mantenimiento de sistemas legacy con ASP.NET MVC.

**"¿Por qué DaCodes?"**
> Me atrae su modelo de Studios especializados — especialmente Software Engineering & QA con el Launch Pod donde QA y DevOps están integrados desde el día uno. También que son AWS Partners, lo cual se alinea con mi experiencia reciente desplegando aplicaciones en AWS. Y valoro que ofrezcan acceso a certificaciones y clases de inglés.

**"¿Cuál es tu expectativa salarial?"**
> Estoy alineado con el rango de la vacante — entiendo que el tope es $35,000 MXN o $2,000 USD. Mi prioridad es el crecimiento profesional y la calidad de los proyectos.

#### Ronda 2: Técnica con Senior (Pomodoro 15)

**"Cuéntame de un proyecto reciente."**
> Construí EcommerceNet, una tienda en línea fullstack. Backend en ASP.NET Core 8 con Clean Architecture (Core, Data, API), autenticación JWT con roles, Entity Framework Core con SQL Server y MongoDB para búsquedas. Frontend en Vue.js 3 con Composition API, Pinia para estado, y una página legacy con jQuery. 22 pruebas unitarias con xUnit, CI/CD con GitHub Actions, y desplegado en AWS. El repositorio está público en GitHub.

**"¿Cómo organizaste la arquitectura?"**
> Clean Architecture con 4 capas: Core tiene entidades, interfaces y lógica pura sin dependencias externas. Data implementa los repositorios con EF Core. API tiene los controladores que solo traducen HTTP a llamadas de servicio. Tests prueba la lógica de Core. La dependencia es estricta: Core ← Data ← API. Esto permite cambiar la BD o el framework web sin tocar la lógica de negocio.

**"Muéstrame el flujo de un checkout."**
> El usuario hace POST a /api/carrito/checkout con la dirección. El CarritoController extrae el userId del JWT y llama a CarritoServicio.CheckoutAsync. El servicio valida que el carrito no esté vacío, verifica stock de cada producto, crea la orden con detalles, reduce stock con Producto.ReducirStock(), vacía el carrito, y guarda todo con GuardarCambiosAsync() — si cualquier paso falla, nada se guarda. Todo en una transacción.

---

## Pomodoro 16 — Merge final (25 min)

```powershell
# Commit final
git add .
git commit -m "feat: Docker, CI/CD con GitHub Actions, documentación de deploy AWS"

# Merge a desarrollo
git checkout desarrollo
git merge dia-05/deploy-aws
git push origin desarrollo

# Merge a main
git checkout main
git merge desarrollo
git push origin main

# Tag de versión
git tag -a v1.0.0 -m "v1.0.0 — EcommerceNet completo para entrevista DaCodes"
git push origin v1.0.0
```

---

## Simulador de entrevista DaCodes — Día 5

**Pregunta 1:** "¿Tienes experiencia con AWS?"
> "Sí, desplegué mi proyecto EcommerceNet en AWS: la API en Elastic Beanstalk con Docker y el frontend como sitio estático en S3. Configuré un pipeline de CI/CD con GitHub Actions que compila, ejecuta pruebas y publica automáticamente. Sé que DaCodes es AWS Partner y utiliza el modelo AWS Migration Pod — mi flujo de deploy sigue exactamente esa filosofía: containerizar, automatizar y migrar."

**Pregunta 2:** "¿Qué es Docker y por qué lo usas?"
> "Docker empaqueta la aplicación con todas sus dependencias en un contenedor que corre igual en cualquier máquina: mi laptop, el servidor de CI, o AWS. Uso un Dockerfile multi-stage: primero compilo con el SDK (imagen pesada) y luego copio solo los binarios a una imagen de runtime (ligera). Con docker-compose levanto la API y SQL Server juntos para desarrollo local."

**Pregunta 3:** "¿Cómo implementaste CI/CD?"
> "Con GitHub Actions. Tengo dos jobs: backend compila la solución .NET, ejecuta las 23 pruebas unitarias, y si estamos en main publica los artefactos. Frontend instala dependencias con npm ci y hace el build de Vue.js. Cualquier push a main o desarrollo dispara el pipeline. Si las pruebas fallan, el merge se bloquea. En DaCodes esto se alinea con el Launch Pod donde QA está embebido en el proceso desde el día uno."

---

## URLs de producción reales (deploy completado 2026-04-09)

| Recurso | URL |
|---------|-----|
| **API (Swagger)** | http://ecommercenet-api.eba-fxkridvp.us-east-1.elasticbeanstalk.com/swagger |
| **Frontend (S3)** | http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com |
| **Admin** | admin@ecommercenet.com / Admin123! |
| **Cliente** | demo@ecommercenet.com / Demo123! |

---

## Resumen de la semana completa

| Día | Qué construiste | Archivos clave |
|-----|----------------|----------------|
| **1** | Entidades, interfaces, DTOs, CarritoServicio, 23 tests | Core/ completo |
| **2** | 5 controladores, 18 endpoints, JWT, Swagger, middleware | API/Controllers/, Program.cs |
| **3** | EF Core, repositorios, migraciones, SQL avanzado, MongoDB | Data/ completo |
| **4** | Vue.js 3 SPA + jQuery legacy, 3 stores, 8 vistas | Web/ completo |
| **5** | Docker, CI/CD, AWS deploy, panel admin, InMemory DB | Dockerfile, .ebignore, appsettings.Production.json |

**Resultado:** Una tienda online fullstack desplegada en AWS con CI/CD, que demuestra dominio de CADA tecnología de la vacante de DaCodes.

**Tu ventaja competitiva:** Puedes abrir el repo en GitHub y mostrar commits organizados por días, pruebas pasando en CI/CD, la app corriendo en AWS, y explicar cada decisión técnica. La mayoría de candidatos solo hablan de su experiencia — tú la demuestras.
