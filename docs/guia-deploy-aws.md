# Guía de Deploy a AWS — EcommerceNet

> **Prerequisitos:** Cuenta AWS activa, Docker Desktop instalado.
> **Objetivo:** API en Elastic Beanstalk + Frontend en S3 + CI/CD verificado.
> **Tiempo estimado:** 45-60 minutos en total.
> **Todos los comandos son para PowerShell en Windows.**

---

## Índice

1. [Sección 1 — Verificar prerequisitos](#sección-1--verificar-prerequisitos)
2. [Sección 2 — Preparar la app para producción](#sección-2--preparar-la-app-para-producción)
3. [Sección 3 — Deploy del backend con Elastic Beanstalk](#sección-3--deploy-del-backend-con-elastic-beanstalk)
4. [Sección 4 — Deploy del frontend en S3](#sección-4--deploy-del-frontend-en-s3)
5. [Sección 5 — CORS en producción](#sección-5--cors-en-producción)
6. [Sección 6 — Verificación (checklist de 10 puntos)](#sección-6--verificación-checklist-de-10-puntos)
7. [Sección 7 — Actualizar README y Git](#sección-7--actualizar-readme-y-git)
8. [Sección 8 — Limpieza (para no pagar)](#sección-8--limpieza-para-no-pagar)
9. [Sección 9 — Alternativa sin AWS](#sección-9--alternativa-sin-aws)

---

## Sección 1 — Verificar prerequisitos

### Paso 1.1 — Verificar Docker Desktop

```powershell
docker --version
```

**Deberías ver:**
```
Docker version 27.x.x, build xxxxxxx
```

Si no está instalado: descarga Docker Desktop desde https://www.docker.com/products/docker-desktop/

**Verificar que Docker esté corriendo:**
```powershell
docker ps
```
Si ves una tabla vacía (sin error) → Docker está funcionando.
Si ves error "Cannot connect to Docker daemon" → abre Docker Desktop y espera que inicie.

### Paso 1.2 — Verificar AWS CLI

```powershell
aws --version
```

**Deberías ver:**
```
aws-cli/2.x.x Python/3.x.x Windows/10
```

**Si no está instalado:**
```powershell
# Opción A: con winget (Windows 11)
winget install Amazon.AWSCLI

# Opción B: descargar el instalador MSI
# Ir a: https://awscli.amazonaws.com/AWSCLIV2.msi
# Ejecutar el instalador y reiniciar PowerShell
```

Después de instalar, **cierra y vuelve a abrir PowerShell** para que el PATH se actualice.

### Paso 1.3 — Instalar EB CLI (Elastic Beanstalk CLI)

El EB CLI es una herramienta separada de la AWS CLI, especializada para Elastic Beanstalk.

**Prerequisito: Python y pip.**

```powershell
# Verificar Python
python --version
# Python 3.x.x

# Verificar pip
pip --version
# pip 23.x.x from ...
```

Si no tienes Python: descarga desde https://www.python.org/downloads/ e instala.

**Instalar EB CLI:**
```powershell
pip install awsebcli --upgrade
```

**Deberías ver:**
```
Successfully installed awsebcli-3.x.x ...
```

**Verificar instalación:**
```powershell
eb --version
```

**Deberías ver:**
```
EB CLI 3.x.x (Python 3.x.x)
```

### Paso 1.4 — Crear Access Keys en IAM Console

Antes de configurar la CLI, necesitas las credenciales de AWS:

1. Abre tu navegador y ve a: https://console.aws.amazon.com/iam
2. En el menú lateral izquierdo, haz clic en **Users**
3. Si no tienes un usuario IAM, créalo:
   - Haz clic en **Create user**
   - Nombre: `ecommercenet-deploy`
   - Marca: **Provide user access to the AWS Management Console** (opcional)
   - Haz clic en **Next**
   - Elige **Attach policies directly**
   - Busca y selecciona: `ElasticBeanstalkFullAccess`
   - Busca y selecciona: `AmazonS3FullAccess`
   - Haz clic en **Next** → **Create user**
4. Haz clic en el nombre del usuario que creaste
5. Haz clic en la pestaña **Security credentials**
6. Baja hasta **Access keys** y haz clic en **Create access key**
7. Selecciona **Command Line Interface (CLI)**
8. Marca la casilla de confirmación y haz clic en **Next**
9. Haz clic en **Create access key**
10. **IMPORTANTE:** Haz clic en **Download .csv file** — este archivo tiene tu Access Key ID y Secret Access Key. Solo se muestran una vez.

**Deberías tener un archivo CSV con:**
```
Access key ID,Secret access key
AKIAIOSFODNN7EXAMPLE,wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
```

### Paso 1.5 — Configurar credenciales AWS

```powershell
aws configure
```

**El CLI te pedirá cuatro valores (escríbelos y presiona Enter después de cada uno):**
```
AWS Access Key ID [None]: AKIAIOSFODNN7EXAMPLE
AWS Secret Access Key [None]: wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
Default region name [None]: us-east-1
Default output format [None]: json
```

- **Access Key ID:** el valor de la columna "Access key ID" de tu CSV
- **Secret Access Key:** el valor de la columna "Secret access key" de tu CSV
- **region:** `us-east-1` (Norte de Virginia — región principal, más servicios disponibles)
- **output format:** `json` (formato más completo para debugging)

**Verificar que las credenciales funcionan:**
```powershell
aws sts get-caller-identity
```

**Deberías ver:**
```json
{
    "UserId": "AIDAIOSFODNN7EXAMPLE",
    "Account": "123456789012",
    "Arn": "arn:aws:iam::123456789012:user/ecommercenet-deploy"
}
```

Si ves un error de `InvalidClientTokenId` → las credenciales son incorrectas. Regresa al Paso 1.4.

---

## Sección 2 — Preparar la app para producción

El problema con el deploy en AWS es que la API usa SQL Server LocalDB — que solo existe en Windows con Visual Studio instalado. En la instancia EC2 de EB no hay SQL Server.

**Solución para la demo:** Usar InMemory Database (base de datos en memoria) en producción. Los datos del seed se cargan al iniciar y se pierden al reiniciar — suficiente para la demo.

### Paso 2.1 — Instalar el paquete InMemory

```powershell
# Navegar a la raíz del repositorio
cd C:\Users\ramir\Source\repos\EcommerceNet

# Agregar el paquete InMemory al proyecto API
dotnet add src/EcommerceNet.API package Microsoft.EntityFrameworkCore.InMemory
```

**Deberías ver:**
```
info : Adding PackageReference for package 'Microsoft.EntityFrameworkCore.InMemory' into project '...'
```

### Paso 2.2 — Crear appsettings.Production.json

```powershell
# Crear el archivo de configuración de producción
```

Crea el archivo `src/EcommerceNet.API/appsettings.Production.json` con este contenido:

```json
{
  "UseInMemoryDatabase": true,
  "Jwt": {
    "Key": "EstaEsMiClaveSecretaSuperSeguraDe256BitsParaJWT!!",
    "Issuer": "EcommerceNet.API",
    "Audience": "EcommerceNet.Web"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Paso 2.3 — Modificar Program.cs para detectar el entorno

Abre `src/EcommerceNet.API/Program.cs` y modifica la sección de registro del DbContext.

Busca la línea que registra el DbContext (busca `AddDbContext`) y reemplaza con:

```csharp
// Detectar si usar InMemory (producción/demo) o SQL Server (desarrollo)
var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

if (useInMemory || builder.Environment.IsProduction())
{
    // Base de datos en memoria para la demo en AWS
    builder.Services.AddDbContext<AppDbContext>(opciones =>
        opciones.UseInMemoryDatabase("EcommerceNetDemo"));
}
else
{
    // SQL Server para desarrollo local
    builder.Services.AddDbContext<AppDbContext>(opciones =>
        opciones.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));
}
```

**Agregar el using necesario** al inicio de Program.cs si no está:
```csharp
using Microsoft.EntityFrameworkCore;
```

### Paso 2.4 — Verificar que el build funciona

```powershell
cd C:\Users\ramir\Source\repos\EcommerceNet
dotnet build
```

**Deberías ver:**
```
Build succeeded.
    0 Error(s)
    0 Warning(s)
```

```powershell
dotnet test
```

**Deberías ver:**
```
Passed! - Failed: 0, Passed: 23, Skipped: 0, Total: 23
```

### Paso 2.5 — Commit de los cambios

```powershell
git add src/EcommerceNet.API/appsettings.Production.json
git add src/EcommerceNet.API/Program.cs
git add src/EcommerceNet.API/EcommerceNet.API.csproj
git commit -m "feat: configurar InMemory DB para entorno de producción en AWS"
```

---

## Sección 3 — Deploy del backend con Elastic Beanstalk

### Paso 3.1 — Navegar al repositorio

```powershell
cd C:\Users\ramir\Source\repos\EcommerceNet
```

Verifica que estás en el directorio correcto:
```powershell
ls
```

**Deberías ver:** `EcommerceNet.sln`, `docker-compose.yml`, `src/`, `tests/`, `.github/`

### Paso 3.2 — Inicializar Elastic Beanstalk

```powershell
eb init
```

**El CLI te hará preguntas — responde así:**

```
Select a default region
1) us-east-1 : US East (N. Virginia)
...
(default is 3): 1

Enter Application Name
(default is "EcommerceNet"): EcommerceNet

It appears you are using Docker. Is this correct?
(Y/n): Y

Select a platform branch.
1) Docker running on 64bit Amazon Linux 2023
...
(default is 1): 1

Do you want to set up SSH for your instances?
(Y/n): n
```

**Deberías ver:**
```
Application EcommerceNet has been created.
```

Esto crea un archivo `.elasticbeanstalk/config.yml` en la raíz del repositorio.

### Paso 3.3 — Verificar que el Dockerfile existe

```powershell
ls src/EcommerceNet.API/Dockerfile
```

**Deberías ver:**
```
src/EcommerceNet.API/Dockerfile
```

Si no existe → hay un problema. El Dockerfile debe estar ahí desde el Día 5.

### Paso 3.4 — Crear el archivo Dockerrun.aws.json

Elastic Beanstalk necesita saber dónde está el Dockerfile cuando no está en la raíz. Crea este archivo en la raíz del repositorio:

**Crea `Dockerrun.aws.json`:**
```json
{
  "AWSEBDockerrunVersion": "1",
  "Image": {
    "Name": "ecommercenet-api",
    "Update": "true"
  },
  "Ports": [
    {
      "ContainerPort": "80"
    }
  ]
}
```

**Nota:** Elastic Beanstalk buscará el `Dockerfile` en la raíz del paquete que sube. Como nuestro Dockerfile está en `src/EcommerceNet.API/`, necesitamos un enfoque diferente.

**Crear un Dockerfile en la raíz del repositorio (VERSIÓN QUE FUNCIONÓ):**

> **⚠️ Errores críticos que ocurrieron y sus fixes:**
> 1. `COPY EcommerceNet.sln .` falla → el archivo es `.slnx` (formato .NET 9+). Fix: usar `EcommerceNet.slnx`
> 2. EB ignora el Dockerfile y usa `docker-compose.yml` → Fix: crear `.ebignore` que excluya `docker-compose.yml`
> 3. `sdk:8.0` no reconoce `.slnx` → Fix: usar `sdk:10.0` (el proyecto es `net10.0`)

Crea el archivo `Dockerfile` en la **raíz** del repositorio:

```dockerfile
# Dockerfile raíz — para Elastic Beanstalk
# IMPORTANTE: usa sdk:10.0 (no 8.0) — el proyecto es net10.0
# IMPORTANTE: usa EcommerceNet.slnx (no .sln) — formato .NET 9+

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY EcommerceNet.slnx .
COPY src/EcommerceNet.Core/EcommerceNet.Core.csproj src/EcommerceNet.Core/
COPY src/EcommerceNet.Data/EcommerceNet.Data.csproj src/EcommerceNet.Data/
COPY src/EcommerceNet.API/EcommerceNet.API.csproj src/EcommerceNet.API/
COPY tests/EcommerceNet.Tests/EcommerceNet.Tests.csproj tests/EcommerceNet.Tests/
RUN dotnet restore

COPY . .
RUN dotnet publish src/EcommerceNet.API -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /publish .
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]
```

**Crear `.ebignore` en la raíz (OBLIGATORIO):**

```
docker-compose.yml
src/EcommerceNet.Web/
.vs/
*.user
node_modules/
dist/
docs/
```

Sin este archivo, EB detecta `docker-compose.yml` y lo usa en lugar del `Dockerfile` raíz.

### Paso 3.5 — Crear el entorno en Elastic Beanstalk

Este comando crea la instancia EC2, el load balancer, el auto-scaling group y todo lo necesario. **Tarda entre 5 y 15 minutos.**

```powershell
eb create ecommercenet-api --single --instance-type t3.micro --timeout 20
```

- `ecommercenet-api` — nombre del entorno (aparecerá en la URL)
- `--single` — una sola instancia EC2 (sin load balancer, más económico)
- `--instance-type t3.micro` — instancia del Free Tier (t2.micro y t3.micro son elegibles)
- `--timeout 20` — esperar hasta 20 minutos antes de declarar timeout

**Durante el proceso verás muchas líneas como:**
```
Creating application version archive "app-xxxxxx".
Uploading EcommerceNet/app-xxxxxx.zip to S3. This may take a while.
Upload Complete.
Environment details for: ecommercenet-api
  Application name: EcommerceNet
  Region: us-east-1
  Deployed Version: app-xxxxxx
  Environment ID: e-xxxxxxxxxx
  Platform: Docker running on 64bit Amazon Linux 2023
  Tier: WebServer-Standard-1.0
  CNAME: ecommercenet-api.us-east-1.elasticbeanstalk.com
  Updated: 2026-04-09 ...
...
2026-04-09 ... INFO   Successfully launched environment: ecommercenet-api
```

**Al final deberías ver:**
```
INFO    Instance deployment completed successfully.
INFO    Application available at ecommercenet-api.us-east-1.elasticbeanstalk.com
INFO    Successfully launched environment: ecommercenet-api
```

### Paso 3.6 — Verificar el estado

```powershell
eb status
```

**Deberías ver:**
```
Environment details for: ecommercenet-api
  Application name: EcommerceNet
  ...
  Health: Green
  ...
  CNAME: ecommercenet-api.us-east-1.elasticbeanstalk.com
```

- **Health: Green** → todo funciona correctamente
- **Health: Yellow** → la app está iniciando, espera un minuto y vuelve a ejecutar `eb status`
- **Health: Red** → hay un error. Ejecuta `eb logs` para ver los logs de la aplicación

### Paso 3.7 — Abrir en el navegador

```powershell
eb open
```

Se abrirá tu navegador con la URL de la API. **Deberías ver:** la respuesta JSON de ASP.NET Core.

Para ver Swagger, agrega `/swagger` a la URL:
```
http://ecommercenet-api.us-east-1.elasticbeanstalk.com/swagger
```

**Deberías ver:** La interfaz de Swagger con todos los endpoints de la API.

### Paso 3.8 — Configurar variables de entorno

```powershell
eb setenv `
  Jwt__Key=EstaEsMiClaveSecretaSuperSeguraDe256BitsParaJWT!! `
  Jwt__Issuer=EcommerceNet.API `
  Jwt__Audience=EcommerceNet.Web `
  ASPNETCORE_ENVIRONMENT=Production
```

**Nota:** El backtick `` ` `` en PowerShell es el carácter de continuación de línea (equivale a `\` en bash).

Después de ejecutar esto, Elastic Beanstalk reinicia la aplicación automáticamente con las nuevas variables. Espera ~30 segundos.

**Verificar que funcionan:**
```powershell
# Probar el endpoint de productos (público)
Invoke-WebRequest -Uri "http://ecommercenet-api.us-east-1.elasticbeanstalk.com/api/productos" | Select-Object -ExpandProperty Content
```

**Deberías ver:** JSON con los 12 productos del seed data.

### Paso 3.9 — Anotar la URL de la API

Ejecuta esto y copia la URL:
```powershell
eb status | Select-String "CNAME"
```

La URL de tu API es:
```
http://ecommercenet-api.us-east-1.elasticbeanstalk.com
```

**Guarda esta URL** — la necesitarás en la Sección 4 para configurar el frontend.

---

## Sección 4 — Deploy del frontend en S3

### Paso 4.1 — Actualizar la URL de la API en el frontend

El frontend necesita saber la URL de la API en producción. Abre el archivo:
`src/EcommerceNet.Web/src/services/api.js`

Busca la línea con `baseURL` y agrega soporte para variable de entorno:

```javascript
// Cambia esto:
baseURL: 'http://localhost:5152/api'

// Por esto:
baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5152/api'
```

### Paso 4.2 — Crear archivo de variables de producción

Crea el archivo `src/EcommerceNet.Web/.env.production`:

```
VITE_API_URL=http://ecommercenet-api.us-east-1.elasticbeanstalk.com/api
```

**Reemplaza la URL** con la URL real de tu entorno (la que obtuviste en el Paso 3.9).

### Paso 4.3 — Build de producción del frontend

```powershell
cd C:\Users\ramir\Source\repos\EcommerceNet\src\EcommerceNet.Web
npm run build
```

**Deberías ver:**
```
vite v5.x.x building for production...
✓ 103 modules transformed.
dist/index.html         0.44 kB
dist/assets/index.css   12.50 kB
dist/assets/index.js    152.67 kB
✓ built in 1.43s
```

La carpeta `dist/` contiene los archivos estáticos listos para subir a S3.

### Paso 4.4 — Crear el bucket S3

**El nombre del bucket debe ser único globalmente** — si el nombre ya existe en cualquier cuenta de AWS del mundo, fallará. Usa tu nombre de usuario de GitHub para hacerlo único.

```powershell
aws s3 mb s3://ecommercenet-ramiro671
```

**Deberías ver:**
```
make_bucket: ecommercenet-ramiro671
```

Si ves error `BucketAlreadyExists` → cambia el nombre (agrega números: `ecommercenet-ramiro671-2026`).

### Paso 4.5 — Deshabilitar Block Public Access

Por defecto, AWS bloquea todo el acceso público a los buckets S3 (medida de seguridad). Para hosting estático necesitamos deshabilitarlo.

```powershell
aws s3api put-public-access-block `
  --bucket ecommercenet-ramiro671 `
  --public-access-block-configuration "BlockPublicAcls=false,IgnorePublicAcls=false,BlockPublicPolicy=false,RestrictPublicBuckets=false"
```

**Deberías ver:** (sin output = éxito)

### Paso 4.6 — Configurar para hosting estático

```powershell
aws s3 website s3://ecommercenet-ramiro671 `
  --index-document index.html `
  --error-document index.html
```

- `--index-document index.html` — el archivo que se sirve en la raíz (`/`)
- `--error-document index.html` — para que Vue Router funcione (todas las rutas devuelven `index.html` y Vue Router maneja la navegación)

**Deberías ver:** (sin output = éxito)

### Paso 4.7 — Crear la política de acceso público

Crea el archivo `bucket-policy.json` en la raíz del repositorio:

```powershell
cd C:\Users\ramir\Source\repos\EcommerceNet
```

Crea el archivo `bucket-policy.json` con este contenido:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "PublicReadGetObject",
      "Effect": "Allow",
      "Principal": "*",
      "Action": "s3:GetObject",
      "Resource": "arn:aws:s3:::ecommercenet-ramiro671/*"
    }
  ]
}
```

**Aplicar la política:**
```powershell
aws s3api put-bucket-policy `
  --bucket ecommercenet-ramiro671 `
  --policy file://bucket-policy.json
```

**Deberías ver:** (sin output = éxito)

### Paso 4.8 — Subir los archivos del frontend

```powershell
aws s3 sync src/EcommerceNet.Web/dist/ s3://ecommercenet-ramiro671
```

**Deberías ver:**
```
upload: src\EcommerceNet.Web\dist\index.html to s3://ecommercenet-ramiro671/index.html
upload: src\EcommerceNet.Web\dist\assets\index-xxxx.css to s3://ecommercenet-ramiro671/assets/index-xxxx.css
upload: src\EcommerceNet.Web\dist\assets\index-xxxx.js to s3://ecommercenet-ramiro671/assets/index-xxxx.js
```

### Paso 4.9 — Verificar el frontend en S3

La URL de tu frontend es:
```
http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com
```

Ábrela en el navegador. **Deberías ver:** La tienda EcommerceNet cargando.

Si ves "Access Denied" → el Paso 4.5 (deshabilitar Block Public Access) no se aplicó correctamente. Ve a la Consola de AWS → S3 → tu bucket → Permissions y verifica que "Block all public access" esté desactivado.

---

## Sección 5 — CORS en producción

El frontend en S3 intenta conectarse a la API en EB. El navegador bloqueará las peticiones si CORS no está configurado correctamente.

### Paso 5.1 — Actualizar CORS en Program.cs

Abre `src/EcommerceNet.API/Program.cs` y busca la sección de CORS (busca `WithOrigins`).

Agrega la URL de S3 a los orígenes permitidos:

```csharp
builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy("PoliticaCors", politica =>
    {
        politica
            .WithOrigins(
                "http://localhost:5173",           // desarrollo local
                "http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com"  // producción S3
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

**Alternativa para demo (más simple):**
```csharp
politica.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
```

### Paso 5.2 — Re-deploy del backend

```powershell
cd C:\Users\ramir\Source\repos\EcommerceNet
eb deploy
```

**Deberías ver:**
```
Uploading EcommerceNet/app-xxxxxx.zip to S3.
INFO    Environment update completed successfully.
```

### Paso 5.3 — Verificar CORS desde el frontend

Abre la URL de S3 en el navegador. Abre las DevTools (F12) → Console. Si no hay errores de CORS al cargar los productos → CORS está configurado correctamente.

---

## Sección 6 — Verificación (checklist de 10 puntos)

Abre la URL del frontend (`http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com`) y verifica cada punto:

### Checklist

| # | Qué verificar | Cómo verificarlo | Esperado |
|---|--------------|-----------------|---------|
| 1 | **API: Swagger carga** | Abrir `http://[URL-EB]/swagger` | Interfaz Swagger con los 5 grupos de endpoints |
| 2 | **API: Productos** | En Swagger, ejecutar `GET /api/productos` | JSON con 12 productos y categorías |
| 3 | **Frontend: carga** | Abrir URL de S3 | La tienda EcommerceNet con header, catálogo |
| 4 | **Frontend: catálogo** | Ver la página principal | 12 productos con imagen, nombre, precio, categoría |
| 5 | **Auth: registro** | Hacer clic en "Registrarse" → crear cuenta nueva | Redirige a la tienda con sesión activa |
| 6 | **Auth: login** | Cerrar sesión → hacer clic en "Iniciar Sesión" | JWT recibido, navbar muestra nombre |
| 7 | **Carrito: agregar** | Hacer clic en "Agregar al carrito" en un producto | Badge del carrito se actualiza |
| 8 | **Carrito: checkout** | Ir al carrito → ingresar dirección → confirmar | "Orden creada: ORD-XXXXXXXX" |
| 9 | **Órdenes: listar** | Hacer clic en "Mis Órdenes" | La orden aparece con estado "Pendiente" |
| 10 | **Admin: panel** | Login con `admin@ecommercenet.com / Admin123!` → clic "Admin Tienda" | Panel con tabs Productos y Categorías |

### Si algo falla

**Problema: productos no cargan en el frontend**
```
Causa: CORS bloqueando la petición o URL de API incorrecta en .env.production
Solución:
1. Abre DevTools (F12) → Console → busca errores CORS
2. Verifica que VITE_API_URL en .env.production apunta a la URL correcta de EB
3. Re-ejecuta: npm run build && aws s3 sync dist/ s3://ecommercenet-ramiro671
```

**Problema: Health Red en Elastic Beanstalk**
```powershell
# Ver los últimos logs de la aplicación
eb logs
```
Busca líneas con `[ERROR]` o `Exception` para identificar el problema.

**Problema: 502 Bad Gateway en la URL de EB**
```
Causa: La app está iniciando o hay un error de configuración
Solución: Espera 2 minutos y recarga. Si persiste, ejecuta: eb logs
```

---

## Sección 7 — Actualizar README y Git

### Paso 7.1 — Actualizar README.md

Abre `README.md` y agrega la sección de demo en vivo con tus URLs reales:

```markdown
## Demo en vivo

| Servicio | URL |
|---------|-----|
| **API (Swagger)** | http://ecommercenet-api.us-east-1.elasticbeanstalk.com/swagger |
| **Frontend** | http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com |
| **Usuario demo** | demo@ecommercenet.com / Demo123! |
| **Admin demo** | admin@ecommercenet.com / Admin123! |
```

### Paso 7.2 — Commit y push final

```powershell
cd C:\Users\ramir\Source\repos\EcommerceNet

git add .
git commit -m "feat: deploy a AWS - API en Elastic Beanstalk, frontend en S3, CORS configurado"
git push origin main
```

**Deberías ver:**
```
[main xxxxxxx] feat: deploy a AWS - API en Elastic Beanstalk, frontend en S3, CORS configurado
To https://github.com/Ramiro671/EcommerceNet.git
   xxxxxxx..xxxxxxx  main -> main
```

### Paso 7.3 — Verificar GitHub Actions

Ve a: https://github.com/Ramiro671/EcommerceNet/actions

Deberías ver un workflow corriendo (o recién completado) con:
- Backend (.NET): ✅ verde
- Frontend (Vue.js): ✅ verde

---

## Sección 8 — Limpieza (para no pagar)

**IMPORTANTE:** El Free Tier de AWS dura 12 meses desde la creación de la cuenta. Después de ese período (o si superas los límites), AWS te cobra. Para evitar cargos inesperados, elimina los recursos cuando no los necesites.

### Paso 8.1 — Eliminar el entorno de Elastic Beanstalk

```powershell
cd C:\Users\ramir\Source\repos\EcommerceNet
eb terminate ecommercenet-api
```

**El CLI te pedirá confirmación:**
```
The environment "ecommercenet-api" and all associated instances will be terminated.
To confirm, type the environment name: ecommercenet-api
```

Escribe `ecommercenet-api` y presiona Enter.

**Deberías ver:**
```
INFO    terminateEnvironment is starting.
INFO    Deleted CloudWatch alarm named: awseb-e-xxxxxxxxxx-stack-AWSEBCloudwatchAlarmHigh-...
INFO    Deleting SNS topic for environment ecommercenet-api.
INFO    Successfully terminated environment: ecommercenet-api
```

Este proceso elimina: la instancia EC2, el Auto Scaling Group, el Security Group, los logs de CloudWatch. **Todo.**

### Paso 8.2 — Eliminar el bucket S3 y los archivos

```powershell
aws s3 rb s3://ecommercenet-ramiro671 --force
```

- `rb` — remove bucket
- `--force` — elimina todos los objetos dentro del bucket antes de eliminarlo

**Deberías ver:**
```
delete: s3://ecommercenet-ramiro671/index.html
delete: s3://ecommercenet-ramiro671/assets/index-xxxx.css
...
remove_bucket: ecommercenet-ramiro671
```

### Paso 8.3 — Eliminar el archivo bucket-policy.json

```powershell
del bucket-policy.json
```

Este archivo tiene el nombre del bucket — no es necesario tenerlo en el repositorio.

### Verificar que no quedan recursos

Abre https://console.aws.amazon.com/billing/home → **Bills** → verifica que no hay cargos pendientes.

También puedes verificar en https://console.aws.amazon.com/ec2 que no hay instancias EC2 corriendo.

---

## Sección 9 — Alternativa sin AWS (si algo falla)

Si el deploy en AWS presenta problemas, estas alternativas son más simples y también son gratuitas:

### Opción A — Backend en Railway

Railway soporta aplicaciones .NET con Docker y tiene un tier gratuito generoso.

```powershell
# Instalar Railway CLI
npm install -g @railway/cli

# Login
railway login

# Inicializar (desde la raíz del repositorio)
cd C:\Users\ramir\Source\repos\EcommerceNet
railway init

# Selecciona "Empty Project"
# Nombre: EcommerceNet

# Configurar variables de entorno
railway variables set ASPNETCORE_ENVIRONMENT=Production
railway variables set Jwt__Key=EstaEsMiClaveSecretaSuperSeguraDe256BitsParaJWT!!
railway variables set Jwt__Issuer=EcommerceNet.API
railway variables set Jwt__Audience=EcommerceNet.Web

# Deploy
railway up
```

Railway detecta el Dockerfile automáticamente y construye la imagen.

**Deberías ver:**
```
☁️ Deployment complete! Your project is live at: https://ecommercenet-XXXX.up.railway.app
```

### Opción B — Frontend en Vercel

Vercel soporta aplicaciones Vue.js nativamente.

```powershell
# Instalar Vercel CLI
npm install -g vercel

# Navegar al frontend
cd C:\Users\ramir\Source\repos\EcommerceNet\src\EcommerceNet.Web

# Login y deploy
vercel
```

**El CLI te hará preguntas:**
```
Set up and deploy "EcommerceNet.Web"? [Y/n] Y
Which scope do you want to deploy to? [tu cuenta]
Link to existing project? [n] n
What's your project's name? ecommercenet-web
In which directory is your code located? ./
Auto-detected Project Settings (Vite):
- Build Command: vite build
- Output Directory: dist
- Install Command: npm install
? Want to override the settings? [y/N] N
```

**Deberías ver:**
```
✅  Production: https://ecommercenet-web.vercel.app [32s]
```

### Opción C — Usar SQLite en lugar de InMemory

Si prefieres persistencia en el deploy, SQLite es una base de datos de archivo que no requiere servidor:

```powershell
# Agregar paquete SQLite
dotnet add src/EcommerceNet.API package Microsoft.EntityFrameworkCore.Sqlite
```

En `appsettings.Production.json`:
```json
{
  "UseSQLite": true,
  "SQLiteConnectionString": "Data Source=/tmp/ecommerce.db"
}
```

En `Program.cs`, agregar la condición para SQLite. Los datos persisten entre reinicios de la app (mientras el contenedor no se elimine).

---

## Sección 9 — Troubleshooting (errores reales del deploy)

Estos errores ocurrieron durante el deploy real (2026-04-09) y requirieron 3 intentos de `eb create`.

### Error 1: `COPY EcommerceNet.sln: not found`

**Síntoma:** El build de Docker en EB falla en la primera instrucción COPY.

**Causa:** El archivo de solución se llama `EcommerceNet.slnx` (formato introducido en .NET 9+). El Dockerfile original hacía `COPY EcommerceNet.sln .` — ese archivo no existe.

**Fix:** En el Dockerfile (raíz y en `src/EcommerceNet.API/`), cambiar:
```dockerfile
COPY EcommerceNet.sln .     # ← INCORRECTO
COPY EcommerceNet.slnx .    # ← CORRECTO
```

### Error 2: EB usa `docker-compose.yml` en vez del `Dockerfile` raíz

**Síntoma:** EB detecta el `docker-compose.yml` existente y lo prioriza sobre el `Dockerfile` raíz. El compose levanta 3 servicios (API + SQL Server + MongoDB) pero EB no puede descargar las imágenes sin configuración adicional.

**Causa:** Elastic Beanstalk tiene este orden de precedencia: `docker-compose.yml` > `Dockerfile`. Como el repo tenía `docker-compose.yml`, EB lo usó.

**Fix:** Crear `.ebignore` en la raíz con `docker-compose.yml` listado. `.ebignore` funciona como `.gitignore` para el archivo ZIP que EB sube a S3 — lo que está en `.ebignore` no se incluye en el deploy.

### Error 3: `MSB1003: Specify a project or solution file`

**Síntoma:** `dotnet restore` dentro del contenedor Docker falla con "MSB1003: no se pudo encontrar ningún archivo de proyecto o solución".

**Causa:** `sdk:8.0` no soporta el formato `.slnx`. El formato `.slnx` fue introducido con el SDK de .NET 9. Como el proyecto usa `net10.0`, se necesita `sdk:10.0`.

**Fix:** En ambos Dockerfiles, cambiar:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build    # ← SDK 8 no soporta .slnx
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build   # ← SDK 10 sí soporta .slnx

FROM mcr.microsoft.com/dotnet/aspnet:8.0           # ← Runtime incorrecto
FROM mcr.microsoft.com/dotnet/aspnet:10.0          # ← Runtime correcto para net10.0
```

### Cómo verificar si EB está usando el Dockerfile correcto

Después del error, si necesitas terminar un entorno fallido y crear uno nuevo:
```powershell
eb terminate ecommercenet-api --force
# Esperar ~5 minutos
eb create ecommercenet-api --single --instance-type t3.micro --timeout 20
```

---

## Resumen de URLs

Una vez completado el deploy, tus URLs de producción son:

| Recurso | URL |
|---------|-----|
| **API (Swagger)** | `http://ecommercenet-api.eba-fxkridvp.us-east-1.elasticbeanstalk.com/swagger` |
| **API (base)** | `http://ecommercenet-api.eba-fxkridvp.us-east-1.elasticbeanstalk.com/api` |
| **Frontend** | `http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com` |
| **GitHub Repo** | `https://github.com/Ramiro671/EcommerceNet` |
| **GitHub Actions** | `https://github.com/Ramiro671/EcommerceNet/actions` |

## Credenciales de demo

| Tipo | Email | Contraseña |
|------|-------|-----------|
| **Admin** | admin@ecommercenet.com | Admin123! |
| **Cliente** | cliente@ecommercenet.com | Cliente123! |

## Lo que puedes decir en la entrevista

> "Desplegué EcommerceNet en AWS: la API en Elastic Beanstalk con Docker — un par de comandos (`eb init`, `eb create`) y la instancia EC2 está lista — y el frontend como sitio estático en S3. Configuré IAM para no usar el usuario root, las variables de entorno sensibles se pasan a EB sin tocar el código, y el pipeline de GitHub Actions compila, prueba y publica los artefactos automáticamente en cada push a `main`. Sé que DaCodes usa el modelo AWS Migration Pod y que son AWS Partner — este flujo es exactamente eso: containerizar, automatizar y migrar a AWS."
