# Clase de Programación — Día 5: Docker + CI/CD + AWS + Entrevista DaCodes

> **A quién va dirigido:** desarrollador que completó los Días 1-4 y ahora empaqueta y despliega la aplicación.
> Cada concepto del Día 5 está explicado desde cero: qué es, por qué existe, cómo funciona por dentro.
> Se asume conocimiento del backend .NET y frontend Vue.js ya construidos.

---

## Índice

1. [¿Qué problema resuelve Docker?](#1-qué-problema-resuelve-docker)
2. [Contenedores vs máquinas virtuales](#2-contenedores-vs-máquinas-virtuales)
3. [Imágenes y capas: cómo funciona Docker por dentro](#3-imágenes-y-capas-cómo-funciona-docker-por-dentro)
4. [Dockerfile: instrucciones fundamentales](#4-dockerfile-instrucciones-fundamentales)
5. [Multi-stage build: el patrón que usan las empresas](#5-multi-stage-build-el-patrón-que-usan-las-empresas)
6. [docker-compose: orquestar múltiples servicios](#6-docker-compose-orquestar-múltiples-servicios)
7. [Volúmenes y redes en Docker](#7-volúmenes-y-redes-en-docker)
8. [CI/CD: qué es y por qué es obligatorio en empresas serias](#8-cicd-qué-es-y-por-qué-es-obligatorio-en-empresas-serias)
9. [GitHub Actions: pipeline como código](#9-github-actions-pipeline-como-código)
10. [AWS: los servicios clave para esta app](#10-aws-los-servicios-clave-para-esta-app)
11. [Elastic Beanstalk vs EC2 vs ECS](#11-elastic-beanstalk-vs-ec2-vs-ecs)
12. [Análisis línea por línea: Dockerfile](#12-análisis-línea-por-línea-dockerfile)
13. [Análisis línea por línea: docker-compose.yml](#13-análisis-línea-por-línea-docker-composeyml)
14. [Análisis línea por línea: ci-cd.yml](#14-análisis-línea-por-línea-ci-cdyml)
15. [DaCodes: Studios, Pods y cultura técnica](#15-dacodes-studios-pods-y-cultura-técnica)
16. [Preguntas de entrevista con respuestas memorizables](#16-preguntas-de-entrevista-con-respuestas-memorizables)
17. [Glosario de palabras clave del Día 5](#17-glosario-de-palabras-clave-del-día-5)

---

## 1. ¿Qué problema resuelve Docker?

### El problema clásico: "en mi máquina sí funciona"

Imagina este escenario:
- Tu máquina: Windows 11, .NET 8.0.402, SQL Server LocalDB, puerto 5152
- El servidor de tu compañero: Ubuntu 22.04, .NET 8.0.301, SQL Server 2019, puerto 1433
- El servidor en AWS: Amazon Linux 2, .NET 8.0.400, sin SQL Server instalado

El mismo código, tres entornos distintos, tres comportamientos distintos. Este es el problema que Docker resuelve.

### La solución: empaquetar todo junto

Docker crea un **contenedor**: una caja cerrada que incluye:
- La aplicación compilada
- Todas las dependencias (.NET runtime, librerías)
- La configuración del entorno
- El sistema operativo mínimo necesario

```
Sin Docker:
  Código → necesita .NET instalado → necesita librerías específicas → necesita config → puede fallar

Con Docker:
  Código + .NET + librerías + config = Imagen Docker → corre igual en cualquier máquina
```

### El contrato de Docker

> "Si corre en tu contenedor, corre en cualquier máquina que tenga Docker instalado."

Esto es lo que hace que Docker sea el estándar de la industria. DaCodes usa Docker en su "AWS Migration Pod" exactamente por esta razón: containerizar antes de migrar garantiza que la app se comporta igual en desarrollo, staging y producción.

---

## 2. Contenedores vs máquinas virtuales

Esta pregunta aparece frecuentemente en entrevistas técnicas. La diferencia es fundamental:

### Máquina Virtual (VM)

```
┌─────────────────────────────────┐
│         Tu aplicación           │
│    Sistema operativo completo   │  ← Windows/Linux/macOS completo (5-20 GB)
│         Hypervisor              │  ← VirtualBox, VMware, Hyper-V
│    Hardware físico del host     │
└─────────────────────────────────┘
```

- Cada VM tiene su propio OS completo
- Arranque: 30-90 segundos
- Tamaño: 5-20 GB por VM
- Aislamiento total: si la VM cae, no afecta al host

### Contenedor Docker

```
┌─────────────────────────────────┐
│    Contenedor A  │ Contenedor B │
│    Tu app .NET   │ Tu app Node  │
│    Runtime .NET  │ Runtime Node │
│         Docker Engine           │  ← Comparte el kernel del host
│    Sistema operativo del host   │  ← Un solo OS
│    Hardware físico del host     │
└─────────────────────────────────┘
```

- Los contenedores comparten el kernel del OS del host
- Arranque: 1-3 segundos (milliseconding en muchos casos)
- Tamaño: 50 MB - 1 GB (solo lo que la app necesita)
- Aislamiento a nivel de proceso: si el contenedor falla, el host sigue corriendo

### Cuándo usar cada uno

| Cuándo usar VM | Cuándo usar Contenedor |
|----------------|----------------------|
| Necesitas OS diferente al host | Quieres empaquetar y distribuir una app |
| Aislamiento de seguridad máxima | Múltiples versiones de la misma app |
| Legacy apps que no pueden containerizarse | Microservicios |
| Testing de diferentes OS | CI/CD pipelines |

---

## 3. Imágenes y capas: cómo funciona Docker por dentro

### ¿Qué es una imagen Docker?

Una imagen es un **snapshot inmutable** del sistema de archivos. Es la "plantilla" a partir de la cual se crean contenedores.

```
Imagen Docker = Sistema de archivos en capas (como commits de Git)

Capa 0:  Ubuntu 22.04 mínimo (imagen base)
Capa 1:  + .NET 8 Runtime instalado
Capa 2:  + archivos de tu aplicación
Capa 3:  + variables de entorno
         = Imagen final de tu app
```

### Por qué las capas importan: caché de Docker

Docker cachea cada capa. Si una capa no cambió, Docker la reutiliza en el siguiente build:

```dockerfile
COPY *.csproj ./          ← Capa A: solo los .csproj
RUN dotnet restore        ← Capa B: paquetes NuGet (se cachea si Capa A no cambió)
COPY . .                  ← Capa C: todo el código
RUN dotnet publish        ← Capa D: compilación
```

Si solo cambias código (Capa C), Docker reutiliza Capas A y B. El restore de NuGet no se repite. Esto ahorra 2-5 minutos por build.

### Imagen vs Contenedor

| Imagen | Contenedor |
|--------|-----------|
| Inmutable (no cambia) | Mutable (tiene estado en tiempo de ejecución) |
| Se guarda en disco | Corre en memoria |
| Se crea con `docker build` | Se crea con `docker run` |
| Se sube a Docker Hub | Existe en el host mientras corre |
| Como una clase en C# | Como una instancia de esa clase |

---

## 4. Dockerfile: instrucciones fundamentales

Un Dockerfile es un archivo de texto con instrucciones secuenciales para construir una imagen.

### Las instrucciones que usamos

| Instrucción | Qué hace | Ejemplo |
|-------------|---------|---------|
| `FROM` | Define la imagen base | `FROM mcr.microsoft.com/dotnet/sdk:8.0` |
| `WORKDIR` | Establece el directorio de trabajo | `WORKDIR /app` |
| `COPY` | Copia archivos del host al contenedor | `COPY . .` |
| `RUN` | Ejecuta un comando durante el build | `RUN dotnet restore` |
| `ENV` | Define variable de entorno | `ENV ASPNETCORE_URLS=http://+:80` |
| `EXPOSE` | Documenta el puerto que usa la app | `EXPOSE 80` |
| `ENTRYPOINT` | Comando al arrancar el contenedor | `ENTRYPOINT ["dotnet", "app.dll"]` |

### `ENTRYPOINT` vs `CMD`

```dockerfile
# ENTRYPOINT: no se puede sobreescribir fácilmente al ejecutar
ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]

# CMD: se puede sobreescribir con argumentos al correr el contenedor
CMD ["dotnet", "EcommerceNet.API.dll"]

# docker run mi-imagen        → ejecuta dotnet EcommerceNet.API.dll
# docker run mi-imagen bash   → con CMD ejecuta bash, con ENTRYPOINT ejecuta "dotnet EcommerceNet.API.dll bash"
```

Usamos `ENTRYPOINT` porque queremos que la app siempre arranque, sin posibilidad de accidente.

### Formato exec vs formato shell

```dockerfile
# Formato exec (array JSON) — CORRECTO
ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]
# El proceso es el PID 1 directamente. Las señales del OS llegan a dotnet.

# Formato shell — EVITAR en ENTRYPOINT
ENTRYPOINT dotnet EcommerceNet.API.dll
# Docker envuelve el comando en /bin/sh -c "...". dotnet es hijo de sh.
# Las señales Ctrl+C no llegan bien al proceso.
```

---

## 5. Multi-stage build: el patrón que usan las empresas

### El problema con un Dockerfile de un solo stage

```dockerfile
# MAL: imagen gigante que va a producción
FROM mcr.microsoft.com/dotnet/sdk:8.0    # 800 MB de SDK
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o /publish
ENTRYPOINT ["dotnet", "/publish/EcommerceNet.API.dll"]
```

Esta imagen pesa ~820 MB y contiene el compilador de C#, herramientas del SDK, y código fuente. **No hay ninguna razón para que el compilador vaya a producción.**

### La solución: multi-stage build

```dockerfile
# Stage 1: compilar con el SDK completo (imagen temporal, no va a producción)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish src/EcommerceNet.API -c Release -o /publish

# Stage 2: ejecutar solo con el runtime (imagen que va a producción)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /publish .         # ← copia solo los binarios del Stage 1
ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]
```

### Lo que queda en la imagen final

```
Stage 1 (descartado después del build):
  - Ubuntu + herramientas del SDK + compilador C# + código fuente + .csproj
  ≈ 800 MB

Stage 2 (imagen que va a AWS):
  - Ubuntu mínimo + ASP.NET Core Runtime + binarios compilados
  ≈ 220 MB
```

### Beneficios del multi-stage build

1. **Imagen más pequeña:** 220 MB vs 820 MB (73% menos)
2. **Seguridad:** el compilador no está en producción — un atacante no puede compilar código
3. **Menor superficie de ataque:** menos herramientas = menos vulnerabilidades
4. **Deploy más rápido:** imagen más pequeña se descarga antes en AWS

---

## 6. docker-compose: orquestar múltiples servicios

### El problema que resuelve docker-compose

La API de EcommerceNet necesita SQL Server para funcionar. Sin docker-compose tendrías que:

```bash
# Sin docker-compose (tedioso y propenso a errores):
docker network create ecommerce-network
docker run -d --name sqlserver --network ecommerce-network \
  -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD=YourStrong!Passw0rd \
  mcr.microsoft.com/mssql/server:2022-latest
docker run -d --name api --network ecommerce-network \
  -e ConnectionStrings__DefaultConnection="Server=sqlserver;..." \
  -p 5000:80 ecommercenet-api
```

Con docker-compose, todo eso es un solo archivo YAML y un solo comando:

```bash
docker-compose up --build
```

### La estructura de docker-compose

```yaml
version: '3.8'      # versión del formato del archivo

services:           # cada servicio es un contenedor
  api:              # nombre del servicio (también es su hostname en la red interna)
    build: ...      # o image: para usar una imagen pública
    ports: ...      # mapeo HOST:CONTENEDOR
    environment: ...# variables de entorno
    depends_on: ... # orden de arranque

volumes:            # almacenamiento persistente
```

### Redes en docker-compose

docker-compose crea automáticamente una **red privada** para todos los servicios del archivo. En esa red, cada servicio es accesible por su nombre:

```yaml
services:
  api:
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;...
      #                                              ↑
      #                  'sqlserver' se resuelve como hostname del contenedor
  sqlserver:
    ...
```

Desde dentro del contenedor `api`, `sqlserver` resuelve a la IP privada del contenedor de SQL Server. Desde tu máquina host, `localhost:1433` resuelve al mismo SQL Server gracias al mapeo de puertos.

---

## 7. Volúmenes y redes en Docker

### El problema de los datos efímeros

Un contenedor Docker es **efímero** — si lo eliminas y vuelves a crear, todos los datos se pierden. Esto es un problema para bases de datos:

```bash
docker-compose down    # elimina los contenedores
docker-compose up      # SQL Server arranca con BD vacía ← ¡perdiste todos los datos!
```

### Solución: volúmenes nombrados

```yaml
services:
  sqlserver:
    volumes:
      - sqlserver-data:/var/opt/mssql   # /var/opt/mssql persiste en el volumen

volumes:
  sqlserver-data:   # Docker gestiona este volumen en /var/lib/docker/volumes/
```

Ahora `docker-compose down` elimina el contenedor pero **no el volumen**. Los datos sobreviven.

```bash
docker-compose down         # contenedor muere, datos sobreviven
docker-compose up           # SQL Server arranca con los mismos datos ✅

docker-compose down -v      # elimina contenedor Y volúmenes (útil para empezar limpio)
```

### Tipos de volúmenes

| Tipo | Sintaxis | Dónde vive | Cuándo usar |
|------|---------|-----------|------------|
| **Nombrado** | `volumen-name:/ruta` | Docker gestiona | Bases de datos, datos persistentes |
| **Bind mount** | `./local:/ruta` | Carpeta del host | Desarrollo: el contenedor lee tu código local |
| **tmpfs** | `type: tmpfs` | Solo memoria RAM | Datos temporales sensibles |

---

## 8. CI/CD: qué es y por qué es obligatorio en empresas serias

### Definiciones

**CI — Integración Continua (Continuous Integration):**
Cada vez que un desarrollador hace push de código, un proceso automático:
1. Descarga el código
2. Instala dependencias
3. Compila la aplicación
4. Ejecuta TODOS los tests
5. Si algo falla → notifica inmediatamente

**CD — Despliegue Continuo (Continuous Deployment):**
Si CI pasa (todo compila y todos los tests pasan), automáticamente:
1. Empaqueta la aplicación
2. La despliega al entorno correspondiente (staging, producción)

### Por qué importa

Sin CI/CD:
```
Desarrollador A hace cambio → funciona en su máquina
Desarrollador B hace cambio → funciona en su máquina
Los dos hacen merge → se rompe TODO → nadie sabe por qué → 4 horas depurando
```

Con CI/CD:
```
Desarrollador A hace PR → CI corre tests → tests pasan → se puede mergear ✅
Desarrollador B hace PR → CI corre tests → test X falla → PR bloqueado ← sabe exactamente qué rompió
```

### El modelo DaCodes: Launch Pod

DaCodes usa un modelo llamado **Launch Pod** donde QA está embebido en el equipo desde el día uno. CI/CD es el corazón de este modelo:

- **QA automatizado:** los tests se ejecutan en cada PR, no manualmente al final
- **Feedback rápido:** el desarrollador sabe en 3 minutos si su código funciona
- **Sin sorpresas en deploy:** lo que pasa en CI es exactamente lo que irá a producción

### Branches y pipelines

```
main ─────────────────────────────── Producción
  ↑ merge después de tests
desarrollo ───────────────────────── Integración
  ↑ merge después de code review + CI
dia-XX/feature ───────────────────── Desarrollo del feature
```

Pipeline en este proyecto:
- Push a `main` o `desarrollo` → CI + CD (build + test + publicar artefactos)
- Pull Request hacia `main` → solo CI (build + test, sin deploy)

---

## 9. GitHub Actions: pipeline como código

### ¿Qué es GitHub Actions?

GitHub Actions es el sistema de CI/CD integrado en GitHub. Las pipelines se definen como archivos YAML en `.github/workflows/`. Cuando hay un push o PR, GitHub provisiona una máquina virtual (runner) y ejecuta los pasos definidos.

### Conceptos clave

```yaml
on:                          # Triggers: cuándo se dispara el workflow
  push:
    branches: [ main ]

jobs:                        # Conjuntos de steps que corren juntos
  backend:                   # Nombre del job
    runs-on: ubuntu-latest   # Sistema operativo del runner

    steps:                   # Pasos secuenciales dentro del job
    - name: Paso 1
      uses: actions/checkout@v4    # Acción pública de GitHub

    - name: Paso 2
      run: dotnet build            # Comando de shell
```

### Actions vs commands

```yaml
# Action: paso reutilizable publicado en GitHub Marketplace
- uses: actions/checkout@v4        # clona el repo (hay miles de actions publicadas)
- uses: actions/setup-dotnet@v4    # instala .NET SDK
- uses: actions/upload-artifact@v4 # sube artefactos

# Command: cualquier comando de shell
- run: dotnet restore
- run: dotnet build --no-restore --configuration Release
- run: npm ci
```

### Jobs paralelos vs secuenciales

```yaml
jobs:
  backend:     # Job A
    ...
  frontend:    # Job B — corre EN PARALELO con Job A (no tiene 'needs:')
    ...

# Para ejecutar en secuencia:
  deploy:
    needs: [backend, frontend]   # espera a que ambos terminen
    ...
```

En este proyecto, `backend` y `frontend` corren en paralelo porque son independientes. El tiempo total es el del job más lento, no la suma de ambos.

### Condicionales: `if:`

```yaml
- name: Publicar API
  if: github.ref == 'refs/heads/main'   # solo se ejecuta en push a main
  run: dotnet publish ...
```

Esto permite tener steps que solo corren en ciertos contextos:
- En PRs: solo compila y testea (no publiques a producción código sin review)
- En `main`: compila, testea Y publica artefactos listos para deploy

### Artefactos

```yaml
- name: Subir artefacto de API
  uses: actions/upload-artifact@v4
  with:
    name: api-publish
    path: ./publish
```

Los artefactos son archivos generados durante el pipeline que se pueden descargar desde la UI de GitHub Actions. En un pipeline de CD completo, un job de deploy los descargaría con `download-artifact` y los enviaría a AWS.

---

## 10. AWS: los servicios clave para esta app

DaCodes es AWS Partner. Entender estos servicios es diferenciador en la entrevista.

### Los 5 servicios que usamos (o planificamos usar)

| Servicio | Categoría | Qué hace | Costo Free Tier |
|---------|-----------|---------|----------------|
| **EC2** | Compute | Máquina virtual en la nube | 750 hrs/mes t2.micro |
| **Elastic Beanstalk** | PaaS | Deploy automático en EC2 | Gratis (pagas el EC2) |
| **RDS** | Base de datos | SQL Server administrado | 750 hrs/mes db.t3.micro |
| **S3** | Storage | Archivos estáticos (frontend) | 5 GB + 20,000 peticiones |
| **CloudFront** | CDN | Distribuir frontend globalmente | 1 TB de transferencia |

### Analogías con tecnologías conocidas

| AWS | Equivalente | Diferencia clave |
|-----|------------|-----------------|
| EC2 | VPS (DigitalOcean, Linode) | AWS tiene más servicios integrados |
| RDS | SQL Server local | AWS gestiona backups, patches, alta disponibilidad |
| S3 | Carpeta de archivos | Acceso HTTP, infinitamente escalable |
| Elastic Beanstalk | Heroku | AWS lo hace, pero tú controlas la infraestructura |
| CloudFront | Cloudflare CDN | Integración nativa con S3 y EB |

### El flujo de deploy completo

```
Developer hace push a main
        ↓
GitHub Actions compila y testea
        ↓
GitHub Actions publica artefactos
        ↓
Job de deploy (CD):
  API → Elastic Beanstalk (EC2 + Load Balancer) ← usa tu Dockerfile
  BD  → RDS SQL Server                           ← connection string apunta aquí
  Frontend → S3 + CloudFront                     ← archivos estáticos de Vue.js
```

---

## 11. Elastic Beanstalk vs EC2 vs ECS

Esta comparación demuestra que entiendes las trade-offs de infraestructura.

### EC2 (IaaS — Infrastructure as a Service)

Tú controlas todo. Pagas por la máquina, no por el servicio.

```bash
# Lo que tienes que hacer tú:
aws ec2 run-instances ...            # crear la VM
ssh ec2-user@IP                      # conectarte
sudo yum install docker -y           # instalar Docker
sudo systemctl start docker          # iniciar Docker
docker pull mi-imagen                # descargar la imagen
docker run -d -p 80:80 mi-imagen    # correr la app
# Y cuando escales: repetir todo en cada nueva instancia
```

**Cuándo usar:** control total, configuraciones muy específicas, legacy apps.

### Elastic Beanstalk (PaaS — Platform as a Service)

AWS gestiona la infraestructura. Tú solo das el código (o Dockerfile).

```bash
eb init -p docker EcommerceNet     # inicializar EB con soporte Docker
eb create ecommercenet-env         # AWS crea EC2 + Load Balancer + Auto Scaling + Health Checks
git push && eb deploy              # desplegar nueva versión
```

**Lo que EB gestiona automáticamente:**
- Aprovisionamiento de EC2
- Load Balancer (distribuye tráfico)
- Auto Scaling (más instancias si hay más tráfico)
- Health checks (reinicia si la app falla)
- Actualizaciones rolling (sin downtime)

**Cuándo usar:** apps web típicas, demos, startups que quieren velocidad. **Recomendado para la entrevista.**

### ECS Fargate (Containers as a Service)

AWS corre tus contenedores sin que tengas que gestionar servidores.

```json
{
  "taskDefinition": "ecommercenet-api",
  "image": "mi-ecr-repo/ecommercenet:latest",
  "cpu": "256",
  "memory": "512"
}
```

**Lo que Fargate gestiona:**
- No hay EC2 que administrar — AWS provisiona la capacidad
- Pago por uso exacto (CPU y memoria mientras el contenedor corre)
- Integración nativa con ALB, ECR, CloudWatch

**Cuándo usar:** microservicios a escala, cuando quieres pagar solo por lo que usas, producción real.

### Tabla de comparación

| Criterio | EC2 | Elastic Beanstalk | ECS Fargate |
|---------|-----|-------------------|------------|
| Control | Total | Medio | Bajo |
| Complejidad | Alta | Baja | Media |
| Costo Free Tier | ✅ t2.micro | ✅ (paga el EC2) | ❌ |
| Tiempo de setup | Horas | Minutos | Media hora |
| Auto-scaling | Manual | Automático | Automático |
| Ideal para | Legacy, control | Demos, startups | Producción, microservicios |

---

## 12. Análisis línea por línea: Dockerfile

Ruta: [src/EcommerceNet.API/Dockerfile](../src/EcommerceNet.API/Dockerfile)

```dockerfile
# ============================================================
# Etapa 1: COMPILAR con el SDK completo (.NET 8)
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
```

- `FROM` es siempre la primera instrucción (obligatoria)
- `mcr.microsoft.com` = Microsoft Container Registry (registro oficial de Microsoft)
- `dotnet/sdk:8.0` = imagen con el SDK completo: compilador C#, dotnet CLI, targets de build
- `AS build` = alias de esta etapa para referenciarlo desde la Etapa 2
- Esta imagen pesa ~800 MB y es temporal — no va a producción

```dockerfile
WORKDIR /app
```

- Crea `/app` dentro del contenedor y lo establece como directorio activo
- Todos los COPY y RUN siguientes son relativos a `/app`
- Si `/app` no existe, Docker lo crea

```dockerfile
COPY EcommerceNet.sln .
COPY src/EcommerceNet.Core/EcommerceNet.Core.csproj src/EcommerceNet.Core/
COPY src/EcommerceNet.Data/EcommerceNet.Data.csproj src/EcommerceNet.Data/
COPY src/EcommerceNet.API/EcommerceNet.API.csproj src/EcommerceNet.API/
COPY tests/EcommerceNet.Tests/EcommerceNet.Tests.csproj tests/EcommerceNet.Tests/
```

- **Estrategia de caché:** solo copia los archivos de proyecto (`.sln` y `.csproj`)
- Los `.csproj` cambian raramente (solo cuando agregas paquetes NuGet)
- Si estos archivos no cambiaron desde el último build, Docker reutiliza la capa siguiente:

```dockerfile
RUN dotnet restore
```

- Esta capa se cachea mientras los `.csproj` no cambien
- Sin esta estrategia, cada cambio de código desencadenaría un restore completo (2-5 min)
- Con la estrategia, un cambio de código solo recompila, no descarga paquetes

```dockerfile
COPY . .
```

- Ahora sí copia TODO el código fuente
- Va después del restore para aprovechar el caché
- `.dockerignore` (en la misma carpeta que el Dockerfile) excluye `bin/`, `obj/` y archivos del IDE

```dockerfile
RUN dotnet publish src/EcommerceNet.API -c Release -o /publish
```

- `-c Release`: modo optimizado (sin símbolos de debug, con optimizaciones del compilador)
- `-o /publish`: todos los archivos necesarios para ejecutar la API quedan en `/publish`
- Incluye: `EcommerceNet.API.dll`, todas las DLLs de dependencias, `appsettings.json`

```dockerfile
# ============================================================
# Etapa 2: EJECUTAR con solo el ASP.NET runtime (imagen ligera)
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
```

- **Segundo `FROM`** — aquí comienza la Etapa 2
- La Etapa 1 se descarta completamente (incluyendo código fuente y SDK)
- `dotnet/aspnet:8.0`: imagen con solo el ASP.NET Core Runtime (~200 MB)
  - Puede ejecutar apps compiladas
  - NO puede compilar código C#
  - NO tiene dotnet CLI

```dockerfile
WORKDIR /app
COPY --from=build /publish .
```

- `--from=build`: no copia desde el host, sino desde la Etapa 1 (alias `build`)
- Solo los binarios del directorio `/publish` llegan a la imagen final
- El código fuente, las herramientas del SDK, los archivos temporales → no existen en esta imagen

```dockerfile
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
```

- `EXPOSE 80`: documentación (no abre el puerto — eso lo hace `ports:` en docker-compose)
- `ASPNETCORE_URLS=http://+:80`:
  - `+` = todas las interfaces de red del contenedor
  - Sin esto, Kestrel escucha solo en `localhost` → inaccesible desde fuera del contenedor
  - Con esto, Kestrel escucha en `0.0.0.0:80` → accesible desde el host vía `localhost:5000`

```dockerfile
ENV ASPNETCORE_ENVIRONMENT=Production
```

- Activa configuración de producción:
  - Swagger UI desactivado (no expongas la documentación de tu API en producción)
  - Developer Exception Page desactivada (no expongas stack traces)
  - Optimizaciones de caché activadas

```dockerfile
ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]
```

- Formato exec (array JSON): `dotnet` es el PID 1 del contenedor
- Las señales del OS (SIGTERM, SIGINT) llegan directamente a dotnet
- Permite un shutdown graceful (finaliza requests en vuelo antes de apagarse)

---

## 13. Análisis línea por línea: docker-compose.yml

Ruta: [docker-compose.yml](../docker-compose.yml)

```yaml
version: '3.8'
```

Define la versión del formato de docker-compose. 3.8 soporta: `depends_on`, `healthcheck`, `configs`, `secrets`. Es compatible con Docker Engine 19.03+.

### Servicio `api`

```yaml
services:
  api:
    build:
      context: .
      dockerfile: src/EcommerceNet.API/Dockerfile
```

- `context: .`: Docker empaqueta **todo el directorio raíz** y lo envía al daemon de Docker para el build
- Por eso el Dockerfile puede hacer `COPY EcommerceNet.sln .` — el `.sln` está en la raíz
- Si el contexto fuera `src/EcommerceNet.API/`, el Dockerfile no encontraría el `.sln`
- `dockerfile`: ruta al Dockerfile relativa al contexto

```yaml
    ports:
      - "5000:80"
```

- Formato `HOST:CONTENEDOR`
- `5000`: puerto en tu máquina → accedes con `http://localhost:5000/swagger`
- `80`: puerto donde Kestrel escucha dentro del contenedor (definido en `ASPNETCORE_URLS`)
- El mapeo de puertos es unidireccional: host puede acceder al contenedor, pero el contenedor NO puede acceder al host por este mecanismo

```yaml
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=EcommerceNetDB;...
      - Jwt__Key=EstaEsMiClaveSecretaSuperSeguraDe256BitsParaJWT!!
      - Jwt__Issuer=EcommerceNet.API
      - Jwt__Audience=EcommerceNet.Web
      - MongoDB__ConnectionString=mongodb://mongo:27017
      - MongoDB__DatabaseName=EcommerceNetDB
      - ASPNETCORE_ENVIRONMENT=Development
```

- El patrón `__` (doble guión bajo) mapea a la jerarquía de `appsettings.json`:
  ```
  ConnectionStrings__DefaultConnection → appsettings["ConnectionStrings"]["DefaultConnection"]
  Jwt__Key                             → appsettings["Jwt"]["Key"]
  MongoDB__ConnectionString            → appsettings["MongoDB"]["ConnectionString"]
  ```
- `Server=sqlserver`: el hostname del contenedor SQL Server en la red interna de Docker
- Docker Compose crea DNS automático: el nombre del servicio es resolvible como hostname
- `ASPNETCORE_ENVIRONMENT=Development`: activa Swagger UI para la demo (en producción iría `Production`)

```yaml
    depends_on:
      - sqlserver
```

- Docker Compose arranca `sqlserver` antes de `api`
- **Limitación importante:** solo espera que el *contenedor* arranque, no que SQL Server esté listo para aceptar conexiones
- SQL Server tarda ~8-10 segundos en estar listo después de que el contenedor inicia
- En producción se agrega `healthcheck:` + `condition: service_healthy` para esperar de verdad

### Servicio `sqlserver`

```yaml
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=YourStrong!Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
```

- `image:` (vs `build:`): usa una imagen pública directamente, sin Dockerfile propio
- `ACCEPT_EULA=Y`: obligatorio para SQL Server en Linux — acepta la licencia de Microsoft
- `MSSQL_SA_PASSWORD`: contraseña del usuario `sa` (system administrator)
  - Requisito mínimo: 8 caracteres, mayúsculas, minúsculas, números y símbolo
  - En producción: usar `secrets:` de Docker o AWS Secrets Manager
- `1433:1433`: puerto estándar de SQL Server — permite conectarte con SSMS desde el host
- `/var/opt/mssql`: directorio donde SQL Server guarda archivos `.mdf` (datos) y `.ldf` (log)

### Servicio `mongo`

```yaml
  mongo:
    image: mongo:7
    ports:
      - "27017:27017"
    volumes:
      - mongo-data:/data/db
```

- MongoDB es opcional en este proyecto (historial de búsquedas)
- `27017`: puerto estándar de MongoDB
- `/data/db`: donde MongoDB guarda sus archivos de datos
- Si este servicio no arranca, la API falla silenciosamente en las búsquedas (el servicio está configurado para ignorar errores de MongoDB)

### Sección `volumes`

```yaml
volumes:
  sqlserver-data:
  mongo-data:
```

- Declara los volúmenes nombrados que Docker gestiona
- Sin esta sección, los volúmenes referenciados en `services:` darían error
- `docker volume ls` — ver todos los volúmenes
- `docker volume inspect sqlserver-data` — ver detalles (dónde está en el host)
- `docker-compose down -v` — eliminar contenedores Y volúmenes (reset completo)

---

## 14. Análisis línea por línea: ci-cd.yml

Ruta: [.github/workflows/ci-cd.yml](../.github/workflows/ci-cd.yml)

```yaml
name: CI/CD EcommerceNet
```

El nombre aparece en la UI de GitHub Actions en la pestaña "Actions" del repositorio.

### Triggers (cuándo se dispara)

```yaml
on:
  push:
    branches: [ main, desarrollo ]
  pull_request:
    branches: [ main ]
```

- `push` a `main` → pipeline completo: build + test + publicar artefactos
- `push` a `desarrollo` → pipeline completo (sin deploy si usas `if: github.ref`)
- `pull_request` hacia `main` → solo CI: build + test (para verificar antes del merge)
- Cualquier otro branch (ej: `dia-05/deploy-aws`) → NO dispara el pipeline

### Job `backend`

```yaml
jobs:
  backend:
    name: Backend (.NET)
    runs-on: ubuntu-latest
```

- `ubuntu-latest`: máquina virtual con Ubuntu (la versión actual es 22.04)
- GitHub provisiona una VM nueva para cada run — sin estado previo
- La VM tiene preinstalado: Git, Docker, Python, Node.js, y el SDK de .NET (en caché)

```yaml
    steps:
    - name: Checkout código
      uses: actions/checkout@v4
```

- `actions/checkout` es la Action más usada en GitHub Actions
- Clona el repositorio en la VM del runner
- Sin este step, los siguientes steps no tendrían el código

```yaml
    - name: Configurar .NET SDK
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
```

- Instala el .NET SDK 8.0.x más reciente (8.0.400, 8.0.402, etc.)
- GitHub cachea automáticamente el SDK entre runs del mismo branch
- La `x` es un comodín: usa el patch más reciente disponible

```yaml
    - name: Restaurar dependencias
      run: dotnet restore
```

- Descarga todos los paquetes NuGet declarados en los `.csproj`
- GitHub Actions cachea automáticamente los paquetes NuGet (`~/.nuget/packages`)
- En builds subsecuentes, este step toma segundos en lugar de minutos

```yaml
    - name: Compilar solución
      run: dotnet build --no-restore --configuration Release
```

- `--no-restore`: no repite el restore (ya se hizo en el step anterior)
- `--configuration Release`: compilación optimizada (como irá a producción)
- Si hay errores de compilación, el step falla y el pipeline se detiene

```yaml
    - name: Ejecutar pruebas
      run: dotnet test --no-build --configuration Release --verbosity normal
```

- `--no-build`: no recompila (ya se hizo en el step anterior)
- Este step ejecuta los **23 tests** del proyecto
- Si **cualquier test falla** → step falla → job falla → el PR queda bloqueado
- `--verbosity normal`: muestra el nombre de cada test ejecutado (útil para ver cuál falló)

```yaml
    - name: Publicar API
      if: github.ref == 'refs/heads/main'
      run: dotnet publish src/EcommerceNet.API -c Release -o ./publish
```

- `if:` condicional: solo se ejecuta cuando el trigger es un push a `main`
- En PRs, `github.ref` es `refs/pull/123/merge` → la condición es falsa → step se salta
- Genera los binarios listos para deploy en `./publish`

```yaml
    - name: Subir artefacto de API
      if: github.ref == 'refs/heads/main'
      uses: actions/upload-artifact@v4
      with:
        name: api-publish
        path: ./publish
```

- Sube el directorio `./publish` como un artefacto descargable
- Visible en la UI de GitHub Actions → Run → Artifacts
- Útil para: descargar manualmente, usar en un job de deploy con `download-artifact`
- Los artefactos se retienen 90 días por defecto

### Job `frontend`

```yaml
  frontend:
    name: Frontend (Vue.js)
    runs-on: ubuntu-latest
```

Este job corre **en paralelo** con el job `backend` porque no tiene `needs:`. GitHub provisiona dos runners simultáneamente.

```yaml
    steps:
    - uses: actions/checkout@v4

    - name: Configurar Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '20'
```

Node.js 20 es la versión LTS (Long Term Support) actual. Usar LTS en CI garantiza estabilidad.

```yaml
    - name: Instalar dependencias
      working-directory: src/EcommerceNet.Web
      run: npm ci
```

- `working-directory`: cambia el directorio de trabajo solo para este step
- `npm ci` vs `npm install`:
  - `npm install`: puede actualizar `package-lock.json`, más lento
  - `npm ci`: usa versiones exactas del `package-lock.json`, más rápido, falla si hay inconsistencias
  - En CI siempre usar `npm ci` para builds reproducibles

```yaml
    - name: Compilar frontend
      working-directory: src/EcommerceNet.Web
      run: npm run build
```

Ejecuta Vite en modo producción:
- Transpila TypeScript/JSX
- Minifica JS y CSS
- Genera hash en nombres de archivos para cache busting
- Resultado: directorio `dist/` con los archivos estáticos

---

## 15. DaCodes: Studios, Pods y cultura técnica

### Los 4 Studios

| Studio | Qué hacen | El Pod relevante |
|--------|-----------|-----------------|
| **Software Engineering & QA** | Desarrollo fullstack, QA automatizado | Launch Pod |
| **Cloud & DevOps** | AWS, CI/CD, containerización, migraciones | AWS Migration Pod |
| **AI & Data** | ML, GenAI, data pipelines | GenAI Accelerator |
| **Product Strategy & Design** | UX/UI, product management, discovery | Discovery Sprint |

### Los Pods: modelo de trabajo

Un **Pod** es un equipo multidisciplinario autónomo que entrega un proyecto de principio a fin:

**Launch Pod** (el más relevante para esta vacante):
```
┌─────────────────────────────────────────┐
│              LAUNCH POD                  │
│  Backend Dev + Frontend Dev + QA + PM   │
│  DevOps embebido en el equipo           │
│                                          │
│  Entregables: app completa con CI/CD,   │
│  tests desde el día 1, deploy en AWS    │
└─────────────────────────────────────────┘
```

**Por qué importa para la entrevista:**
- QA no es un departamento separado — está en el equipo
- CI/CD no es responsabilidad de DevOps — todos lo configuran
- Este proyecto DEMUESTRA que puedes trabajar en ese modelo:
  - 23 tests unitarios (QA desde día 1)
  - GitHub Actions (CI/CD)
  - Dockerfile + docker-compose (containerización)
  - Clean Architecture (mantenibilidad)

### Cómo responder "¿Por qué DaCodes?"

> "Me atrae el modelo de Launch Pod donde QA y DevOps están integrados en el equipo desde el día uno — no como silos separados. En mi proyecto EcommerceNet demostré exactamente ese enfoque: tests unitarios desde la primera entidad, CI/CD con GitHub Actions, y Dockerfile multi-stage listo para AWS. También que sean AWS Partner con un Cloud & DevOps Studio alineado con mi experiencia de deploy en Elastic Beanstalk."

---

## 16. Preguntas de entrevista con respuestas memorizables

### Sobre Docker

**P: "¿Qué es un contenedor y en qué se diferencia de una VM?"**
> "Un contenedor es un proceso aislado que comparte el kernel del OS host. Una VM tiene su propio OS completo. Los contenedores arrancan en segundos y pesan cientos de MB; las VMs tardan minutos y pesan gigabytes. Para apps web, los contenedores son la opción correcta."

**P: "¿Qué es un multi-stage Dockerfile y por qué lo usas?"**
> "Un Dockerfile con múltiples bloques FROM. El primero compila la app con el SDK completo (800 MB). El segundo toma solo los binarios compilados y los pone en la imagen de runtime (200 MB). La imagen final no tiene compilador, código fuente, ni herramientas — menos peso y menos superficie de ataque."

**P: "¿Para qué sirve docker-compose?"**
> "Para orquestar múltiples contenedores con un solo archivo YAML y un solo comando. En lugar de correr manualmente docker run para la API, para SQL Server y para MongoDB con todas sus variables de entorno y networking — docker-compose lo hace todo. También gestiona volúmenes para persistir datos y define el orden de arranque."

### Sobre CI/CD

**P: "¿Qué es CI/CD?"**
> "CI es Integración Continua: cada push dispara automáticamente compilación y tests. Si falla, el desarrollador sabe al instante. CD es Despliegue Continuo: si CI pasa, la app se despliega automáticamente. Juntos eliminan el problema de 'en mi máquina sí funciona' y el 'sorpresa del viernes a las 5pm cuando haces deploy'."

**P: "¿Cómo funciona tu pipeline de GitHub Actions?"**
> "Dos jobs paralelos: backend y frontend. Backend instala .NET 8, hace restore, compila en Release, ejecuta los 23 tests, y si estamos en main publica artefactos. Frontend instala Node 20, hace npm ci, y ejecuta vite build. Si cualquier test falla, el pipeline bloquea el merge del PR. Tardamos ~3 minutos en tener feedback."

**P: "¿Por qué usas GitHub Actions y no Jenkins?"**
> "GitHub Actions es gratuito en repos públicos, el YAML vive en el repositorio (infrastructure as code), se integra nativamente con los PRs de GitHub, y no requiero un servidor dedicado para el CI. Jenkins requiere infraestructura propia, mantenimiento, y plugins. Para este proyecto y para startups, GitHub Actions es la elección correcta."

### Sobre AWS

**P: "¿Tienes experiencia con AWS?"**
> "Sí. Desplegué EcommerceNet en AWS: la API en Elastic Beanstalk usando el Dockerfile multi-stage, y el frontend como sitio estático en S3. El pipeline de GitHub Actions compila, testea, y cuando hay push a main publica los artefactos listos para deploy. Sé que DaCodes usa el modelo AWS Migration Pod — este flujo es exactamente esa filosofía."

**P: "¿Diferencia entre Elastic Beanstalk y EC2?"**
> "EC2 es IaaS: te da una VM y tú instalas, configuras y mantienes todo. EB es PaaS: le das el código o el Dockerfile y AWS gestiona la infraestructura — load balancer, auto scaling, health checks, rolling updates. Para una app web típica, EB es la opción correcta porque elimina semanas de configuración de infraestructura."

**P: "¿Para qué usarías S3 en este proyecto?"**
> "Para servir el frontend de Vue.js. Después de `npm run build`, Vite genera HTML, CSS y JS estáticos. Esos archivos no necesitan un servidor web — S3 los sirve directamente con hosting estático. Con CloudFront encima, tienes CDN global. Es más barato que un EC2 y escala infinitamente."

### Preguntas de arquitectura

**P: "¿Por qué Clean Architecture?"**
> "Para que las capas externas (API, base de datos) dependan de las internas (Core), y nunca al revés. El Core tiene la lógica de negocio pura, sin saber si la BD es SQL Server o MongoDB. Puedo cambiar EF Core por Dapper, o SQL Server por PostgreSQL, sin tocar los servicios ni los tests. Los 23 tests solo dependen del Core — corren sin BD."

**P: "¿Cómo manejas errores en la API?"**
> "Middleware centralizado de manejo de errores. Un try-catch en cada controlador es código duplicado y difícil de mantener. El middleware intercepta cualquier excepción no manejada, la loguea, y retorna un JSON consistente con el código HTTP correcto. Los controladores quedan limpios."

---

## 17. Glosario de palabras clave del Día 5

| Término | Definición en 1 línea |
|---------|----------------------|
| **Contenedor** | Proceso aislado con su propio sistema de archivos, red y variables de entorno |
| **Imagen Docker** | Snapshot inmutable del sistema de archivos; plantilla para crear contenedores |
| **Capa (layer)** | Incremento inmutable en una imagen Docker; cacheable por Docker |
| **Multi-stage build** | Dockerfile con múltiples FROM; la imagen final solo contiene lo del último stage |
| **docker-compose** | Herramienta para definir y ejecutar múltiples contenedores con un archivo YAML |
| **Volumen nombrado** | Almacenamiento persistente gestionado por Docker; sobrevive a `docker-compose down` |
| **Bind mount** | Carpeta del host montada dentro del contenedor; para desarrollo local |
| **DNS de Docker** | Resolución automática de nombres: el nombre del servicio es su hostname en la red interna |
| **CI** | Continuous Integration; cada push compila y ejecuta tests automáticamente |
| **CD** | Continuous Deployment; si CI pasa, la app se despliega automáticamente |
| **GitHub Actions** | Sistema de CI/CD de GitHub; workflows en YAML en `.github/workflows/` |
| **Runner** | Máquina virtual donde GitHub ejecuta el workflow |
| **Job** | Conjunto de steps que corren en el mismo runner |
| **Step** | Paso individual en un job: una Action o un comando de shell |
| **Action** | Paso reutilizable publicado en GitHub Marketplace (ej: `actions/checkout`) |
| **Artefacto** | Archivo generado durante el pipeline descargable desde la UI de GitHub Actions |
| **Trigger (`on:`)** | Evento que dispara el workflow (push, pull_request, schedule, etc.) |
| **EC2** | Elastic Compute Cloud; máquina virtual en AWS (IaaS) |
| **Elastic Beanstalk** | PaaS de AWS; gestiona EC2, load balancer y auto scaling automáticamente |
| **RDS** | Relational Database Service; base de datos administrada en AWS (SQL Server, PostgreSQL, etc.) |
| **S3** | Simple Storage Service; almacenamiento de archivos con acceso HTTP en AWS |
| **CloudFront** | CDN de AWS; distribuye contenido (archivos S3) globalmente desde edge locations |
| **Free Tier** | Nivel gratuito de AWS; 750 hrs/mes de t2.micro durante los primeros 12 meses |
| **IaaS** | Infrastructure as a Service; el proveedor da hardware, tú gestionas el OS y la app |
| **PaaS** | Platform as a Service; el proveedor gestiona OS e infraestructura, tú solo la app |
| **ASPNETCORE_URLS** | Variable de entorno que configura en qué interfaces y puertos escucha Kestrel |
| **ASPNETCORE_ENVIRONMENT** | Activa/desactiva features según entorno: Development (Swagger) vs Production |
| **npm ci** | Instala dependencias exactas del package-lock.json; más rápido y determinístico que npm install |
| **Launch Pod** | Modelo de DaCodes: equipo multidisciplinario con QA y DevOps embebidos |
| **AWS Migration Pod** | Modelo de DaCodes para containerizar y migrar apps a AWS |
| **ENTRYPOINT** | Instrucción Dockerfile que define el proceso principal del contenedor (PID 1) |
| **context (Docker)** | Directorio que Docker empaqueta y envía al daemon para el build |
| **depends_on** | En docker-compose, define el orden de arranque de servicios |
| **EXPOSE** | Instrucción Dockerfile que documenta el puerto (no lo abre; eso lo hace `ports:`) |
| **github.ref** | Variable de contexto en GitHub Actions con la referencia del branch/tag que disparó el workflow |
| **upload-artifact** | Action de GitHub que sube archivos del runner para descarga posterior |
| **Graceful shutdown** | Proceso de apagado que termina requests en vuelo antes de cerrarse |
| **Surface de ataque** | Conjunto de puntos donde un atacante podría comprometer el sistema; menos código = menos riesgo |
