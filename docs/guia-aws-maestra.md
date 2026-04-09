# Guía AWS Maestra — EcommerceNet

> **Documento autosuficiente:** Alguien que solo lea este archivo puede configurar AWS desde
> cero y desplegar EcommerceNet completo sin consultar ningún otro documento.
>
> **Entorno:** Windows 11, PowerShell, VS Code
> **Resultado:** API en Elastic Beanstalk (Docker + .NET 10, t3.micro) + Frontend en S3
> **URLs de producción:**
> - API: http://ecommercenet-api.eba-fxkridvp.us-east-1.elasticbeanstalk.com/swagger
> - Frontend: http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com

---

## Índice

1. [Parte 1 — Prerequisitos e instalación](#parte-1--prerequisitos-e-instalación)
2. [Parte 2 — Preparar la app para producción](#parte-2--preparar-la-app-para-producción)
3. [Parte 3 — Docker](#parte-3--docker)
4. [Parte 4 — Deploy del backend con Elastic Beanstalk](#parte-4--deploy-del-backend-con-elastic-beanstalk)
5. [Parte 5 — Deploy del frontend en S3](#parte-5--deploy-del-frontend-en-s3)
6. [Parte 6 — CORS y conexión frontend ↔ backend](#parte-6--cors-y-conexión-frontend--backend)
7. [Parte 7 — CI/CD con GitHub Actions](#parte-7--cicd-con-github-actions)
8. [Parte 8 — Troubleshooting (errores REALES del deploy)](#parte-8--troubleshooting-errores-reales-del-deploy)
9. [Parte 9 — Verificación final (checklist de 10 puntos)](#parte-9--verificación-final-checklist-de-10-puntos)
10. [Parte 10 — Costos y limpieza](#parte-10--costos-y-limpieza)
11. [Parte 11 — Respuestas de entrevista sobre AWS/DevOps](#parte-11--respuestas-de-entrevista-sobre-awsdevops)

---

## Parte 1 — Prerequisitos e instalación

### 1.1 Crear cuenta AWS Free Tier

Si ya tienes cuenta, salta al paso 1.2.

1. Ve a https://aws.amazon.com/free/
2. Click **Create a Free Account**
3. Ingresa email, nombre de cuenta
4. Verificar email con el código enviado
5. Datos de tarjeta de crédito (cargo temporal de $1 USD, se revierte)
6. Verificar identidad por teléfono
7. Seleccionar plan **Basic Support (Free)**

**Free Tier incluye (primeros 12 meses):**

| Servicio | Límite gratis |
|---------|--------------|
| EC2 (t2.micro o t3.micro) | 750 horas/mes |
| S3 | 5 GB de storage |
| Elastic Beanstalk | Gratis (pagas la EC2) |
| RDS (db.t3.micro) | 750 horas/mes |

### 1.2 Configurar MFA en la cuenta root

AWS muestra una alerta hasta que configures MFA. Es obligatorio para seguridad.

1. Ve a https://console.aws.amazon.com/iam/ → **Security credentials**
2. En "Assign MFA device":
   - **Device name:** `mi-celular`
   - **MFA device:** seleccionar **Authenticator app** (no Passkey — Passkey requiere hardware FIDO2)
3. Te muestra un código QR
4. Abre **Google Authenticator** o **Microsoft Authenticator** en tu celular
5. Escanea el QR
6. Ingresa **2 códigos consecutivos** (espera a que cambie para el segundo)
7. Click **Assign MFA**

### 1.3 Crear usuario IAM para deploy

**NUNCA uses la cuenta root para deploy.** Crea un usuario con permisos limitados.

1. Ve a https://console.aws.amazon.com/iam/ → **Users** → **Create user**
2. **User name:** `ecommercenet-deploy`
3. Click **Next** → **Attach policies directly**
4. Busca y marca estas políticas:

| Búsqueda | Política |
|----------|---------|
| `ElasticBeanstalk` | **AdministratorAccess-AWSElasticBeanstalk** |
| `S3Full` | **AmazonS3FullAccess** |
| `RDSFull` | **AmazonRDSFullAccess** |

5. Click **Next** → **Create user**

### 1.4 Crear Access Keys para CLI

1. Click en el usuario **ecommercenet-deploy** → tab **Security credentials**
2. Bajar a "Access keys" → **Create access key**
3. Seleccionar **Command Line Interface (CLI)** → marcar confirmación
4. **Description tag:** `CLI EcommerceNet`
5. Click **Create access key**

```
Access Key ID:     AKIA................  ← copiar
Secret Access Key: xxxxxxxxxxxxxxxxxxxx  ← copiar (SOLO SE MUESTRA UNA VEZ)
```

> **⚠️ La Secret Access Key solo se muestra ahora.** Guárdala en un password manager.

### 1.5 Instalar AWS CLI en Windows

```powershell
# Opción A: con winget (recomendada)
winget install Amazon.AWSCLI

# Opción B: descarga directa del MSI
# https://awscli.amazonaws.com/AWSCLIV2.msi
```

**Cerrar y reabrir PowerShell** después de instalar. Verificar:

```powershell
aws --version
# aws-cli/2.34.27 Python/3.14.3 Windows/11 exe/AMD64
```

> **Error común:** Si dice "aws is not recognized", cierra TODAS las ventanas de PowerShell y abre una nueva.

### 1.6 Instalar Python y EB CLI

EB CLI requiere Python.

```powershell
# Paso 1: Instalar Python 3.12
winget install Python.Python.3.12
# → cerrar y reabrir PowerShell

python --version
# Python 3.12.10

# Paso 2: Instalar EB CLI
pip install awsebcli

eb --version
# EB CLI 3.27.1 (Python 3.12.10 ...)
```

> **Nota:** EB CLI queda en `C:\Users\ramir\AppData\Local\Programs\Python\Python312\Scripts\eb.exe`
> Si el PATH no se actualiza automáticamente, usa la ruta completa en PowerShell.

### 1.7 Configurar credenciales con aws configure

```powershell
aws configure
```

Responde **una por una** (NO pegues todo junto):

```
AWS Access Key ID [None]:     ← pegar el Access Key ID y Enter
AWS Secret Access Key [None]: ← pegar el Secret Access Key y Enter
Default region name [None]:   us-east-1
Default output format [None]: json
```

> **Error que ocurrió:** Si pegas las respuestas en PowerShell ANTES de ejecutar `aws configure`,
> PowerShell lo interpreta como comandos y falla. Primero ejecuta el comando, después responde.

### 1.8 Verificar que todo funciona

```powershell
aws sts get-caller-identity
```

Resultado esperado:
```json
{
    "UserId": "AIDA...............",
    "Account": "578101931920",
    "Arn": "arn:aws:iam::578101931920:user/ecommercenet-deploy"
}
```

**Resumen de herramientas instaladas:**

| Herramienta | Versión verificada | Comando de verificación |
|-------------|-------------------|------------------------|
| AWS CLI | 2.34.27 | `aws --version` |
| Python | 3.12.10 | `python --version` |
| EB CLI | 3.27.1 | `eb --version` |

---

## Parte 2 — Preparar la app para producción

El problema con AWS es que la API usa SQL Server LocalDB — que solo existe en Windows con Visual Studio.
La instancia EC2 de Elastic Beanstalk corre Linux, sin SQL Server.

**Solución:** InMemory Database para la demo en AWS. Los datos del seed se cargan al iniciar y se pierden al reiniciar — suficiente para mostrar la funcionalidad completa.

### 2.1 Instalar el paquete InMemory

```powershell
cd C:\Users\ramir\Source\repos\EcommerceNet

dotnet add src/EcommerceNet.API package Microsoft.EntityFrameworkCore.InMemory
# → versión instalada: 10.0.5
```

### 2.2 Crear appsettings.Production.json

Crear `src/EcommerceNet.API/appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Jwt": {
    "Key": "EstaEsMiClaveSecretaSuperSeguraDe256BitsParaProduccion!!",
    "Issuer": "EcommerceNet.API",
    "Audience": "EcommerceNet.Web",
    "ExpireMinutes": 120
  },
  "UseInMemoryDatabase": true
}
```

> En AWS, las variables de entorno configuradas con `eb setenv` sobreescriben este archivo.

### 2.3 Modificar Program.cs para detectar el entorno

En `src/EcommerceNet.API/Program.cs`, reemplazar el registro del DbContext:

```csharp
// Detectar si usar InMemory (producción/demo) o SQL Server (desarrollo local)
if (builder.Configuration.GetValue<bool>("UseInMemoryDatabase"))
{
    // Base de datos en memoria para la demo en AWS (sin necesidad de RDS)
    builder.Services.AddDbContext<AppDbContext>(opciones =>
        opciones.UseInMemoryDatabase("EcommerceNetDB"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(opciones =>
        opciones.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));
}
```

**Agregar seeding con EnsureCreated** al final de Program.cs (antes de `app.Run()`):

```csharp
// Seed data para InMemory DB (EnsureCreated aplica el HasData de OnModelCreating)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}
```

**Habilitar Swagger en todos los entornos** (necesario para la demo en AWS):

```csharp
// SIEMPRE habilitar Swagger (no solo en Development)
app.UseSwagger();
app.UseSwaggerUI(opciones =>
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "EcommerceNet API v1");
    opciones.RoutePrefix = "swagger";
});

// HTTPS redirect solo en desarrollo (EB maneja HTTP en el puerto 80)
if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
```

### 2.4 Verificar build y tests

```powershell
cd C:\Users\ramir\Source\repos\EcommerceNet
dotnet build
# Build succeeded. 0 Error(s). 0 Warning(s).

dotnet test
# Passed! - Failed: 0, Passed: 23, Skipped: 0, Total: 23
```

---

## Parte 3 — Docker

### 3.1 ¿Por qué Docker?

Sin Docker, para desplegar en AWS necesitarías:
- Instalar .NET runtime en el servidor
- Instalar todas las dependencias manualmente
- Configurar el servidor para ejecutar la app

Con Docker:
- La app y todas sus dependencias van en una imagen
- La imagen corre igual en tu laptop, en CI, y en AWS
- EB solo necesita el `Dockerfile` — él hace el resto

**VM vs Contenedor:**

| | Máquina Virtual | Contenedor Docker |
|-|----------------|------------------|
| **Incluye** | OS completo + app | Solo app + dependencias |
| **Tamaño** | ~GB | ~MB a ~GB |
| **Inicio** | Minutos | Segundos |
| **Aislamiento** | Hardware virtualizado | Kernel compartido |

### 3.2 Dockerfile raíz (para Elastic Beanstalk)

El Dockerfile correcto que funciona en producción (después de resolver 3 errores):

```dockerfile
# Dockerfile raíz — para Elastic Beanstalk
# Multi-stage build: compilar con SDK (.NET 10), ejecutar con runtime ligero

# Etapa 1: COMPILAR (~800 MB, incluye compilador C#)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copiar primero los .csproj (truco de caché: restore solo se re-ejecuta si cambian)
COPY EcommerceNet.slnx .
COPY src/EcommerceNet.Core/EcommerceNet.Core.csproj src/EcommerceNet.Core/
COPY src/EcommerceNet.Data/EcommerceNet.Data.csproj src/EcommerceNet.Data/
COPY src/EcommerceNet.API/EcommerceNet.API.csproj src/EcommerceNet.API/
COPY tests/EcommerceNet.Tests/EcommerceNet.Tests.csproj tests/EcommerceNet.Tests/
RUN dotnet restore

# Copiar el código y publicar en Release
COPY . .
RUN dotnet publish src/EcommerceNet.API -c Release -o /publish

# Etapa 2: EJECUTAR (~200 MB, solo runtime — sin compilador, sin código fuente)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /publish .

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]
```

**Por qué multi-stage:**
- Etapa 1 (build): `sdk:10.0` pesa ~800 MB. Incluye compilador, NuGet, herramientas.
- Etapa 2 (run): `aspnet:10.0` pesa ~200 MB. Solo el runtime de ASP.NET Core.
- La imagen final tiene **4x menos tamaño** y no contiene el código fuente ni el compilador.

### 3.3 Archivo .ebignore (CRÍTICO para Elastic Beanstalk)

Crear `.ebignore` en la raíz del repositorio:

```
docker-compose.yml
src/EcommerceNet.Web/
.vs/
*.user
node_modules/
dist/
docs/
```

**¿Por qué es crítico?** Elastic Beanstalk tiene este orden de precedencia para detectar cómo ejecutar la app:
1. `docker-compose.yml` (si existe) → EB lo usa, ignora el `Dockerfile`
2. `Dockerfile` (si existe) → EB lo construye y ejecuta

Como el repositorio tiene `docker-compose.yml` (para desarrollo local), EB lo detectaría y lo usaría — fallando porque el compose levanta SQL Server y MongoDB que no están disponibles en EB.

`.ebignore` funciona como `.gitignore` para el ZIP que EB sube a S3. Con `docker-compose.yml` excluido, EB solo ve el `Dockerfile` raíz.

### 3.4 Verificar localmente (si Docker Desktop está corriendo)

```powershell
cd C:\Users\ramir\Source\repos\EcommerceNet

# Construir la imagen
docker build -t ecommercenet-api .

# Ejecutar localmente
docker run -p 5000:80 ecommercenet-api

# Verificar: http://localhost:5000/swagger
```

---

## Parte 4 — Deploy del backend con Elastic Beanstalk

### 4.1 ¿Qué es Elastic Beanstalk?

EB es un servicio **PaaS** (Platform as a Service) de AWS. La diferencia con EC2 (IaaS):

| | EC2 (IaaS) | Elastic Beanstalk (PaaS) |
|-|-----------|--------------------------|
| **Tú administras** | Todo (OS, servidor, app) | Solo la aplicación |
| **AWS administra** | Hardware | Hardware + OS + runtime |
| **Configuración** | Manual | Automática |
| **Flexibilidad** | Total | Limitada al entorno |

Para un proyecto de demo, EB es la opción correcta: 2 comandos y la app está corriendo.

### 4.2 Inicializar Elastic Beanstalk

```powershell
cd C:\Users\ramir\Source\repos\EcommerceNet

eb init EcommerceNet --platform Docker --region us-east-1
```

Si el CLI pide confirmaciones interactivas:
- **Region:** `1` (us-east-1)
- **Application name:** `EcommerceNet`
- **Platform:** Docker
- **SSH:** `n`

Esto crea `.elasticbeanstalk/config.yml` en la raíz.

### 4.3 Crear el entorno

```powershell
eb create ecommercenet-api --single --instance-type t3.micro --timeout 20
```

- `ecommercenet-api` — nombre del entorno (aparece en la URL)
- `--single` — una sola instancia, sin load balancer (más económico para demo)
- `--instance-type t3.micro` — elegible para Free Tier
- `--timeout 20` — espera hasta 20 min

**Lo que pasa en los 5-10 minutos de espera:**
1. EB sube el ZIP del repo a S3 (excluido lo que está en `.ebignore`)
2. Crea una instancia EC2 t3.micro en Linux
3. Instala Docker en la instancia
4. Ejecuta `docker build` con el Dockerfile raíz
5. Inicia el contenedor con la app
6. Crea un Security Group con el puerto 80 abierto

**Salida esperada al final:**
```
INFO    Instance deployment completed successfully.
INFO    Application available at ecommercenet-api.eba-fxkridvp.us-east-1.elasticbeanstalk.com
INFO    Successfully launched environment: ecommercenet-api
```

### 4.4 Verificar el estado

```powershell
eb status
```

```
Environment details for: ecommercenet-api
  Application name: EcommerceNet
  Health: Green          ← esto es lo que buscamos
  CNAME: ecommercenet-api.eba-fxkridvp.us-east-1.elasticbeanstalk.com
```

- **Health: Green** → todo funciona
- **Health: Yellow** → iniciando, espera 1 min
- **Health: Red** → error, ejecuta `eb logs` para ver el problema

### 4.5 Configurar variables de entorno

```powershell
eb setenv `
  Jwt__Key="EstaEsMiClaveSecretaSuperSeguraDe256BitsParaProduccion!!" `
  Jwt__Issuer=EcommerceNet.API `
  Jwt__Audience=EcommerceNet.Web `
  ASPNETCORE_ENVIRONMENT=Production `
  UseInMemoryDatabase=true
```

> El backtick `` ` `` en PowerShell es el carácter de continuación de línea.
> Después de `eb setenv`, EB reinicia el contenedor con las nuevas variables (~30 segundos).

**¿Por qué `Jwt__Key` y no `Jwt:Key`?**
En .NET, los dos guiones bajos `__` son el separador jerárquico para variables de entorno.
`Jwt__Key` equivale a `appsettings["Jwt"]["Key"]`.

### 4.6 Verificar en el navegador

```powershell
eb open
```

Se abre el navegador. Agrega `/swagger` a la URL para ver la documentación interactiva.

**URL real:** http://ecommercenet-api.eba-fxkridvp.us-east-1.elasticbeanstalk.com/swagger

---

## Parte 5 — Deploy del frontend en S3

### 5.1 Actualizar la URL de la API en el frontend

El frontend necesita saber la URL de la API en producción (no localhost).

Crear `src/EcommerceNet.Web/.env.production`:

```
VITE_API_URL=http://ecommercenet-api.eba-fxkridvp.us-east-1.elasticbeanstalk.com/api
```

Vite lee este archivo automáticamente durante `npm run build` y reemplaza
`import.meta.env.VITE_API_URL` en el código compilado.

### 5.2 Build de producción del frontend

```powershell
cd C:\Users\ramir\Source\repos\EcommerceNet\src\EcommerceNet.Web
npm run build
```

Salida esperada:
```
vite v5.x.x building for production...
✓ 103 modules transformed.
dist/index.html         0.44 kB
dist/assets/index.css   12.50 kB
dist/assets/index.js    152.67 kB
✓ built in 1.43s
```

La carpeta `dist/` contiene los archivos estáticos listos para S3.

### 5.3 Crear el bucket S3

```powershell
cd C:\Users\ramir\Source\repos\EcommerceNet

"C:\Program Files\Amazon\AWSCLIV2\aws.exe" s3 mb s3://ecommercenet-ramiro671 --region us-east-1
# make_bucket: ecommercenet-ramiro671
```

> El nombre del bucket debe ser único globalmente en todos los buckets de AWS del mundo.
> Usa tu nombre de usuario de GitHub para garantizar unicidad.

### 5.4 Deshabilitar Block Public Access

Por defecto S3 bloquea todo el acceso público. Para hosting estático hay que habilitarlo.

```powershell
"C:\Program Files\Amazon\AWSCLIV2\aws.exe" s3api put-public-access-block `
  --bucket ecommercenet-ramiro671 `
  --public-access-block-configuration "BlockPublicAcls=false,IgnorePublicAcls=false,BlockPublicPolicy=false,RestrictPublicBuckets=false"
```

Sin output = éxito.

### 5.5 Configurar hosting estático

```powershell
"C:\Program Files\Amazon\AWSCLIV2\aws.exe" s3 website s3://ecommercenet-ramiro671 `
  --index-document index.html `
  --error-document index.html
```

- `--error-document index.html` — para que Vue Router funcione: cualquier ruta devuelve
  `index.html` y Vue Router maneja la navegación del lado del cliente.

### 5.6 Aplicar política de acceso público

Crear `bucket-policy.json` en la raíz del repo:

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

Aplicar la política:

```powershell
"C:\Program Files\Amazon\AWSCLIV2\aws.exe" s3api put-bucket-policy `
  --bucket ecommercenet-ramiro671 `
  --policy file://bucket-policy.json
```

### 5.7 Subir los archivos del frontend

```powershell
"C:\Program Files\Amazon\AWSCLIV2\aws.exe" s3 sync src/EcommerceNet.Web/dist/ s3://ecommercenet-ramiro671
```

Salida esperada:
```
upload: src\EcommerceNet.Web\dist\index.html to s3://ecommercenet-ramiro671/index.html
upload: src\EcommerceNet.Web\dist\assets\index-xxxx.css to s3://...
upload: src\EcommerceNet.Web\dist\assets\index-xxxx.js to s3://...
```

**URL del frontend:**
http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com

---

## Parte 6 — CORS y conexión frontend ↔ backend

El frontend (S3) y la API (EB) están en dominios diferentes. El navegador bloquea
peticiones cross-origin por defecto (política de Same-Origin). CORS es el mecanismo
que permite que un origen llame a otro.

### 6.1 Actualizar CORS en Program.cs

```csharp
builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy("PermitirVue", politica =>
    {
        politica.WithOrigins(
                    "http://localhost:5173",    // desarrollo local
                    "http://localhost:5000",    // docker-compose local
                    "http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com"  // S3
                )
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});
```

Y en el middleware:

```csharp
app.UseCors("PermitirVue");  // debe ir ANTES de UseAuthentication
```

### 6.2 Re-deploy del backend con los cambios de CORS

```powershell
cd C:\Users\ramir\Source\repos\EcommerceNet
C:\Users\ramir\AppData\Local\Programs\Python\Python312\Scripts\eb.exe deploy
```

Salida esperada:
```
Uploading EcommerceNet/app-xxxxxx.zip to S3.
INFO    Environment update completed successfully.
```

### 6.3 Verificar CORS

Abre el frontend en S3, abre DevTools (F12) → Console. Si los productos cargan sin errores
CORS → CORS está configurado correctamente.

---

## Parte 7 — CI/CD con GitHub Actions

### 7.1 ¿Qué es CI/CD?

- **CI (Continuous Integration):** Cada push compila y ejecuta pruebas automáticamente.
  Si algo falla, lo sabes en minutos — no cuando el código llega a producción.
- **CD (Continuous Deployment):** Si CI pasa, el código se publica automáticamente.

En DaCodes, el modelo **Launch Pod** del Software Engineering Studio incluye CI/CD desde
el día uno del proyecto. Nuestro pipeline sigue esa filosofía.

### 7.2 Archivo: `.github/workflows/ci-cd.yml`

```yaml
name: CI/CD EcommerceNet

# Se activa en cada push a main o desarrollo, y en PRs hacia main
on:
  push:
    branches: [ main, desarrollo ]
  pull_request:
    branches: [ main ]

jobs:
  # Job 1: Backend .NET
  backend:
    name: Backend (.NET)
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout código
      uses: actions/checkout@v4

    - name: Configurar .NET SDK
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'    # debe coincidir con el TargetFramework del proyecto

    - name: Restaurar dependencias
      run: dotnet restore

    - name: Compilar solución
      run: dotnet build --no-restore --configuration Release

    - name: Ejecutar pruebas
      run: dotnet test --no-build --configuration Release --verbosity normal
      # Si cualquier prueba falla → el job falla → el merge a main se bloquea

    - name: Publicar API
      if: github.ref == 'refs/heads/main'    # solo en main (no en desarrollo)
      run: dotnet publish src/EcommerceNet.API -c Release -o ./publish

    - name: Subir artefacto de API
      if: github.ref == 'refs/heads/main'
      uses: actions/upload-artifact@v4
      with:
        name: api-publish
        path: ./publish

  # Job 2: Frontend Vue.js (corre en paralelo con el backend)
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
      run: npm ci    # npm ci usa package-lock.json exacto (más reproducible que npm install)

    - name: Compilar frontend
      working-directory: src/EcommerceNet.Web
      run: npm run build

    - name: Subir artefacto del frontend
      if: github.ref == 'refs/heads/main'
      uses: actions/upload-artifact@v4
      with:
        name: frontend-dist
        path: src/EcommerceNet.Web/dist
```

### 7.3 Verificar en GitHub

Ve a https://github.com/Ramiro671/EcommerceNet/actions después de hacer push.

Deberías ver:
- **Backend (.NET):** `dotnet restore` → `dotnet build` → `dotnet test` (23/23) → `dotnet publish` ✅
- **Frontend (Vue.js):** `npm ci` → `npm run build` ✅

Los dos jobs corren **en paralelo** — reducen el tiempo total de CI de ~4 min a ~2.5 min.

---

## Parte 8 — Troubleshooting (errores REALES del deploy)

El deploy en AWS requirió **3 intentos** de `eb create`. Cada intento falló con un error diferente.
Aquí están los 3 errores, sus causas exactas y los fixes aplicados.

### Error 1: `COPY EcommerceNet.sln: not found`

**Contexto:** Primer intento de `eb create`. La instancia EC2 construye el Dockerfile y falla.

**Síntoma en `eb logs`:**
```
Step 4/13 : COPY EcommerceNet.sln .
ERROR [build 4/13] COPY EcommerceNet.sln .: "/EcommerceNet.sln": not found
```

**Causa:** El Dockerfile original tenía `COPY EcommerceNet.sln .`. Pero con .NET 10 (SDK 9+),
`dotnet new sln` crea `EcommerceNet.slnx` (nuevo formato XML introducido en .NET 9).
El archivo `.sln` nunca existió en este proyecto.

**Fix aplicado:**
```dockerfile
# Antes (incorrecto):
COPY EcommerceNet.sln .

# Después (correcto):
COPY EcommerceNet.slnx .
```

**Archivos modificados:**
- `Dockerfile` (raíz)
- `src/EcommerceNet.API/Dockerfile`

**Commit:** `74ff410 fix: corregir EcommerceNet.sln → slnx en Dockerfiles, agregar .ebignore para EB`

---

### Error 2: EB usa docker-compose.yml en vez del Dockerfile raíz

**Contexto:** Segundo intento de `eb create`. El entorno arranca pero la app no funciona.

**Síntoma en `eb logs`:**
```
Starting services with docker compose...
Container "sqlserver" failed to start: unable to pull image mcr.microsoft.com/mssql/server:2022-latest
```

**Causa:** Elastic Beanstalk tiene este orden de detección:
1. Si existe `docker-compose.yml` en el ZIP → lo usa
2. Si existe `Dockerfile` → lo construye

Como el repositorio tiene `docker-compose.yml` (para desarrollo local con SQL Server + MongoDB),
EB lo detectó y intentó levantar 3 contenedores. SQL Server no pudo descargarse en la instancia
porque requiere configuración adicional de imágenes.

**Fix aplicado:** Crear `.ebignore` en la raíz:
```
docker-compose.yml
src/EcommerceNet.Web/
.vs/
*.user
node_modules/
dist/
docs/
```

`.ebignore` funciona como `.gitignore` para el archivo ZIP que EB sube a S3. Con `docker-compose.yml`
excluido, EB solo ve el `Dockerfile` raíz y lo construye correctamente.

**Commit:** `74ff410` (mismo commit que el Error 1 — se detectaron juntos)

---

### Error 3: SDK .NET 8 no soporta formato .slnx

**Contexto:** Tercer intento de `eb create`, después de los fixes 1 y 2. El Dockerfile
ahora encuentra el `.slnx`, pero el restore falla.

**Síntoma en `eb logs`:**
```
Step 6/13 : RUN dotnet restore
MSBUILD: error MSB1003: Specify a project or solution file.
The current working directory does not contain a project or solution file.
```

**Causa:** El Dockerfile original usaba `FROM mcr.microsoft.com/dotnet/sdk:8.0`. El formato
`.slnx` fue introducido en el SDK de .NET 9. El SDK 8 no lo reconoce — para él, el directorio
no tiene ningún archivo de solución válido.

El proyecto usa `<TargetFramework>net10.0</TargetFramework>` con todos los paquetes en versión
`10.0.5`. Necesita SDK 10, no 8.

**Fix aplicado:**
```dockerfile
# Antes (incorrecto — SDK 8 no soporta .slnx):
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
...
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Después (correcto — SDK 10 soporta net10.0 y .slnx):
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
...
FROM mcr.microsoft.com/dotnet/aspnet:10.0
```

**Commit:** `2470e85 fix: usar SDK .NET 10 en Dockerfiles — proyecto usa net10.0, no net8.0`

**Resultado:** Tercer intento exitoso. Health: Green en ~8 minutos.

---

### Error 4: `aws` y `eb` no encontrados en bash shell

**Contexto:** Claude Code usa el Bash tool con shell `bash`. AWS CLI y EB CLI están instalados
en Windows pero no están en el PATH del shell bash.

**Fix:**
- AWS CLI: usar ruta completa `"C:\Program Files\Amazon\AWSCLIV2\aws.exe"` o `powershell.exe -Command "aws ..."`
- EB CLI: usar ruta completa `C:\Users\ramir\AppData\Local\Programs\Python\Python312\Scripts\eb.exe`

---

### Error 5: Credenciales pegadas directamente en PowerShell

**Contexto:** Al intentar responder las preguntas de `aws configure`, las credenciales se
pegaron en la terminal ANTES de ejecutar el comando.

**Síntoma:** PowerShell interpreta el texto como comandos:
```
The term 'AKIA...' is not recognized as the name of a cmdlet, function, script file...
```

**Fix:** Primero ejecutar `aws configure`, DESPUÉS responder cada pregunta cuando la solicita.

---

### Error 6: `eb terminate` para entornos fallidos

Cuando un entorno de EB falla y necesitas crearlo de nuevo:

```powershell
# Terminar el entorno fallido (sin pedir confirmación)
C:\Users\ramir\AppData\Local\Programs\Python\Python312\Scripts\eb.exe terminate nombre-entorno --force

# Esperar ~5 minutos (EB elimina todos los recursos AWS)
# Luego crear el entorno de nuevo con los fixes aplicados
C:\Users\ramir\AppData\Local\Programs\Python\Python312\Scripts\eb.exe create ecommercenet-api --single --instance-type t3.micro --timeout 20
```

---

## Parte 9 — Verificación final (checklist de 10 puntos)

Abre el frontend en http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com
y verifica cada punto:

| # | Qué verificar | Cómo verificarlo | Esperado |
|---|--------------|-----------------|---------|
| 1 | **API: Swagger carga** | Abrir URL de EB + `/swagger` | Interfaz Swagger con 5 grupos de endpoints |
| 2 | **API: Productos** | En Swagger, `GET /api/productos` | JSON con 12 productos del seed data |
| 3 | **Frontend: carga** | Abrir URL de S3 | La tienda EcommerceNet con header y catálogo |
| 4 | **Catálogo completo** | Ver la página principal | 12 productos con imagen, nombre, precio, categoría |
| 5 | **Auth: registro** | Click "Registrarse" → crear cuenta | Redirige a la tienda con sesión activa |
| 6 | **Auth: login** | Cerrar sesión → "Iniciar Sesión" | JWT recibido, navbar muestra nombre |
| 7 | **Carrito: agregar** | Click "Agregar al carrito" | Badge del carrito se actualiza |
| 8 | **Checkout** | Ir al carrito → dirección → confirmar | "Orden creada: ORD-XXXXXXXX" |
| 9 | **Mis Órdenes** | Click "Mis Órdenes" | La orden aparece con estado "Pendiente" |
| 10 | **Panel Admin** | Login admin@ecommercenet.com / Admin123! → "Admin Tienda" | Panel con tabs Productos y Categorías |

### Si algo falla

**Productos no cargan (error CORS):**
```
1. Abrir DevTools (F12) → Console → buscar errores de tipo "CORS policy"
2. Verificar que Program.cs tiene la URL de S3 en WithOrigins(...)
3. Re-deploy: eb deploy
```

**Health Red en EB:**
```powershell
eb logs
# Buscar líneas con [ERROR] o Exception para identificar el problema
```

**"Access Denied" en S3:**
```
Verificar en consola AWS → S3 → tu bucket → Permissions:
- Block all public access: desactivado
- Bucket policy: con la acción s3:GetObject para Principal: *
```

---

## Parte 10 — Costos y limpieza

### ¿Cuánto cuesta?

Durante los primeros 12 meses (Free Tier):

| Servicio | Costo | Límite free |
|---------|-------|------------|
| EC2 t3.micro (EB) | $0.00 | 750 horas/mes |
| S3 Storage | $0.00 | 5 GB |
| S3 Requests | $0.00 | 20,000 GET, 2,000 PUT |
| Elastic Beanstalk | $0.00 | (pagas la EC2) |
| **Total** | **$0.00** | — |

> **Después del Free Tier:** t3.micro cuesta ~$0.01/hora. Con 720 horas/mes = ~$7.50/mes.

### Eliminar TODO cuando ya no necesites la demo

```powershell
# 1. Terminar el entorno de Elastic Beanstalk
# (elimina la EC2, Security Group, load balancer, logs de CloudWatch)
C:\Users\ramir\AppData\Local\Programs\Python\Python312\Scripts\eb.exe terminate ecommercenet-api --force

# 2. Eliminar el bucket S3 y todos los archivos
"C:\Program Files\Amazon\AWSCLIV2\aws.exe" s3 rb s3://ecommercenet-ramiro671 --force

# 3. Verificar que no quedan recursos huérfanos
# → https://console.aws.amazon.com/billing/home → Bills
# → https://console.aws.amazon.com/ec2 → Instances (verify "No instances")
```

---

## Parte 11 — Respuestas de entrevista sobre AWS/DevOps

### "¿Tienes experiencia con AWS?"

> "Sí, desplegué EcommerceNet en AWS: la API en Elastic Beanstalk con Docker y el frontend
> como sitio estático en S3. Configuré un usuario IAM con permisos mínimos, las variables de
> entorno sensibles se pasan a EB con `eb setenv` sin tocar el código, y el pipeline de CI/CD
> con GitHub Actions compila, prueba y publica artefactos automáticamente en cada push.
> Sé que DaCodes es AWS Partner y usa el modelo AWS Migration Pod — mi flujo de deploy
> sigue exactamente esa filosofía: containerizar, automatizar y migrar a AWS."

### "¿Qué es Docker y por qué lo usas?"

> "Docker empaqueta la aplicación con todas sus dependencias en un contenedor que corre igual
> en cualquier máquina: mi laptop, el servidor de CI, o AWS. Uso un Dockerfile multi-stage:
> primero compilo con el SDK completo (~800 MB) y luego copio solo los binarios compilados
> a una imagen de runtime ligera (~200 MB). El resultado es una imagen 4x más pequeña y que
> no expone el código fuente ni el compilador. Para desarrollo local tengo docker-compose que
> levanta la API, SQL Server y MongoDB con un solo comando."

### "¿Qué es CI/CD?"

> "CI — Continuous Integration — significa que cada push a GitHub ejecuta automáticamente
> el build y todas las pruebas. Si algo falla, lo sabes en minutos. CD — Continuous Deployment —
> extiende CI para publicar automáticamente si todo pasa. Mi pipeline tiene dos jobs en paralelo:
> backend compila y ejecuta 23 pruebas con dotnet test; frontend hace npm run build. Ambos
> deben pasar antes de que se generen los artefactos de deploy. En DaCodes esto se alinea con
> el Launch Pod donde QA está embebido en el equipo desde el día uno."

### "¿Conoces el modelo AWS Migration Pod de DaCodes?"

> "Sí. El Cloud & DevOps Studio de DaCodes ofrece el servicio AWS Migration Pod para
> ayudar a empresas a migrar sus aplicaciones a AWS. El flujo es: containerizar la app
> con Docker, configurar la infraestructura en AWS, y automatizar el deploy con CI/CD.
> Mi proyecto sigue exactamente ese flujo: la API está containerizada con un Dockerfile
> multi-stage, desplegada en Elastic Beanstalk que gestiona la infraestructura EC2 automáticamente,
> y el pipeline de GitHub Actions publica artefactos listos para deploy en cada push."
