# Clase de Programación — Día 5: Docker, CI/CD y AWS desde cero

> **A quién va dirigido:** desarrollador que conoce el backend y el frontend pero nunca ha
> tocado DevOps ni cloud. Cada concepto del Día 5 explicado en detalle: qué hace,
> por qué existe, cómo se usa en EcommerceNet y qué preguntar en una entrevista.

---

## Índice

1. [Antes de leer — ¿qué problema resuelve DevOps?](#1-antes-de-leer--qué-problema-resuelve-devops)
2. [Contenedores vs Máquinas Virtuales](#2-contenedores-vs-máquinas-virtuales)
3. [Docker — conceptos fundamentales](#3-docker--conceptos-fundamentales)
4. [Dockerfile — instrucción por instrucción](#4-dockerfile--instrucción-por-instrucción)
5. [Multi-stage build — por qué importa](#5-multi-stage-build--por-qué-importa)
6. [docker-compose — orquestar múltiples contenedores](#6-docker-compose--orquestar-múltiples-contenedores)
7. [CI/CD — Integración y Despliegue Continuo](#7-cicd--integración-y-despliegue-continuo)
8. [GitHub Actions — workflow línea por línea](#8-github-actions--workflow-línea-por-línea)
9. [AWS — la nube de Amazon](#9-aws--la-nube-de-amazon)
10. [IAM — gestión de identidad y acceso](#10-iam--gestión-de-identidad-y-acceso)
11. [EC2 — servidores virtuales en la nube](#11-ec2--servidores-virtuales-en-la-nube)
12. [Elastic Beanstalk — PaaS de AWS](#12-elastic-beanstalk--paas-de-aws)
13. [S3 — almacenamiento de objetos](#13-s3--almacenamiento-de-objetos)
14. [RDS — bases de datos relacionales en AWS](#14-rds--bases-de-datos-relacionales-en-aws)
15. [CloudFront — CDN de AWS](#15-cloudfront--cdn-de-aws)
16. [AWS CLI — controlar AWS desde la terminal](#16-aws-cli--controlar-aws-desde-la-terminal)
17. [DaCodes Studios y el modelo de Pods](#17-dacodes-studios-y-el-modelo-de-pods)
18. [Análisis completo del Dockerfile de EcommerceNet](#18-análisis-completo-del-dockerfile-de-ecommercenet)
19. [Análisis completo del ci-cd.yml de EcommerceNet](#19-análisis-completo-del-ci-cdyml-de-ecommercenet)
20. [Análisis completo del docker-compose.yml de EcommerceNet](#20-análisis-completo-del-docker-composeyml-de-ecommercenet)
21. [Glosario DevOps y Cloud](#21-glosario-devops-y-cloud)
22. [Preguntas de entrevista con respuestas](#22-preguntas-de-entrevista-con-respuestas)

---

## 1. Antes de leer — ¿qué problema resuelve DevOps?

### El problema clásico: "en mi máquina sí funciona"

Imagina este escenario:
1. Escribes código en Windows con .NET 8.0.3 y SQL Server 2022
2. Tu compañero tiene macOS con .NET 8.0.1 y SQL Server 2019
3. El servidor de producción tiene Linux con .NET 7 y PostgreSQL

El mismo código puede fallar en cada ambiente por diferencias en:
- Versiones del runtime
- Variables de entorno
- Puertos disponibles
- Dependencias del sistema operativo
- Cadenas de conexión a la base de datos

**DevOps resuelve esto:** Empaqueta la aplicación con TODO lo que necesita para correr, de forma que el mismo paquete funcione en cualquier máquina.

### El ciclo antes de DevOps

```
Desarrollador → escribe código → lo comprime en ZIP → lo manda por email → alguien lo sube al servidor manualmente
```

Problemas: ¿Qué versión está en producción? ¿Quién subió qué? ¿Cómo se vuelve atrás?

### El ciclo con DevOps moderno (lo que tiene EcommerceNet)

```
git push → GitHub detecta el push → ejecuta pruebas automáticamente →
si pasan, publica los artefactos → se suben a AWS → la app está actualizada
```

Todo es **automático**, **trazable** y **reproducible**.

---

## 2. Contenedores vs Máquinas Virtuales

### Máquina Virtual (VM)

Una VM emula una computadora completa: tiene su propio hardware virtual, su propio sistema operativo, sus propios drivers. Es como tener otra computadora dentro de tu computadora.

```
┌─────────────────────────────────────┐
│         Tu computadora              │
│  ┌───────────────────────────────┐  │
│  │      Hypervisor (VMware/VBox) │  │
│  │  ┌─────────┐  ┌─────────┐    │  │
│  │  │  VM 1   │  │  VM 2   │    │  │
│  │  │  Linux  │  │ Windows │    │  │
│  │  │  App A  │  │  App B  │    │  │
│  │  └─────────┘  └─────────┘    │  │
│  └───────────────────────────────┘  │
└─────────────────────────────────────┘
```

**Peso:** Una VM de Ubuntu ocupa ~8 GB de disco. Tarda 2-3 minutos en arrancar.

### Contenedor Docker

Un contenedor comparte el kernel del sistema operativo del host pero tiene su propio sistema de archivos, procesos y red.

```
┌─────────────────────────────────────┐
│         Tu computadora              │
│  ┌───────────────────────────────┐  │
│  │      Docker Engine            │  │
│  │  ┌──────────┐ ┌──────────┐   │  │
│  │  │Contenedor│ │Contenedor│   │  │
│  │  │  API     │ │SQL Server│   │  │
│  │  └──────────┘ └──────────┘   │  │
│  └───────────────────────────────┘  │
│  Sistema Operativo del Host         │
└─────────────────────────────────────┘
```

**Peso:** El contenedor de la API de EcommerceNet ocupa ~200 MB. Tarda 1-2 segundos en arrancar.

### Comparación directa

| Característica | VM | Contenedor |
|----------------|----|-----------:|
| Tamaño | 2-20 GB | 50-500 MB |
| Tiempo de arranque | 1-3 minutos | 1-5 segundos |
| Aislamiento | Completo (hardware virtual) | Proceso (kernel compartido) |
| Portabilidad | Alta | Muy alta |
| Rendimiento | Overhead del hypervisor | Cercano al nativo |

**En producción moderna:** AWS EC2 es una VM, y dentro de esa VM corren contenedores Docker. Se usan juntos.

---

## 3. Docker — conceptos fundamentales

### Los cuatro conceptos clave

**1. Imagen (Image)**
Una imagen es una "fotografía" de un sistema de archivos. Es inmutable. Es el molde.

```
mcr.microsoft.com/dotnet/aspnet:8.0   ← imagen de ASP.NET Core 8 runtime
mcr.microsoft.com/dotnet/sdk:8.0      ← imagen del SDK de .NET 8
mongo:7                                ← imagen de MongoDB 7
```

**2. Contenedor (Container)**
Un contenedor es una imagen en ejecución. La imagen es el molde; el contenedor es el objeto creado. Puedes tener múltiples contenedores de la misma imagen.

**3. Dockerfile**
Un script que describe cómo construir una imagen. Cada instrucción crea una "capa" (layer).

**4. Registry**
Un repositorio de imágenes. Docker Hub es el público. AWS tiene ECR. Microsoft usa MCR (`mcr.microsoft.com`).

### El sistema de capas

Docker almacena las imágenes en capas. Cada instrucción `RUN`, `COPY` crea una nueva capa.

```
Layer 5: COPY --from=build /publish .     (tus binarios)
Layer 4: WORKDIR /app
Layer 3: [imagen base] aspnet:8.0
Layer 2: [imagen base] dotnet runtime
Layer 1: [imagen base] debian:12-slim
```

Si cambias tu código y reconstruyes la imagen, solo se recrea la capa 5. Las 1-4 se reutilizan del caché. Esto hace los builds mucho más rápidos.

---

## 4. Dockerfile — instrucción por instrucción

### `FROM`

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
```
- Imagen base. `dotnet/sdk:8.0` incluye el compilador de C#, herramientas de NuGet
- `AS build` — nombre para la etapa (usado en multi-stage builds)

### `WORKDIR`

```dockerfile
WORKDIR /app
```
- Crea y establece el directorio de trabajo. Sin esto, los archivos van a la raíz `/`.

### `COPY`

```dockerfile
COPY origen destino
COPY src/EcommerceNet.API/EcommerceNet.API.csproj src/EcommerceNet.API/
```
- El origen es relativo al "build context" (carpeta pasada a `docker build`)
- El destino es relativo al `WORKDIR`

### `RUN`

```dockerfile
RUN dotnet restore
RUN dotnet publish src/EcommerceNet.API -c Release -o /publish
```
- Ejecuta un comando durante el build. El resultado se "hornea" en la imagen.
- Cada `RUN` crea una nueva capa.

### `EXPOSE`

```dockerfile
EXPOSE 80
```
- Documenta qué puerto escucha el contenedor. Solo es metadata — no abre el puerto.
- El puerto real se abre al correr el contenedor con `-p 5000:80`.

### `ENV`

```dockerfile
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
```
- Variables de entorno disponibles cuando el contenedor corre.
- `ASPNETCORE_ENVIRONMENT` controla qué `appsettings.{Entorno}.json` se carga.

### `ENTRYPOINT`

```dockerfile
ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]
```
- El comando que se ejecuta cuando el contenedor inicia.
- Forma de array JSON (`["cmd", "arg"]`) — el proceso es PID 1, recibe señales del OS directamente (graceful shutdown).
- Forma string (`"dotnet ..."`) — usa shell como intermediario, las señales no llegan correctamente.

---

## 5. Multi-stage build — por qué importa

### El problema con un build de una sola etapa

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0  # 800 MB con compilador y código fuente
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o /publish
ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]
```

La imagen final contiene el SDK completo, el compilador, el código fuente — **800 MB**.

### La solución: multi-stage build

```dockerfile
# Etapa 1: imagen PESADA para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish src/EcommerceNet.API -c Release -o /publish

# Etapa 2: imagen LIGERA para producción
FROM mcr.microsoft.com/dotnet/aspnet:8.0   # ← nueva imagen base, sin SDK (~200 MB)
WORKDIR /app
COPY --from=build /publish .               # ← copia SOLO los binarios de la etapa 1
ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]
```

### Beneficios

| Beneficio | Detalle |
|-----------|---------|
| **Tamaño** | 200 MB en lugar de 800 MB |
| **Seguridad** | El código fuente no está en la imagen de producción |
| **Velocidad** | Imágenes más pequeñas se descargan más rápido en EC2 |
| **Superficie de ataque** | Sin compilador, sin herramientas de debug |

---

## 6. docker-compose — orquestar múltiples contenedores

### El problema que resuelve

Para correr EcommerceNet localmente necesitas tres procesos: API + SQL Server + MongoDB. Sin docker-compose, cada uno requiere un `docker run` con decenas de flags. docker-compose define todo en YAML y lo levanta con un solo comando.

### Conceptos clave

**`services`** — cada servicio es un contenedor. Se comunican usando el nombre del servicio como hostname.

```yaml
services:
  api:        # la API puede conectarse a SQL Server con hostname "sqlserver"
  sqlserver:
  mongo:
```

**`build` vs `image`**
```yaml
api:
  build:
    context: .
    dockerfile: src/EcommerceNet.API/Dockerfile   # construye desde Dockerfile
sqlserver:
  image: mcr.microsoft.com/mssql/server:2022-latest  # descarga imagen pública
```

**`ports`** — mapea HOST:CONTENEDOR
```yaml
ports:
  - "5000:80"   # localhost:5000 → contenedor:80
```

**`environment`** — variables inyectadas al contenedor
```yaml
environment:
  - ConnectionStrings__DefaultConnection=Server=sqlserver;...
```
El `__` (doble guión) mapea a la jerarquía JSON: `appsettings["ConnectionStrings"]["DefaultConnection"]`.
`Server=sqlserver` — Docker crea una red privada donde los servicios se descubren por nombre.

**`depends_on`** — orden de inicio
```yaml
api:
  depends_on:
    - sqlserver   # sqlserver inicia antes que api
```
**Limitación:** solo espera a que el contenedor arranque, no a que SQL Server esté listo. SQL Server tarda ~30 segundos. En producción se usa un healthcheck o reintentos en el código.

**`volumes`** — persistencia de datos
```yaml
sqlserver:
  volumes:
    - sqlserver-data:/var/opt/mssql

volumes:
  sqlserver-data:   # volumen nombrado gestionado por Docker
```
Sin volumen, los datos de SQL Server se pierden al hacer `docker-compose down`.

### Comandos esenciales

```bash
docker-compose up --build   # construye imágenes y levanta todos los servicios
docker-compose up -d        # levanta en background (detached)
docker-compose down         # detiene y elimina contenedores (datos persisten)
docker-compose down -v      # detiene, elimina contenedores Y volúmenes (borra datos)
docker-compose logs api     # ver logs de un servicio específico
docker-compose exec api bash  # abrir shell dentro de un contenedor
```

---

## 7. CI/CD — Integración y Despliegue Continuo

### ¿Qué significa CI/CD?

**CI — Continuous Integration (Integración Continua)**
Cada vez que un desarrollador hace `git push`, el sistema automáticamente:
1. Descarga el código
2. Instala dependencias
3. Compila el proyecto
4. Ejecuta todas las pruebas

Si algo falla, el sistema notifica inmediatamente. El código "roto" no llega a producción.

**CD — Continuous Deployment (Despliegue Continuo)**
Si todas las pruebas pasan, el sistema automáticamente actualiza producción sin intervención humana.

### Por qué importa para DaCodes

DaCodes usa el modelo "Launch Pod" donde QA y DevOps están integrados en el equipo desde el primer día. El CI/CD permite que QA valide automáticamente cada cambio sin revisar código manualmente.

### El flujo en EcommerceNet

```
git push origin main
        ↓
GitHub detecta el push
        ↓
Lanza dos jobs en paralelo:
┌──────────────────────┐  ┌──────────────────────┐
│   Job: backend       │  │   Job: frontend       │
│   1. checkout        │  │   1. checkout         │
│   2. setup .NET 8    │  │   2. setup Node 20    │
│   3. dotnet restore  │  │   3. npm ci           │
│   4. dotnet build    │  │   4. npm run build    │
│   5. dotnet test ✅  │  │   5. upload dist/ ✅  │
│   6. dotnet publish  │  └──────────────────────┘
│   7. upload publish/ │
└──────────────────────┘
        ↓
Artefactos disponibles para deploy manual a AWS
```

---

## 8. GitHub Actions — workflow línea por línea

### Estructura de un workflow

```yaml
name: Nombre del workflow          # visible en la UI de GitHub

on:                                # cuándo se ejecuta
  push:
    branches: [ main ]

jobs:                              # trabajos (corren en paralelo por defecto)
  nombre-del-job:
    runs-on: ubuntu-latest         # tipo de máquina virtual
    steps:                         # pasos secuenciales dentro del job
    - name: Descripción del paso
      uses: action/nombre@version  # usar una Action del marketplace
      run: comando bash            # O ejecutar un comando directo
```

### Conceptos clave

**`on:`** — triggers
```yaml
on:
  push:
    branches: [ main, desarrollo ]  # push a estas ramas
  pull_request:
    branches: [ main ]              # PR cuyo destino sea main
  workflow_dispatch:                # disparar manualmente desde la UI
```

**`jobs:`** — trabajan en paralelo, en VMs separadas
Cada job obtiene una **VM nueva y limpia**. No comparte nada con otros jobs. Para compartir datos se usan artefactos.

**`uses:` vs `run:`**
```yaml
- uses: actions/checkout@v4        # Action publicada en el marketplace
- run: dotnet test                 # comando bash en la VM
```

**`if:` — condicionales**
```yaml
- name: Publicar API
  if: github.ref == 'refs/heads/main'   # solo en la rama main
  run: dotnet publish ...
```

**`working-directory:`**
```yaml
- name: Instalar dependencias
  working-directory: src/EcommerceNet.Web    # cambia de directorio
  run: npm ci
```

---

## 9. AWS — la nube de Amazon

### ¿Qué es "la nube"?

Servidores de Amazon que tú alquilas. En lugar de comprar y mantener tu propio servidor físico, pagas por los recursos que usas (CPU, memoria, disco, transferencia de datos).

### Por qué AWS

AWS tiene el 33% del mercado de cloud. **DaCodes es AWS Partner** — sus clientes piden AWS específicamente.

### Regiones y Zonas de Disponibilidad

AWS divide el mundo en **regiones** (ej: `us-east-1` = Norte de Virginia). Cada región tiene múltiples data centers físicamente separados (Zonas de Disponibilidad).

**Para la entrevista:** "Elegí `us-east-1` porque es la región más antigua, tiene más servicios disponibles y menor latencia para EE.UU. — mercado principal de DaCodes."

### Free Tier — qué es gratis (12 meses)

| Servicio | Gratis por mes |
|---------|----------------|
| EC2 t2.micro | 750 horas (una instancia corriendo 24/7) |
| S3 | 5 GB de almacenamiento + 20,000 GET |
| RDS t3.micro | 750 horas |
| CloudFront | 1 TB de transferencia de datos |

---

## 10. IAM — gestión de identidad y acceso

### ¿Qué es IAM?

IAM (Identity and Access Management) define **quién puede hacer qué** en tu cuenta de AWS.

### Regla de oro: nunca usar el usuario root

La cuenta de AWS tiene un usuario **root** con acceso total e irrevocable. Si alguien obtiene las credenciales root, puede borrar todos tus recursos y gastar miles de dólares.

**La práctica correcta:** Crear un usuario IAM con solo los permisos necesarios para la tarea.

### Tipos de credenciales IAM

**1. Usuario + contraseña** — para acceder a la consola web de AWS

**2. Access Key ID + Secret Access Key** — para programas y la CLI
```
Access Key ID:     TU_ACCESS_KEY_ID_AQUI
Secret Access Key: TU_SECRET_ACCESS_KEY_AQUI
```
**Nunca los pongas en el código fuente — ni en GitHub, ni en comentarios.**

### Políticas IAM

Una política es un documento JSON que define permisos:
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": ["s3:GetObject", "s3:PutObject"],
      "Resource": "arn:aws:s3:::ecommercenet-ramiro671/*"
    }
  ]
}
```

Para EcommerceNet el usuario de deploy necesita:
- `ElasticBeanstalkFullAccess`
- `AmazonS3FullAccess`

---

## 11. EC2 — servidores virtuales en la nube

### ¿Qué es EC2?

EC2 (Elastic Compute Cloud) son máquinas virtuales en la nube. Eliges el tamaño (CPU + RAM), el sistema operativo, y pagas por hora.

### Tipos de instancias relevantes

| Tipo | vCPU | RAM | Precio/hr |
|------|------|-----|-----------|
| `t2.micro` | 1 | 1 GB | Gratis (Free Tier) |
| `t3.small` | 2 | 2 GB | ~$0.02 |
| `t3.medium` | 2 | 4 GB | ~$0.04 |

Para EcommerceNet: `t2.micro` — entra en el Free Tier y tiene suficiente RAM para la demo.

### Security Groups

Son firewalls virtuales para las instancias EC2:
- Puerto 80 (HTTP) — abierto al mundo
- Puerto 443 (HTTPS) — abierto al mundo
- Puerto 22 (SSH) — solo desde tu IP

---

## 12. Elastic Beanstalk — PaaS de AWS

### ¿Qué es PaaS?

**PaaS (Platform as a Service)** — subes tu código y AWS gestiona el servidor, el balanceador de carga, el auto-scaling, las actualizaciones del OS.

Comparación:
- **IaaS (EC2):** Tú gestionas el OS, el servidor web, el runtime
- **PaaS (Elastic Beanstalk):** Subes el código o Docker image, AWS gestiona todo

### Cómo funciona Elastic Beanstalk

```
eb init              → configura la aplicación (nombre, región, plataforma)
eb create            → crea el entorno (EC2 + Load Balancer + Auto Scaling)
eb deploy            → sube una nueva versión del código
eb setenv KEY=VALUE  → configura variables de entorno
eb open              → abre la URL del entorno en el navegador
eb status            → ver estado (Health: Green/Yellow/Red)
eb terminate         → elimina TODOS los recursos del entorno
```

### EB y Docker

Cuando la plataforma es "Docker", Elastic Beanstalk lee el `Dockerfile`, construye la imagen en EC2 y corre el contenedor.

### Por qué Elastic Beanstalk para EcommerceNet

| Razón | Detalle |
|-------|---------|
| **Free Tier** | La instancia EC2 subyacente es t2.micro |
| **Sin gestión** | No hay que instalar .NET, configurar nginx |
| **Docker nativo** | Soporta Dockerfile directamente |
| **Velocidad** | Un comando (`eb create`) vs horas de configuración manual |

---

## 13. S3 — almacenamiento de objetos

### ¿Qué es S3?

S3 (Simple Storage Service) almacena cualquier tipo de archivo (objeto) en la nube. Los objetos se guardan en **buckets** (el nombre debe ser único globalmente en AWS).

### Por qué S3 para el frontend

El frontend de Vue.js después de `npm run build` es solo HTML + CSS + JavaScript — archivos estáticos. No necesita un servidor de aplicaciones. S3 puede servir archivos estáticos directamente (static website hosting):

```
Usuario → http://ecommercenet-ramiro671.s3-website-us-east-1.amazonaws.com
                           ↓
                    S3 sirve index.html
                           ↓
                Vue.js arranca en el navegador del usuario
```

### Ventajas de S3 vs servidor web

| Aspecto | EC2 con Nginx | S3 |
|---------|-------------|-----|
| Costo | ~$0.01/hr | ~$0 en Free Tier |
| Mantenimiento | Actualizar OS, nginx | Cero |
| Disponibilidad | Depende de EC2 | 99.99% (SLA AWS) |
| Escalabilidad | Manual | Automática |

### Bucket policy — acceso público

Por defecto los objetos son privados. Para hosting estático necesitas:
```json
{
  "Version": "2012-10-17",
  "Statement": [{
    "Effect": "Allow",
    "Principal": "*",
    "Action": "s3:GetObject",
    "Resource": "arn:aws:s3:::ecommercenet-ramiro671/*"
  }]
}
```

### ARN — Amazon Resource Name

Identificador único de cualquier recurso en AWS:
```
arn:aws:s3:::ecommercenet-ramiro671/*
│    │   │                   │       └─ todos los objetos (wildcard)
│    │   │                   └─ nombre del bucket
│    │   └─ s3 (tipo de servicio)
│    └─ aws (partición)
└─ arn (prefijo siempre)
```

---

## 14. RDS — bases de datos relacionales en AWS

### ¿Qué es RDS?

RDS (Relational Database Service) es SQL Server, PostgreSQL, MySQL, etc. gestionados por AWS. AWS se encarga de backups automáticos, parches de seguridad, failover automático y monitoreo.

### Por qué InMemory DB para la demo

RDS SQL Server Express requiere configuración adicional. Para una demo de entrevista, `InMemory` (o SQLite) es más rápido y sin costos adicionales.

---

## 15. CloudFront — CDN de AWS

### ¿Qué es un CDN?

CDN (Content Delivery Network) es una red de servidores distribuidos globalmente. Los archivos se sirven desde el servidor más cercano a cada usuario.

```
Sin CDN:
Usuario en CDMX → servidor en us-east-1 (Virginia) → 80ms de latencia

Con CloudFront:
Usuario en CDMX → edge location en México/Dallas → 10ms de latencia
```

### CloudFront + S3

CloudFront delante de S3 para:
1. Servir archivos más rápido globalmente (CDN)
2. Añadir HTTPS (S3 sin CloudFront solo tiene HTTP)
3. Cachear archivos en los edge locations

Para la demo inicial, S3 solo es suficiente. CloudFront se agrega después.

---

## 16. AWS CLI — controlar AWS desde la terminal

### ¿Qué es la AWS CLI?

Programa de línea de comandos que controla todos los servicios de AWS sin necesidad de abrir el navegador.

```bash
# Consola web                          # Equivalente CLI
Ir a S3 → Crear bucket                 aws s3 mb s3://mi-bucket
Ir a EC2 → Lanzar instancia            aws ec2 run-instances ...
```

### Instalación en Windows

```powershell
# Windows (PowerShell como Administrador)
winget install Amazon.AWSCLI

# Verificar
aws --version
# aws-cli/2.x.x Python/3.x.x Windows/...
```

### Configuración

```bash
aws configure
# AWS Access Key ID [None]: TU_ACCESS_KEY_ID_AQUI
# AWS Secret Access Key [None]: TU_SECRET_ACCESS_KEY_AQUI
# Default region name [None]: us-east-1
# Default output format [None]: json
```

Las credenciales se guardan en `~/.aws/credentials`.

### Comandos más usados en el proyecto

```bash
# S3
aws s3 mb s3://nombre-bucket              # crear bucket
aws s3 sync dist/ s3://nombre-bucket      # sincronizar carpeta → bucket
aws s3 rb s3://nombre-bucket --force      # eliminar bucket y contenido

# Elastic Beanstalk (via EB CLI - instalar con: pip install awsebcli)
eb init                                   # inicializar configuración
eb create nombre-entorno                  # crear entorno
eb deploy                                 # desplegar nueva versión
eb status                                 # ver estado del entorno
eb open                                   # abrir URL en el navegador
eb setenv CLAVE=VALOR                     # configurar variable de entorno
eb terminate nombre-entorno               # eliminar entorno (¡libera recursos!)

# Verificar credenciales
aws sts get-caller-identity              # ver con qué usuario estás autenticado
```

---

## 17. DaCodes Studios y el modelo de Pods

### Los 4 Studios de DaCodes

| Studio | Qué hace | Pod relevante |
|--------|----------|--------------|
| **Software Engineering & QA** | Desarrollo fullstack + QA integrado | Launch Pod |
| **Cloud & DevOps** | AWS migrations, CI/CD, infraestructura | AWS Migration Pod |
| **AI & Data** | Machine Learning, GenAI | GenAI Accelerator |
| **Product Strategy & Design** | UX/UI, product management | Discovery Sprint |

### El modelo Pod

Un "Pod" es un equipo pequeño y autónomo (4-8 personas) con todo lo necesario para entregar un feature de principio a fin:

```
Launch Pod típico:
├── 2-3 Desarrolladores Fullstack
├── 1 QA Engineer (integrado, no separado)
├── 1 DevOps / Cloud Engineer
└── 1 Tech Lead / Scrum Master
```

### Lo que DaCodes busca en un Senior Fullstack

| Competencia | Evidencia en EcommerceNet |
|-------------|--------------------------|
| Clean Architecture | 4 capas con dependencias estrictas |
| API RESTful con JWT | 20+ endpoints, roles Admin/Cliente |
| Base de datos relacional | EF Core, migraciones, SQL avanzado |
| Frontend moderno | Vue.js 3 Composition API, Pinia |
| DevOps básico | Dockerfile, GitHub Actions, AWS |
| Calidad de código | 23 pruebas, sin code smells |

---

## 18. Análisis completo del Dockerfile de EcommerceNet

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
```
- `FROM` define la imagen base. El SDK completo de .NET 8 (~800 MB): compilador, NuGet, dotnet CLI.
- `AS build` nombra esta etapa para referenciarla con `COPY --from=build` en la etapa 2.

```dockerfile
WORKDIR /app
```
- Establece `/app` como directorio de trabajo. Todos los comandos posteriores son relativos a `/app`.

```dockerfile
COPY EcommerceNet.sln .
COPY src/EcommerceNet.Core/EcommerceNet.Core.csproj src/EcommerceNet.Core/
COPY src/EcommerceNet.Data/EcommerceNet.Data.csproj src/EcommerceNet.Data/
COPY src/EcommerceNet.API/EcommerceNet.API.csproj src/EcommerceNet.API/
COPY tests/EcommerceNet.Tests/EcommerceNet.Tests.csproj tests/EcommerceNet.Tests/
```
- **Truco de caché de Docker.** Copia solo los archivos `.csproj` antes del código fuente.
- Los `.csproj` cambian raramente (solo cuando se agrega un paquete NuGet).
- Si no cambió ningún `.csproj`, Docker reutiliza el layer de `restore` cacheado. Build: 60s → 3s.

```dockerfile
RUN dotnet restore
```
- Descarga todos los paquetes NuGet de los `.csproj`. Solo se re-ejecuta si cambió algún `.csproj`.

```dockerfile
COPY . .
```
- Ahora sí copia TODO el código fuente. Esta capa se invalida con cualquier cambio en `.cs`.
- Pero el restore ya está cacheado — no se repite.

```dockerfile
RUN dotnet publish src/EcommerceNet.API -c Release -o /publish
```
- `-c Release` — optimizaciones de JIT, sin símbolos de debug.
- `-o /publish` — output en `/publish`. Usamos `/publish` para simplificar el COPY de la etapa 2.

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
```
- **Nueva imagen base.** Solo el runtime de ASP.NET Core (~200 MB). Sin compilador.
- La etapa 1 se descarta. Sus 800 MB no van en la imagen final.

```dockerfile
WORKDIR /app
COPY --from=build /publish .
```
- `--from=build` — copia desde la etapa nombrada "build".
- Solo los binarios de `/publish`. Sin código fuente, sin SDK, sin archivos temporales.

```dockerfile
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
```
- `EXPOSE 80` — documenta que el contenedor escucha en 80. Solo metadata.
- `ASPNETCORE_URLS=http://+:80` — le dice a Kestrel que escuche en todas las interfaces en puerto 80.
- El `+` es el wildcard de IP (equivale a `0.0.0.0`).

```dockerfile
ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]
```
- Forma de array JSON — el proceso `dotnet` es PID 1, recibe SIGTERM directamente.
- Al hacer `docker stop`, el contenedor recibe SIGTERM → ASP.NET Core completa requests en vuelo antes de cerrar (graceful shutdown).

---

## 19. Análisis completo del ci-cd.yml de EcommerceNet

```yaml
name: CI/CD EcommerceNet
```
Visible en GitHub → pestaña Actions → lista de workflows.

```yaml
on:
  push:
    branches: [ main, desarrollo ]
  pull_request:
    branches: [ main ]
```
- Push a `main` → CI completo + generación de artefactos
- Push a `desarrollo` → solo CI (sin artefactos)
- PR hacia `main` → CI para verificar antes del merge. GitHub bloquea el merge si falla.

```yaml
jobs:
  backend:
    runs-on: ubuntu-latest
```
- VM nueva y limpia por cada ejecución (efímera). Cada job no comparte nada con otros jobs.

```yaml
    - uses: actions/checkout@v4
```
- Clona el repositorio en la VM. `@v4` fija la versión — evita roturas si la Action cambia.

```yaml
    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
```
- Instala .NET 8. Ubuntu latest no tiene .NET preinstalado.
- `8.0.x` — última patch version de .NET 8.

```yaml
    - run: dotnet restore
    - run: dotnet build --no-restore --configuration Release
```
- Separar restore y build permite ver en el log cuánto tarda cada paso.
- `--no-restore` — no re-descarga paquetes (ya se hizo).

```yaml
    - run: dotnet test --no-build --configuration Release --verbosity normal
```
- **El paso más crítico.** Si cualquier prueba falla → job falla → merge bloqueado.
- `--verbosity normal` — muestra cada prueba individualmente en el log.

```yaml
    - if: github.ref == 'refs/heads/main'
      run: dotnet publish src/EcommerceNet.API -c Release -o ./publish
```
- Solo en `main`. En `desarrollo` el pipeline termina después de las pruebas.

```yaml
    - uses: actions/upload-artifact@v4
      with:
        name: api-publish
        path: ./publish
```
- Guarda los binarios como artefacto descargable en la UI de GitHub Actions.
- Retención: 90 días. En un CD completo, el job de deploy descargaría este artefacto.

```yaml
  frontend:
    runs-on: ubuntu-latest
```
- Job **independiente y paralelo** al de backend. Reduce el tiempo total de CI de ~4 min a ~2.5 min.

```yaml
    - working-directory: src/EcommerceNet.Web
      run: npm ci
```
- `npm ci` instala exactamente las versiones del `package-lock.json`. Más reproducible que `npm install`.
- `working-directory:` cambia de directorio (el proyecto Vue no está en la raíz).

---

## 20. Análisis completo del docker-compose.yml de EcommerceNet

```yaml
version: '3.8'
```
Versión del formato. Compatible con Docker Engine 19.03+.

```yaml
services:
  api:
    build:
      context: .
      dockerfile: src/EcommerceNet.API/Dockerfile
```
- `context: .` — contexto de build es la raíz del repositorio.
- Si fuera `context: src/EcommerceNet.API/`, el `COPY EcommerceNet.sln .` del Dockerfile fallaría.

```yaml
    ports:
      - "5000:80"
```
- `HOST:CONTENEDOR`. Acceso desde el host: `http://localhost:5000`.
- Acceso desde otro contenedor en la misma red: `http://api` (nombre del servicio = hostname).

```yaml
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;...
```
- Variables sobreescriben `appsettings.json`. `__` es el separador jerárquico en .NET.
- `Server=sqlserver` — Docker crea una red privada donde servicios se descubren por nombre.

```yaml
    depends_on:
      - sqlserver
```
- Garantiza que el contenedor de SQL Server **arranque** antes que la API.
- No garantiza que SQL Server esté listo. SQL Server tarda ~30 segundos en inicializar.

```yaml
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=YourStrong!Passw0rd
```
- `ACCEPT_EULA=Y` — requerido. Sin esto, el contenedor no inicia.
- La contraseña debe cumplir: mínimo 8 chars, mayúsculas, minúsculas, número, símbolo.

```yaml
    volumes:
      - sqlserver-data:/var/opt/mssql
```
- Sin volumen, los datos se pierden al hacer `docker-compose down`.
- Volumen nombrado gestionado por Docker. Se elimina con `docker-compose down -v`.

```yaml
volumes:
  sqlserver-data:
  mongo-data:
```
Docker los crea automáticamente en el primer `up`. En Windows: `C:\ProgramData\docker\volumes\`.

---

## 21. Glosario DevOps y Cloud

| Término | Definición |
|---------|-----------|
| **Artefacto** | Archivo compilado y empaquetado listo para deploy |
| **Auto Scaling** | Añadir o quitar instancias automáticamente según la carga |
| **Build Context** | Carpeta cuyos archivos están disponibles durante el `docker build` |
| **CDN** | Red de servidores distribuidos geográficamente para servir contenido más rápido |
| **CI** | Integración Continua — ejecutar pruebas automáticamente en cada commit |
| **CD** | Despliegue Continuo — actualizar producción automáticamente al pasar CI |
| **Container** | Instancia en ejecución de una imagen Docker |
| **Docker Hub** | Registry público de imágenes Docker |
| **Dockerfile** | Script para construir una imagen Docker |
| **ECR** | Elastic Container Registry — registry privado de imágenes en AWS |
| **Edge Location** | Servidor de CloudFront distribuido geográficamente |
| **Fargate** | Variante de ECS sin gestión de servidores EC2 |
| **Free Tier** | Capa gratuita de AWS — 12 meses con límites por servicio |
| **IAM** | Identity and Access Management — control de acceso de AWS |
| **Image** | Sistema de archivos inmutable que sirve de base para contenedores |
| **IaaS** | Infrastructure as a Service — alquiler de servidores virtuales (EC2) |
| **Job** | Unidad de trabajo en GitHub Actions que corre en una VM |
| **Lambda** | Compute sin servidor de AWS — solo pagas por ejecución |
| **Layer** | Capa de una imagen Docker creada por una instrucción del Dockerfile |
| **Load Balancer** | Distribuye el tráfico entre múltiples instancias |
| **Multi-stage build** | Dockerfile con múltiples etapas `FROM` para reducir el tamaño final |
| **PaaS** | Platform as a Service — plataforma gestionada (Elastic Beanstalk) |
| **Pipeline** | Secuencia automatizada de pasos para CI/CD |
| **Pod** | Equipo pequeño autónomo en el modelo de DaCodes |
| **RDS** | Relational Database Service — bases de datos gestionadas en AWS |
| **Registry** | Repositorio de imágenes Docker |
| **Region** | Zona geográfica de AWS (ej: `us-east-1`) |
| **S3** | Simple Storage Service — almacenamiento de objetos en AWS |
| **Secret** | Variable sensible almacenada de forma segura |
| **Static Hosting** | Servir archivos HTML/CSS/JS sin servidor de aplicaciones |
| **Step** | Paso individual dentro de un job de GitHub Actions |
| **t2.micro** | Instancia EC2 del Free Tier: 1 vCPU, 1 GB RAM |
| **Trigger** | Evento que dispara un workflow (push, PR, schedule) |
| **Volume** | Almacenamiento persistente para contenedores Docker |
| **Workflow** | Archivo YAML que define un pipeline en GitHub Actions |

---

## 22. Preguntas de entrevista con respuestas

### Sobre Docker

**"¿Qué es Docker y por qué lo usas?"**
> Docker empaqueta la aplicación con todas sus dependencias en un contenedor que corre igual en cualquier máquina. Usé un Dockerfile multi-stage: primero compilo con el SDK de .NET (imagen de 800MB) y en la segunda etapa copio solo los binarios al runtime de ASP.NET (200MB). La imagen de producción es 4x más ligera y no contiene el código fuente. Con docker-compose levanto la API y SQL Server juntos con un solo comando para desarrollo local.

**"¿Diferencia entre imagen y contenedor?"**
> La imagen es el molde — inmutable, como un `.iso`. El contenedor es una instancia en ejecución de esa imagen. Puedo tener 10 contenedores de la misma imagen corriendo en paralelo.

**"¿Qué es un multi-stage build?"**
> Un Dockerfile con múltiples instrucciones `FROM`. En EcommerceNet, la etapa 1 compila con el SDK completo. La etapa 2 copia solo los binarios al runtime ligero. La imagen final no contiene el SDK — es 4x más pequeña y más segura porque el código fuente no va en producción.

### Sobre CI/CD

**"¿Cómo implementaste CI/CD?"**
> Con GitHub Actions. Tengo un workflow con dos jobs en paralelo: backend y frontend. El job de backend hace checkout, instala .NET 8, restaura paquetes, compila y ejecuta las 23 pruebas unitarias. Si alguna falla, el pipeline falla y GitHub bloquea el merge. En `main`, también publica los artefactos. Cualquier push a `main` o `desarrollo` dispara el pipeline automáticamente.

**"¿Qué pasa si una prueba falla en CI?"**
> El step de `dotnet test` retorna un código de salida distinto de cero. GitHub Actions marca el job en rojo. Si es un PR, bloquea el botón de merge. El equipo recibe una notificación. Nadie puede mergear código roto a `main`.

### Sobre AWS

**"¿Tienes experiencia con AWS?"**
> Sí. Desplegué EcommerceNet en AWS: la API en Elastic Beanstalk con Docker y el frontend como hosting estático en S3. Configuré IAM con un usuario de deploy con permisos mínimos. Sé que DaCodes es AWS Partner y usa el modelo AWS Migration Pod — mi experiencia sigue exactamente esa filosofía: containerizar, automatizar y migrar.

**"¿Por qué Elastic Beanstalk en lugar de EC2 directo?"**
> Elastic Beanstalk es PaaS — subo el Dockerfile y AWS gestiona el EC2, el load balancer, el auto-scaling y los deploys. Con EC2 directo tendría que instalar .NET manualmente, configurar nginx, gestionar SSL. Para una demo o MVP, EB es mucho más rápido y entra en el Free Tier.

**"¿Por qué S3 para el frontend?"**
> Después de `npm run build`, el frontend es solo HTML + CSS + JavaScript. No necesita un servidor de aplicaciones. S3 sirve archivos estáticos directamente — es más barato, más escalable (infinita) y tiene 99.99% de disponibilidad sin que yo administre ningún servidor.

**"¿Qué es IAM?"**
> El sistema de control de acceso de AWS. Permite crear usuarios con permisos específicos. La regla de oro es nunca usar el usuario root. Para el deploy creé un usuario IAM con solo los permisos necesarios (EB + S3). Las credenciales de este usuario se usan en la CLI — nunca en el código.
