# Diccionario Técnico — EcommerceNet
> Todos los términos técnicos, siglas, comandos y nomenclaturas del proyecto.
> Organizado por categorías, orden alfabético dentro de cada una.
> Generado: 2026-04-09 | Basado en lectura completa del código fuente.

---

## 1. Siglas y acrónimos

**AAA** — Arrange-Act-Assert. Estructura estándar de una prueba unitaria: preparar datos, ejecutar el método, verificar el resultado. Patrón usado en `ProductoTests.cs`, `CarritoTests.cs`, `OrdenTests.cs`.

**ACID** — Atomicity, Consistency, Isolation, Durability. Propiedades que garantizan que las transacciones de base de datos sean confiables. EF Core con `SaveChangesAsync()` provee atomicidad: si falla algo, nada se guarda.

**AJAX** — Asynchronous JavaScript and XML. Técnica para hacer peticiones HTTP desde el navegador sin recargar la página. En `legacy.html` se usa `$.ajax()` de jQuery para consumir la API.

**API** — Application Programming Interface. Interfaz que permite que dos sistemas se comuniquen. En el proyecto, la API REST en `src/EcommerceNet.API` expone 20+ endpoints HTTP para el frontend.

**ARN** — Amazon Resource Name. Identificador único de un recurso en AWS. En `bucket-policy.json`: `"Resource": "arn:aws:s3:::ecommercenet-ramiro671/*"`.

**AWS** — Amazon Web Services. Plataforma de servicios en la nube de Amazon. El proyecto usa Elastic Beanstalk (backend) y S3 (frontend). El proyecto se despliega en AWS.

**BCrypt** — Algoritmo de hashing de contraseñas diseñado para ser lento y resistente a ataques de fuerza bruta. Incluye un salt aleatorio y un work factor configurable. Usado en `AuthServicio.cs`.

**BSON** — Binary JSON. Formato de serialización binario usado por MongoDB para almacenar documentos. Más eficiente que JSON para operaciones internas. Ver atributos `[BsonId]`, `[BsonRepresentation]` en `BusquedaHistorial.cs`.

**CDN** — Content Delivery Network. Red de servidores distribuidos geográficamente para entregar contenido más rápido. AWS CloudFront es un CDN. Mencionado en `README.md` como opción para el frontend.

**CI/CD** — Continuous Integration / Continuous Delivery. Práctica de automatizar la compilación, pruebas y despliegue en cada cambio de código. En el proyecto: `.github/workflows/ci-cd.yml` con GitHub Actions.

**CLI** — Command Line Interface. Interfaz de línea de comandos. El proyecto usa `dotnet CLI`, `AWS CLI`, `EB CLI`, `npm CLI`, `git CLI`. Sin GUI — todo desde la terminal.

**CORS** — Cross-Origin Resource Sharing. Mecanismo HTTP que controla qué orígenes pueden hacer peticiones a la API. Configurado en `Program.cs` con `WithOrigins(...)` incluyendo la URL de S3.

**CRUD** — Create, Read, Update, Delete. Las 4 operaciones básicas de persistencia de datos. `IRepositorio<T>` define el contrato CRUD genérico. Los controladores exponen CRUD como endpoints REST.

**CSRF** — Cross-Site Request Forgery. Ataque donde un sitio malicioso engaña al navegador para que haga peticiones en nombre del usuario. JWT stateless previene esto; `[ValidateAntiForgeryToken]` era la defensa en MVC Framework.

**CSS** — Cascading Style Sheets. Lenguaje para estilizar páginas HTML. Usado en los bloques `<style scoped>` de los componentes Vue y en `legacy.html`.

**CTE** — Common Table Expression. Subconsulta con nombre en SQL Server usando `WITH`. Permite escribir queries complejas de forma más legible. Mencionado en la documentación de conceptos SQL avanzados.

**DDD** — Domain-Driven Design. Enfoque de diseño donde la lógica de negocio vive en las entidades del dominio. En el proyecto: `Producto.ReducirStock()`, `Carrito.AgregarProducto()`, `Orden.Cancelar()` contienen la lógica, no los servicios.

**DI** — Dependency Injection. Patrón donde las dependencias se inyectan en el constructor en vez de instanciarse dentro de la clase. En `Program.cs`: `AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>()`. Toda la DI es por constructor en este proyecto.

**DNS** — Domain Name System. Sistema que traduce nombres de dominio a direcciones IP. Los dominios de AWS EB y S3 usan DNS para resolver las URLs públicas del proyecto.

**DOM** — Document Object Model. Representación en árbol del HTML que el navegador mantiene en memoria. Vue.js actualiza el DOM mediante Virtual DOM diffing. jQuery manipula el DOM directamente (`$('#grid').html(...)`).

**DTO** — Data Transfer Object. Clase simple que transporta datos entre capas sin lógica de negocio. Ejemplos: `ProductoDto`, `CarritoDto`, `AuthRespuestaDto`. Evita exponer entidades de BD directamente en la API.

**EB** — Elastic Beanstalk. Servicio PaaS de AWS para desplegar aplicaciones web sin administrar infraestructura. El backend del proyecto se despliega en EB con Docker. Comandos: `eb init`, `eb create`, `eb deploy`.

**EC2** — Elastic Compute Cloud. Servicio IaaS de AWS que provee máquinas virtuales en la nube. EB internamente usa EC2 (`t3.micro`) pero abstrae la administración del servidor.

**ECS** — Elastic Container Service. Servicio de orquestación de contenedores Docker en AWS. Alternativa a EB para deployar contenedores con más control. Mencionado en docs como alternativa avanzada.

**EF** — Entity Framework. ORM de Microsoft para .NET. En este proyecto se usa EF Core 10 (la versión moderna). Ver `AppDbContext.cs`, `RepositorioBase.cs`, `ProductoRepositorio.cs`.

**EIP** — Elastic IP. Dirección IP pública estática asignada a una instancia EC2 en AWS. Permite que la IP no cambie entre reinicios del servidor.

**GUI** — Graphical User Interface. Interfaz gráfica de usuario. La consola web de AWS (aws.amazon.com/console) es una GUI. Los comandos `eb`, `aws`, `dotnet` son CLI, no GUI.

**HMAC** — Hash-based Message Authentication Code. Algoritmo que combina una función hash con una clave secreta para verificar integridad y autenticidad. En el proyecto: `SecurityAlgorithms.HmacSha256` para firmar los tokens JWT.

**HTML** — HyperText Markup Language. Lenguaje de marcado para páginas web. El frontend Vue.js genera HTML dinámicamente. `legacy.html` es HTML estático con jQuery.

**HTTP** — HyperText Transfer Protocol. Protocolo de comunicación entre cliente y servidor web. La API REST usa verbos HTTP (GET, POST, PUT, DELETE) y códigos de estado (200, 201, 400, etc.).

**HTTPS** — HTTP Secure. HTTP con cifrado TLS/SSL. En desarrollo la API usa HTTPS; en producción en EB usa HTTP (puerto 80) porque EB maneja el cifrado externamente. Ver `Program.cs`: `UseHttpsRedirection()` solo en Development.

**IAM** — Identity and Access Management. Servicio de AWS para gestionar usuarios, roles y permisos. Se crea usuario `ecommercenet-deploy` con permisos mínimos (PoLP: Principle of Least Privilege) en vez de usar root.

**IaaS** — Infrastructure as a Service. Modelo cloud donde se alquila infraestructura (servidores, redes, almacenamiento). EC2 es IaaS: tú administras el SO y la app.

**IIS** — Internet Information Services. Servidor web de Microsoft para Windows. Requerido en ASP.NET MVC Framework. ASP.NET Core usa Kestrel y no necesita IIS.

**IP** — Internet Protocol. Protocolo de red para identificar dispositivos. Las URLs de EB y S3 se resuelven a direcciones IP mediante DNS.

**JIT** — Just-In-Time. Compilación en tiempo de ejecución. .NET compila el IL (Intermediate Language) a código nativo JIT en la primera ejecución. En .NET 10 con AOT (Ahead-of-Time) se puede compilar antes.

**JOIN** — Operación SQL que combina filas de dos tablas basándose en una condición. EF Core genera JOINs con `Include()`: `INNER JOIN Categorias ON Productos.CategoriaId = Categorias.Id`.

**JSON** — JavaScript Object Notation. Formato de intercambio de datos basado en texto, legible por humanos. La API devuelve JSON. `appsettings.json`, `bucket-policy.json`, `package.json` son archivos JSON.

**JWT** — JSON Web Token. Estándar para tokens de autenticación. Tiene 3 partes: Header + Payload + Signature, separadas por puntos. Firmado con HMAC-SHA256. Generado en `AuthServicio.cs`, validado en `Program.cs`.

**LATAM** — Latin America. Término geográfico. El mercado objetivo es México y LATAM. Relevante para el contexto de la empresa donde se aplica este proyecto.

**LINQ** — Language Integrated Query. Sistema de consultas integrado en C# que permite consultar colecciones y bases de datos con sintaxis de C#. EF Core traduce LINQ a SQL. Ampliamente usado en los repositorios.

**MFA** — Multi-Factor Authentication. Autenticación con múltiples factores (algo que sabes + algo que tienes). En AWS: MFA con TOTP (Google Authenticator) para el usuario root e IAM. Obligatorio para cuentas de producción.

**MVC** — Model-View-Controller. Patrón de arquitectura. El proyecto usa ASP.NET Core Web API (sin vistas Razor) — es MVC sin la V. Vue.js implementa MVVM en el frontend.

**MVVM** — Model-View-ViewModel. Patrón usado en frameworks frontend reactivos. Las stores de Pinia actúan como ViewModel: contienen estado y lógica que la vista consuma.

**NoSQL** — Not Only SQL. Categoría de bases de datos que no usan el modelo relacional. MongoDB es NoSQL: almacena documentos JSON (BSON) sin esquema fijo. Usado para historial de búsquedas en `HistorialBusquedaServicio.cs`.

**npm** — Node Package Manager. Gestor de paquetes para Node.js. Usado para instalar dependencias del frontend (`npm install`), compilar (`npm run build`) y ejecutar en dev (`npm run dev`).

**NuGet** — Gestor de paquetes oficial de .NET. Equivalente a npm para C#. Los paquetes se declaran en los `.csproj` y se instalan con `dotnet restore`. Ej: `Microsoft.EntityFrameworkCore`, `BCrypt.Net-Next`.

**ORM** — Object-Relational Mapper. Herramienta que traduce entre objetos de un lenguaje de programación y tablas de una base de datos relacional. Entity Framework Core es el ORM del proyecto.

**OS** — Operating System. Sistema operativo. El runner de GitHub Actions usa `ubuntu-latest`. El contenedor Docker en EB corre sobre Linux (imagen `mcr.microsoft.com/dotnet/aspnet:10.0`).

**PaaS** — Platform as a Service. Modelo cloud donde el proveedor gestiona la infraestructura y el SO; tú solo despligas la app. Elastic Beanstalk es PaaS: tú subes el Dockerfile, AWS gestiona EC2, load balancer, health checks.

**PM** — Project Manager. Rol de gestión de proyectos. Los equipos de pods tienen PMs en cada Studio/Pod.

**PoLP** — Principle of Least Privilege. Principio de seguridad que indica dar a cada usuario/proceso solo los permisos mínimos necesarios. Aplicado al crear el usuario IAM `ecommercenet-deploy`.

**QA** — Quality Assurance. Proceso de garantía de calidad del software. El área de QA es parte de los servicios del studio (Software Engineering & QA Studio).

**RDS** — Relational Database Service. Servicio de base de datos relacional gestionado de AWS. Opción para SQL Server en producción (en vez de InMemory). Mencionado en docs como alternativa de producción real.

**REST** — Representational State Transfer. Estilo de arquitectura para APIs web. Usa verbos HTTP (GET, POST, PUT, DELETE), recursos en URLs (`/api/productos`), y respuestas JSON stateless. Toda la API del proyecto es REST.

**S3** — Simple Storage Service. Servicio de almacenamiento de objetos de AWS. En el proyecto: almacena y sirve el frontend Vue.js compilado (`dist/`) como sitio web estático.

**SDK** — Software Development Kit. Conjunto de herramientas para desarrollar aplicaciones en una plataforma. `.NET SDK` incluye compilador, CLI (`dotnet`), librerías. En Docker: `FROM mcr.microsoft.com/dotnet/sdk:10.0`.

**SPA** — Single Page Application. Aplicación web que carga una sola página HTML y actualiza el contenido dinámicamente con JavaScript sin reloads completos. El frontend Vue.js en `EcommerceNet.Web` es una SPA.

**SQL** — Structured Query Language. Lenguaje para consultar y manipular bases de datos relacionales. EF Core genera SQL automáticamente desde LINQ. SQL Server usa T-SQL (Transact-SQL).

**SSH** — Secure Shell. Protocolo para conectarse de forma segura a servidores remotos. Se usa para acceder a instancias EC2. EB abstrae SSH para la mayoría de operaciones.

**SSL** — Secure Sockets Layer. Protocolo de cifrado (predecesor de TLS). El término se usa coloquialmente para HTTPS. Los certificados SSL/TLS protegen las conexiones HTTP.

**SSO** — Single Sign-On. Sistema de autenticación única donde un login da acceso a múltiples sistemas. No implementado en este proyecto, pero mencionado en docs de auth avanzada.

**TLS** — Transport Layer Security. Protocolo criptográfico que asegura las comunicaciones en red. HTTPS = HTTP + TLS.

**TOTP** — Time-based One-Time Password. Algoritmo para generar contraseñas de un solo uso basadas en el tiempo. Usado en MFA de AWS con apps como Google Authenticator o Microsoft Authenticator.

**UI** — User Interface. Interfaz de usuario. El frontend Vue.js es la UI del cliente. Swagger UI es la interfaz para explorar la API.

**URL** — Uniform Resource Locator. Dirección de un recurso en la web. La API usa URLs como `/api/productos/{id}`. El frontend y la API tienen URLs de producción en AWS.

**UUID** — Universally Unique Identifier. Identificador único de 128 bits. MongoDB usa `ObjectId` (similar concepto). SQL Server puede usar `GUID` (equivalente a UUID). El proyecto usa `int` como ID para simplidad.

**UX** — User Experience. Experiencia del usuario. El diseño del frontend Vue.js busca una UX clara con feedback visual (mensajes de éxito/error, estados de carga).

**VM** — Virtual Machine. Máquina virtual — emula hardware completo incluyendo SO. EC2 provee VMs. Docker usa contenedores (más ligeros que VMs — comparten el kernel del host).

**VPC** — Virtual Private Cloud. Red privada virtual en AWS que aísla los recursos. Las instancias EC2/EB se despliegan dentro de una VPC.

**VPN** — Virtual Private Network. Red privada virtual que cifra el tráfico de red. No usado directamente en el proyecto, pero recomendado para conectarse a RDS en producción.

**XML** — eXtensible Markup Language. Formato de datos basado en etiquetas. Predecesor de JSON. `Web.config` en MVC Framework era XML. En ASP.NET Core, se reemplaza por `appsettings.json`.

**YAML** — YAML Ain't Markup Language. Formato de serialización de datos legible por humanos. Usado en `.github/workflows/ci-cd.yml` para definir el pipeline de CI/CD. También en algunos archivos de configuración de EB.

**XSS** — Cross-Site Scripting. Ataque donde código malicioso se inyecta en páginas web. Vue.js escapa automáticamente el HTML en las plantillas (`{{ }}`) previniendo XSS.

---

## 2. Tecnologías y frameworks

**.NET 10** — La versión más reciente del runtime de .NET (multiplataforma, open source). El proyecto usa `net10.0` como `TargetFramework`. Todos los paquetes NuGet están en versión `10.0.5`.

**.NET Core** — La versión moderna y multiplataforma de .NET (también llamada simplemente ".NET" desde la versión 5). Compatible con Linux, Mac y Windows. Predecesor al nombre ".NET 10".

**.NET Framework** — La versión original de .NET, solo para Windows. Considerado legacy. ASP.NET MVC Framework corría sobre .NET Framework.

**ASP.NET Core** — Framework web de Microsoft sobre .NET. Multiplataforma, usa Kestrel, DI nativa, middleware pipeline, `Program.cs`. El proyecto usa ASP.NET Core Web API (sin vistas Razor).

**ASP.NET MVC** — Framework web de Microsoft sobre .NET Framework (legacy). Usa IIS, `Global.asax`, `Web.config`, rutas convencionales. Requiere Windows.

**Axios** — Cliente HTTP para JavaScript/TypeScript. En `api.js`: crea una instancia con `baseURL` y configura interceptores para JWT y manejo de errores 401. Versión `1.7.2`.

**BCrypt.Net-Next** — Librería NuGet de C# para hashear contraseñas con el algoritmo BCrypt. Usada en `AuthServicio.cs`: `BCrypt.Net.BCrypt.HashPassword()` y `BCrypt.Net.BCrypt.Verify()`. Versión `4.1.0`.

**Bash** — Shell de Unix/Linux. El runner de GitHub Actions (`ubuntu-latest`) usa Bash. Los comandos `dotnet`, `npm`, `aws`, `eb` se ejecutan en Bash en CI/CD.

**Bootstrap** — Framework CSS de componentes UI. No usado en este proyecto (el CSS es custom en cada componente Vue con `<style scoped>`).

**Docker** — Plataforma de contenedores que empaqueta una aplicación con todas sus dependencias. El `Dockerfile` raíz construye la imagen de la API para EB usando multi-stage build.

**Docker Desktop** — Aplicación de escritorio para Windows/Mac que instala Docker Engine y Docker Compose. Requerida para desarrollo local con `docker-compose up`.

**ESLint** — Linter de JavaScript/Vue que detecta errores y problemas de estilo en el código. Configurado en `package.json` con `eslint-plugin-vue`. Comando: `npm run lint`.

**Entity Framework Core** — ORM de Microsoft para .NET que traduce objetos C# a SQL. Versión `10.0.5`. Soporta Code First, migraciones, Fluent API, LINQ to SQL. Ver `AppDbContext.cs`.

**GitHub** — Plataforma de alojamiento de repositorios Git. El proyecto está en `github.com/Ramiro671/EcommerceNet`. GitHub Actions corre los pipelines de CI/CD.

**GitHub Actions** — Servicio de CI/CD integrado en GitHub. Se configura con archivos YAML en `.github/workflows/`. El proyecto tiene `ci-cd.yml` con dos jobs paralelos.

**Git** — Sistema de control de versiones distribuido. El proyecto tiene ramas `main` (producción) y `desarrollo` (integración). Commits con prefijos `feat:`, `fix:`, `docs:`, `test:`, `refactor:`.

**IIS** — Internet Information Services. Servidor web de Microsoft. Requerido por ASP.NET MVC Framework en Windows. ASP.NET Core usa Kestrel y no necesita IIS.

**Intl.NumberFormat** — API de JavaScript para formatear números según el locale. En `ProductoCard.vue`: `new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' })` para mostrar precios en pesos.

**jQuery** — Librería de JavaScript (versión `3.7.1`) para manipulación del DOM, AJAX y eventos. En `legacy.html`: consume `GET /api/productos` con `$.ajax()` y renderiza el catálogo manipulando el DOM directamente.

**Kestrel** — Servidor HTTP multiplataforma integrado en ASP.NET Core. Reemplaza IIS para desarrollo y producción. En EB, Kestrel escucha en el puerto 80 (`ENV ASPNETCORE_URLS=http://+:80`).

**LocalDB** — Versión liviana de SQL Server para desarrollo, sin necesidad de instalación completa. Incluida con Visual Studio. Configurada en `appsettings.json` con `Server=(localdb)\\MSSQLLocalDB`.

**MongoDB** — Base de datos NoSQL que almacena documentos en formato BSON. Versión `7.x`. Usada para historial de búsquedas en `HistorialBusquedaServicio.cs` y `BusquedaHistorial.cs`.

**Node.js** — Runtime de JavaScript del lado del servidor. Requerido para ejecutar Vite y npm. El frontend Vue.js se compila con Node.js pero se sirve como archivos estáticos.

**OpenAPI** — Especificación estándar para describir APIs REST (antes llamada Swagger Specification). El proyecto genera la spec automáticamente con Swashbuckle y la expone en `/swagger/v1/swagger.json`.

**Pinia** — Librería de gestión de estado para Vue.js 3 (reemplaza Vuex). Versión `2.1.7`. El proyecto tiene 3 stores: `authStore`, `carritoStore`, `productoStore`. Usa `defineStore()` con Composition API.

**PowerShell** — Shell de comandos de Microsoft para Windows. Los comandos `eb` y `aws` se ejecutan en PowerShell (con backtick `` ` `` para continuar línea en Windows).

**Prettier** — Formateador de código automático para JS/Vue. Versión `3.2.5`. Integrado con ESLint para mantener estilo consistente en el código frontend.

**Python** — Lenguaje de programación. Requerido para instalar el EB CLI (`pip install awsebcli`). Versión `3.12.10`. No se usa Python en el proyecto — solo como dependencia del CLI de EB.

**Razor** — Motor de vistas de ASP.NET Core para generar HTML en el servidor. No usado en este proyecto (es una API pura). Sí era central en ASP.NET MVC Framework.

**SignalR** — Librería de ASP.NET Core para comunicación en tiempo real (WebSockets). No implementada en este proyecto pero mencionada en docs como posible mejora.

**SQL Server** — Sistema de gestión de bases de datos relacionales de Microsoft. En desarrollo: LocalDB. En producción AWS: puede usar RDS SQL Server o InMemory (según `UseInMemoryDatabase` en config).

**Swagger** — Herramienta para documentar y probar APIs REST. Configurado en `Program.cs` con Swashbuckle; disponible en todas las envs (incluyendo producción en EB) en `/swagger`.

**Swashbuckle** — Paquete NuGet que genera automáticamente la interfaz Swagger/OpenAPI para ASP.NET Core. Versión `6.9.0`. Configurado en `Program.cs` con `AddSwaggerGen()`.

**Tailwind CSS** — Framework CSS de utilidades. No usado en este proyecto (CSS custom en cada componente Vue).

**VS Code** — Visual Studio Code. Editor de código ligero. El proyecto incluye configuración en la carpeta `.vs/` (excluida del repo). Compatible con las extensiones de C# y Vue.

**Visual Studio** — IDE completo de Microsoft para .NET. El proyecto es compatible — el archivo `.slnx` permite abrirlo directamente.

**Vite** — Bundler y dev server moderno para frontend. Versión `5.3.1`. Compila los componentes Vue y los assets para producción con `npm run build`. Lee variables de entorno con prefijo `VITE_`.

**Vue Router** — Router oficial para Vue.js. Versión `4.3.3`. Maneja la navegación entre vistas (SPA). Soporta `meta` en rutas para guards de autenticación.

**Vue.js 3** — Framework progresivo de JavaScript para interfaces de usuario. Versión `3.4.29`. Composition API con `<script setup>`. El frontend del proyecto usa Vue 3 con Pinia y Vue Router.

**Vuex** — Librería de estado para Vue.js 2 (legacy). Reemplazada por Pinia en Vue 3. No usada en este proyecto.

**winget** — Gestor de paquetes de Windows. Usado para instalar herramientas: `winget install Amazon.AWSCLI`, `winget install Python.Python.3.12`.

**xUnit** — Framework de pruebas unitarias para .NET. Versión `2.9.x`. Usa atributos `[Fact]`, `[Theory]`, `[InlineData]` y métodos `Assert.*`. Los 23 tests del proyecto están en `tests/EcommerceNet.Tests/`.

---

## 3. Servicios de AWS

**Access Key** — Par de credenciales (Access Key ID + Secret Access Key) para autenticar requests programáticos a AWS desde la CLI o SDK. Solo se muestran una vez al crearlas. Configurar con `aws configure`.

**CloudFront** — CDN (Content Delivery Network) de AWS. Distribuye contenido estático (como el frontend Vue.js) desde servidores cercanos al usuario. Mencionado en `README.md` como mejora de producción.

**CloudWatch** — Servicio de monitoreo y logs de AWS. Elastic Beanstalk envía logs de la aplicación a CloudWatch automáticamente. Útil para debugging en producción.

**EC2** — Elastic Compute Cloud. Máquinas virtuales en AWS. Elastic Beanstalk crea y gestiona instancias EC2 (`t3.micro`) internamente para ejecutar la API.

**ECS** — Elastic Container Service. Servicio de orquestación de contenedores en AWS. Alternativa más avanzada a EB para deployar contenedores Docker con auto-scaling granular.

**EIP (Elastic IP)** — Dirección IP pública estática en AWS. Se asigna a instancias EC2 para que la IP no cambie entre reinicios. Las instancias de EB tienen IP dinámica por defecto.

**Elastic Beanstalk** — Servicio PaaS de AWS que despliega apps automáticamente. Lee el `Dockerfile` raíz, construye la imagen, crea la instancia EC2, configura el load balancer y el health check. Comando: `eb deploy`.

**Free Tier** — Nivel gratuito de AWS (primeros 12 meses). Incluye 750h/mes de EC2 `t2.micro`/`t3.micro`, 5 GB en S3, 750h RDS. El proyecto cabe completamente en Free Tier = $0.

**IAM** — Identity and Access Management. Servicio para gestionar usuarios, grupos, roles y permisos en AWS. Nunca usar el usuario root; crear usuario `ecommercenet-deploy` con permisos mínimos.

**Lambda** — Servicio de funciones serverless de AWS. No usado en el proyecto pero relevante para arquitecturas serverless en AWS. Ejecuta código sin gestionar servidores.

**RDS** — Relational Database Service. Servicio de bases de datos relacionales gestionado de AWS. Soporte para SQL Server, MySQL, PostgreSQL. En el proyecto se usa InMemory en vez de RDS para simplificar el demo.

**Route 53** — Servicio de DNS de AWS. Para configurar dominios personalizados (ej: `api.misitio.com` → Elastic Beanstalk). No configurado en el proyecto (se usan las URLs por defecto de EB y S3).

**S3** — Simple Storage Service. Almacenamiento de objetos en AWS. En el proyecto: aloja el frontend Vue.js (`dist/`) como sitio web estático. Bucket: `ecommercenet-ramiro671`. Acceso público configurado con `bucket-policy.json`.

**Secret Key** — La parte secreta del par de credenciales IAM (`Secret Access Key`). Solo se muestra una vez al crearla. No commitear nunca a git. Configurar con `aws configure`.

**Security Group** — Firewall virtual en AWS que controla el tráfico de entrada y salida a las instancias EC2/EB. Configura qué puertos y protocolos están permitidos.

**SNS** — Simple Notification Service. Servicio de mensajería pub/sub de AWS. No usado en el proyecto; útil para notificaciones en sistemas más grandes.

---

## 4. Comandos y herramientas CLI

**`aws configure`** — Configura las credenciales y región de AWS CLI: pide Access Key ID, Secret Access Key, región (`us-east-1`) y formato de salida (`json`). Guarda en `~/.aws/credentials`.

**`aws rds create-db-instance`** — Crea una instancia de base de datos en RDS. No usado en el proyecto actual (usa InMemory), pero documentado como opción para producción real.

**`aws s3 mb`** — Make Bucket. Crea un bucket de S3. Ejemplo: `aws s3 mb s3://ecommercenet-ramiro671 --region us-east-1`.

**`aws s3 rb`** — Remove Bucket. Elimina un bucket y su contenido. Ejemplo: `aws s3 rb s3://ecommercenet-ramiro671 --force` (--force vacía el bucket antes de eliminarlo).

**`aws s3 sync`** — Sincroniza archivos locales con S3 (solo sube los archivos nuevos o modificados). Ejemplo: `aws s3 sync src/EcommerceNet.Web/dist/ s3://ecommercenet-ramiro671`.

**`aws s3api put-bucket-policy`** — Aplica una política de acceso a un bucket S3 desde un archivo JSON. Ejemplo: `aws s3api put-bucket-policy --bucket ecommercenet-ramiro671 --policy file://bucket-policy.json`.

**`aws s3api put-public-access-block`** — Configura o deshabilita el bloqueo de acceso público de un bucket S3. Necesario antes de habilitar el hosting estático.

**`aws s3 website`** — Configura un bucket S3 como sitio web estático, especificando el documento índice y el de error.

**`aws sts get-caller-identity`** — Muestra la identidad actual del usuario de AWS CLI (Account ID, UserID, ARN). Útil para verificar que las credenciales están configuradas correctamente.

**`dotnet add package`** — Agrega un paquete NuGet al proyecto `.csproj`. Ejemplo: `dotnet add package Microsoft.EntityFrameworkCore.SqlServer`.

**`dotnet build`** — Compila el proyecto o solución. Genera los binarios en `bin/`. Comando de CI en `ci-cd.yml`: `dotnet build --no-restore --configuration Release`.

**`dotnet ef database update`** — Aplica las migraciones pendientes a la base de datos. Equivale a ejecutar el SQL generado por EF Core.

**`dotnet ef migrations add`** — Crea una nueva migración de EF Core basándose en los cambios del modelo. Genera un archivo C# con los métodos `Up()` y `Down()`.

**`dotnet ef migrations remove`** — Elimina la última migración (solo si aún no fue aplicada a la BD). Útil cuando se equivoca en la migración.

**`dotnet publish`** — Publica el proyecto en formato deployable (sin herramientas de desarrollo). El Dockerfile usa `dotnet publish src/EcommerceNet.API -c Release -o /publish`.

**`dotnet restore`** — Descarga los paquetes NuGet declarados en `.csproj`. Primer paso en la compilación. En CI: `dotnet restore`.

**`dotnet run`** — Compila y ejecuta la aplicación. Para desarrollo: `dotnet run --project src/EcommerceNet.API`.

**`dotnet test`** — Ejecuta los proyectos de pruebas. En CI: `dotnet test --no-build --configuration Release --verbosity normal`.

**`eb create`** — Crea un nuevo entorno de Elastic Beanstalk. Ejemplo: `eb create ecommercenet-api --single --instance-type t3.micro --platform Docker`.

**`eb deploy`** — Despliega la versión actual del código al entorno EB. Empaqueta el proyecto, lo sube a S3 y EB reconstruye el contenedor.

**`eb init`** — Inicializa un proyecto de EB en el directorio actual. Configura la región, el nombre de la aplicación y la plataforma (Docker).

**`eb open`** — Abre en el navegador la URL del entorno EB. Equivalente a ir a `http://ecommercenet-api.eba-fxkridvp.us-east-1.elasticbeanstalk.com`.

**`eb setenv`** — Configura variables de entorno en el entorno EB. Ejemplo: `eb setenv Jwt__Key="..." UseInMemoryDatabase=true`. En .NET, `__` separa secciones jerárquicas de appsettings.

**`eb status`** — Muestra el estado del entorno EB (Health: Green/Yellow/Red, URL, versión desplegada).

**`eb terminate`** — Elimina el entorno EB y todos sus recursos (instancia EC2, load balancer, etc.). Útil para limpiar y evitar cobros.

**`gh auth login`** — Autentica la CLI de GitHub (`gh`). Permite crear repositorios y gestionar PRs desde la terminal.

**`gh repo create`** — Crea un repositorio en GitHub desde la terminal.

**`git add`** — Agrega archivos al staging area (índice) para el próximo commit. En el proyecto se usa `git add archivo.cs` (específico) en vez de `git add .` (todo) para control.

**`git branch`** — Lista, crea o elimina ramas. El proyecto usa `main` (producción) y `desarrollo` (integración).

**`git checkout`** — Cambia de rama o restaura archivos. `git checkout -b feature/nueva-feature` crea y cambia de rama.

**`git clone`** — Clona un repositorio remoto a la máquina local.

**`git commit`** — Crea un snapshot del estado actual del staging area. El proyecto usa prefijos: `feat:`, `fix:`, `docs:`, `test:`, `refactor:`, `security:`.

**`git diff`** — Muestra las diferencias entre el estado actual y el último commit (o entre commits).

**`git log`** — Muestra el historial de commits. Con `--all --oneline` muestra todos los commits de todas las ramas en una línea.

**`git merge`** — Fusiona una rama con la rama actual. En el proyecto: las ramas de feature se mergean a `desarrollo`, luego a `main`.

**`git push`** — Envía los commits locales al repositorio remoto. `git push origin main` sube la rama `main` a GitHub.

**`git remote`** — Gestiona las conexiones remotas. `origin` apunta a `github.com/Ramiro671/EcommerceNet`.

**`git rm --cached`** — Elimina un archivo del índice git sin eliminarlo del disco. Usado para dejar de rastrear `appsettings.Production.json` sin borrarlo localmente.

**`git stash`** — Guarda cambios temporalmente sin commitear. Útil para cambiar de rama sin perder el trabajo en progreso.

**`git tag`** — Crea etiquetas para marcar versiones importantes. Ejemplo: `git tag v1.0.0`.

**`npm ci`** — Clean Install. Instala dependencias exactas del `package-lock.json`. Más reproducible que `npm install`. Usado en CI/CD.

**`npm create vue@latest`** — Crea un nuevo proyecto Vue.js con Vite. Así se inició `EcommerceNet.Web`.

**`npm install`** — Instala las dependencias declaradas en `package.json`.

**`npm run build`** — Ejecuta el script `build` de `package.json` (`vite build`). Genera la carpeta `dist/` con los archivos estáticos optimizados para producción.

**`npm run dev`** — Ejecuta el servidor de desarrollo de Vite en `localhost:5173` con hot reload.

**`pip install`** — Instala paquetes Python. Usado para instalar el EB CLI: `pip install awsebcli`.

---

## 5. Atributos y decoradores de C# / .NET

**[AllowAnonymous]** — Permite el acceso sin autenticación a un endpoint o controlador, incluso cuando hay `[Authorize]` en el controlador padre. Los endpoints GET de `ProductosController` son implícitamente anónimos.

**[ApiController]** — Habilita comportamientos automáticos en ASP.NET Core: validación del modelo con respuesta 400 automática, inferencia de fuentes de parámetros (`[FromBody]` en POST), respuestas `ProblemDetails`. Todos los controladores del proyecto lo usan.

**[Authorize]** — Requiere que el usuario esté autenticado (token JWT válido). En `CarritoController`: aplicado a nivel de clase. En `OrdenesController`: aplicado a nivel de clase.

**[Authorize(Roles = "Admin")]** — Requiere autenticación Y que el claim de rol sea "Admin". Usado en `ProductosController` (crear, actualizar, eliminar) y `CategoriasController` (crear, actualizar, desactivar).

**[BsonId]** — Marca una propiedad como el identificador principal (`_id`) de un documento MongoDB. Equivale a la PK en SQL. En `BusquedaHistorial.cs`: `[BsonId] public string? Id { get; set; }`.

**[BsonRepresentation(BsonType.ObjectId)]** — Indica que el `string` en C# se serializa como `ObjectId` de MongoDB (24 caracteres hexadecimales, auto-generado). En `BusquedaHistorial.cs`.

**[Fact]** — Marca un método como prueba unitaria sin parámetros en xUnit. Cada `[Fact]` es un test independiente. Usado en `ProductoTests`, `CarritoTests`, `OrdenTests`.

**[FromBody]** — Indica que el parámetro viene del cuerpo JSON del request HTTP (POST/PUT). Ejemplo: `Crear([FromBody] CrearProductoDto dto)` en `ProductosController`.

**[FromQuery]** — Indica que el parámetro viene de los query params de la URL (`?param=valor`). Ejemplo: `Buscar([FromQuery] string termino)` en `ProductosController`.

**[HttpDelete]** — Marca un método de controlador para responder a solicitudes DELETE. En `CarritoController`: `[HttpDelete("{productoId}")]` elimina un item; `[HttpDelete]` vacía el carrito.

**[HttpGet]** — Marca un método de controlador para responder a solicitudes GET. Ejemplo: `[HttpGet]` lista todos los productos; `[HttpGet("{id}")]` obtiene uno por ID.

**[HttpPost]** — Marca un método de controlador para responder a solicitudes POST. Ejemplo: `[HttpPost("agregar")]` en `CarritoController`, `[HttpPost("login")]` en `AuthController`.

**[HttpPut]** — Marca un método de controlador para responder a solicitudes PUT. Ejemplo: `[HttpPut("{id}")]` actualiza un producto; `[HttpPut("{id}/cancelar")]` cancela una orden.

**[InlineData]** — Provee datos a un `[Theory]`. Permite correr el mismo test con múltiples conjuntos de parámetros. No usado en el proyecto actual (solo `[Fact]`), pero documentado como alternativa.

**[MaxLength]** — Data Annotation que limita la longitud máxima de un string. No usado directamente en las entidades del proyecto (se usa Fluent API: `HasMaxLength(200)` en `AppDbContext`).

**[Required]** — Data Annotation que marca una propiedad como obligatoria. No usado directamente (se usa Fluent API: `IsRequired()` en `AppDbContext`).

**[Route]** — Define el patrón de URL de un controlador o acción. `[Route("api/[controller]")]` usa el nombre del controlador como segmento de URL: `ProductosController` → `/api/productos`.

**[Theory]** — Marca un método como prueba parametrizada en xUnit. Se combina con `[InlineData]` para probar múltiples casos. Más expresivo que múltiples `[Fact]` similares.

**[ValidateAntiForgeryToken]** — Atributo de ASP.NET MVC Framework para proteger formularios HTML contra CSRF. No usado en este proyecto (JWT stateless previene CSRF en APIs).

---

## 6. Directivas y sintaxis de Vue.js

**`@click`** — Evento de clic. Versión abreviada de `v-on:click`. En `NavBar.vue`: `@click="cerrarSesion"`. En `ProductoCard.vue`: `@click="emit('agregar', producto.id)"`.

**`@filtrar` / `@limpiar`** — Eventos personalizados emitidos por componentes hijos. En `CategoriaFiltro.vue`: `emit('filtrar', cat)`. El padre los escucha con `@filtrar="productoStore.filtrarPorCategoria"`.

**`:alt`** — Binding dinámico del atributo `alt` de una imagen. Versión abreviada de `v-bind:alt`. En `ProductoCard.vue`: `:alt="producto.nombre"`.

**`:class`** — Binding dinámico de clases CSS. Permite aplicar clases condicionalmente. En `ProductoCard.vue`: `:class="['stock', producto.stock > 0 ? 'en-stock' : 'sin-stock']"`.

**`:disabled`** — Binding dinámico del atributo `disabled`. En `ProductoCard.vue`: `:disabled="producto.stock <= 0"` deshabilita el botón si no hay stock.

**`:key`** — Atributo requerido en `v-for` para que Vue identifique elementos únicamente y haga diff eficiente. En `TiendaView.vue`: `v-for="producto in productoStore.productosFiltrados" :key="producto.id"`.

**`:src`** — Binding dinámico del atributo `src` de una imagen. En `ProductoCard.vue`: `:src="producto.imagenUrl || 'https://placehold.co/400x300?text=Producto'"`.

**`:to`** — Binding dinámico de la ruta de `RouterLink`. En `ProductoCard.vue`: `:to="'/producto/' + producto.id"`.

**`computed()`** — Crea una propiedad derivada que se recalcula solo cuando cambian sus dependencias reactivas. En `productoStore.js`: `productosFiltrados` filtra productos por categoría y término de búsqueda.

**`createRouter()`** — Crea la instancia del router de Vue Router. En `router/index.js`: `createRouter({ history: createWebHistory(), routes: rutas })`.

**`createWebHistory()`** — Crea un historial basado en la API de History de HTML5 (URLs sin `#`). Usado en `router/index.js` para que las rutas se vean como URLs normales.

**`defineEmits()`** — Declara los eventos que un componente puede emitir al padre. En `ProductoCard.vue`: `const emit = defineEmits(['agregar'])`. En `CategoriaFiltro.vue`: `defineEmits(['filtrar', 'limpiar'])`.

**`defineProps()`** — Declara las propiedades que un componente recibe del padre. En `ProductoCard.vue`: `defineProps({ producto: { type: Object, required: true } })`. En `CategoriaFiltro.vue`: `defineProps({ categorias: Array, seleccionada: String })`.

**`defineStore()`** — Crea una store de Pinia. Primer argumento: nombre único de la store. Segundo argumento: función setup (Composition API). Retorna una función que se usa como `useXxxStore()`.

**`onMounted()`** — Hook del ciclo de vida de Vue. Se ejecuta cuando el componente ya está insertado en el DOM. En `TiendaView.vue`: `onMounted(() => productoStore.cargarProductos())`.

**`reactive()`** — Crea un objeto reactivo profundo. Las propiedades del objeto se rastrean automáticamente. Para primitivos usar `ref()`. No se usa directamente en las stores del proyecto (prefieren `ref()`).

**`ref()`** — Crea un valor reactivo. Accesible en JS con `.value`, en la plantilla directamente. En `authStore.js`: `const usuario = ref(...)`, `const token = ref(...)`.

**`RouterLink`** — Componente de Vue Router para navegación declarativa. Genera un `<a>` sin recargar la página. En `NavBar.vue`: `<RouterLink to="/">Tienda</RouterLink>`.

**`RouterView`** — Componente donde Vue Router renderiza la vista activa según la ruta actual. Generalmente en `App.vue`.

**`<script setup>`** — Syntactic sugar de Vue 3 para la Composition API. Las variables, funciones y componentes declarados son automáticamente accesibles en el template. Todos los componentes del proyecto usan esta sintaxis.

**`<style scoped>`** — Estilos CSS que solo aplican al componente actual (Vue los encapsula con un atributo único). En `NavBar.vue`, `ProductoCard.vue`, `CategoriaFiltro.vue`, `TiendaView.vue`.

**`<template>`** — Bloque HTML del componente Vue. Puede tener `v-if`, `v-for` y otros con múltiples elementos raíz (a diferencia de Vue 2). En `NavBar.vue`: `<template v-if="auth.estaLogueado">`.

**`useAuthStore()`** — Función generada por `defineStore()` para acceder a la store de autenticación. Disponible en cualquier componente sin importar la store directamente.

**`useCarritoStore()`** — Función para acceder a la store del carrito. Expone `items`, `totalItems`, `checkout()`, `agregarProducto()`, etc.

**`useProductoStore()`** — Función para acceder a la store de productos. Expone `productosFiltrados`, `categorias`, `cargarProductos()`, `filtrarPorCategoria()`, etc.

**`useRoute()`** — Devuelve el objeto de la ruta actual (params, query, path). Usado en `ProductoDetalleView.vue` para obtener el `id` del producto de la URL.

**`useRouter()`** — Devuelve la instancia del router. Permite navegar programáticamente. En `NavBar.vue`: `router.push('/')` después de hacer logout.

**`v-else`** — Directiva para la rama `else` de un `v-if`. En `NavBar.vue`: muestra links de Login/Registro cuando el usuario no está logueado.

**`v-else-if`** — Directiva para una rama `else if`. En `TiendaView.vue`: `v-else-if="productoStore.productosFiltrados.length === 0"` muestra mensaje cuando no hay resultados.

**`v-for`** — Directiva para renderizar listas. En `TiendaView.vue`: `v-for="producto in productoStore.productosFiltrados"`. En `CategoriaFiltro.vue`: `v-for="cat in categorias"`.

**`v-if`** — Directiva que renderiza condicionalmente un elemento (lo crea/destruye del DOM). En `TiendaView.vue`: `v-if="productoStore.cargando"` muestra el spinner de carga.

**`v-model`** — Binding bidireccional entre una propiedad reactiva y un input. En `TiendaView.vue`: `v-model="productoStore.terminoBusqueda"` sincroniza el campo de búsqueda con el store.

**`v-show`** — Directiva que muestra/oculta un elemento con CSS (`display: none`), sin crearlo/destruirlo. Más eficiente que `v-if` cuando el elemento cambia frecuentemente.

**`watch()`** — Observa cambios en una propiedad reactiva y ejecuta una función. Alternativa a `computed()` cuando hay efectos secundarios.

---

## 7. Métodos y funciones de jQuery

**`$()` / `jQuery()`** — Selector de jQuery. Selecciona elementos del DOM. `$('#productosGrid')` selecciona el elemento con id `productosGrid`. En `legacy.html`.

**`$.ajax()`** — Realiza peticiones HTTP asíncronas. En `legacy.html`: `$.ajax({ url: API_URL + '/productos', method: 'GET', success: function(response) {...} })`.

**`$.each()`** — Itera sobre un array o objeto. En `legacy.html`: `$.each(response.datos, function(index, producto) { renderizarProducto(producto); })`.

**`.appendTo()`** — Agrega un elemento al final de otro en el DOM. En `legacy.html`: `$(tarjeta).appendTo('#productosGrid')`.

**`.click()`** — Escucha el evento de clic en un elemento. Atajo de `.on('click', handler)`.

**`.data()`** — Lee/escribe datos almacenados en un elemento del DOM. En `legacy.html`: `$(btn).data('id', producto.id)`.

**`.empty()`** — Vacía el contenido HTML de un elemento sin eliminarlo. En `legacy.html`: `$('#productosGrid').empty()` para limpiar el grid antes de re-renderizar.

**`.fadeIn()` / `.fadeOut()`** — Animaciones de aparición/desaparición con fade. En `legacy.html` para mostrar el mensaje de éxito al agregar al carrito.

**`.hide()` / `.show()`** — Muestra u oculta un elemento (equivalente a `display: none`/`block`).

**`.html()`** — Lee o establece el contenido HTML de un elemento. En `legacy.html`: `$('#productosGrid').html('<p class="cargando">Cargando...</p>')`.

**`.on()` / `.off()`** — Agrega o quita event listeners. `.on('click', handler)` es el método moderno (vs `.click()`).

**`.text()`** — Lee o establece el contenido de texto de un elemento (sin HTML). Más seguro que `.html()` para texto dinámico (previene XSS).

**`.trim()`** — Elimina espacios en blanco al inicio y final de un string. En `legacy.html`: `$('#inputBusqueda').val().trim()`.

**`.val()`** — Lee o establece el valor de un input. En `legacy.html`: `$('#inputBusqueda').val()` lee el texto escrito en el campo de búsqueda.

**`$(document).ready()`** — Ejecuta código cuando el DOM está completamente cargado. Equivalente moderno: `$(function() {...})`. En `legacy.html`.

**`setTimeout()` / `clearTimeout()`** — Funciones nativas de JavaScript para ejecutar código después de un delay. En `legacy.html`: oculta el mensaje de éxito después de 3 segundos.

---

## 8. Métodos de EF Core y LINQ

**`AddAsync()`** — Agrega una entidad al DbContext en estado `Added`. No persiste hasta `SaveChangesAsync()`. En `RepositorioBase<T>`.

**`AddDbContext<T>()`** — Registra el DbContext en el contenedor de DI como `Scoped` (una instancia por request). En `Program.cs`.

**`AnyAsync()`** — Devuelve `true` si hay al menos un elemento que cumple la condición. Más eficiente que `.CountAsync() > 0`. En `AuthServicio.cs`: `AnyAsync(u => u.Email == dto.Email)`.

**`Aggregate()`** — Pipeline de agregación de MongoDB. Equivale a `GROUP BY`, `ORDER BY`, `LIMIT` en SQL. En `HistorialBusquedaServicio.cs`.

**`Count()`** — Cuenta elementos en una colección LINQ. En `Categoria.cs`: `Productos.Count(p => p.Activo)`. En MongoDB: `g.Count()` en el pipeline de agregación.

**`DbContext`** — Clase base de EF Core que representa la conexión a la base de datos. `AppDbContext` hereda de `DbContext`. Una instancia por request HTTP (Scoped).

**`DbSet<T>`** — Representa una tabla en EF Core. `DbSet<Producto> Productos` = tabla `Productos`. Se usa para hacer queries LINQ que se traducen a SQL.

**`EnsureCreated()`** — Crea la base de datos si no existe, aplicando el modelo actual. Usado para InMemory en producción (no migrations). En `Program.cs`: `db.Database.EnsureCreated()`.

**`FindAsync()`** — Busca una entidad por su clave primaria. Primero revisa el caché del DbContext, luego va a la BD. Más eficiente que `FirstOrDefaultAsync(x => x.Id == id)` para PKs.

**`FirstOrDefaultAsync()`** — Devuelve el primer elemento que cumple la condición, o `null` si no hay ninguno. En `ProductoRepositorio`: `FirstOrDefaultAsync(p => p.Id == id)` con Include.

**`GroupBy()`** — Agrupa elementos por una clave. En MongoDB equivalent con `Aggregate().Group()`.

**`HasColumnType("decimal(18,2)")`** — Especifica el tipo de columna SQL. En `AppDbContext`: aplicado a `Precio`, `PrecioUnitario`, `Subtotal`, `Total` para garantizar 2 decimales.

**`HasData()`** — Define seed data en `OnModelCreating()`. EF Core la inserta en la primera migración. En `AppDbContext.AgregarDatosIniciales()`: 5 categorías, 12 productos, 1 usuario admin.

**`HasForeignKey<T>()`** — Define la FK de una relación en Fluent API. En `AppDbContext`: `HasForeignKey<Carrito>(c => c.UsuarioId)`.

**`HasIndex()` / `IsUnique()`** — Crea un índice en la columna. `HasIndex(u => u.Email).IsUnique()` garantiza emails únicos y acelera búsquedas por email. `HasIndex(p => p.Nombre)` para búsquedas LIKE.

**`HasKey()`** — Define la clave primaria de una entidad en Fluent API. En `AppDbContext`: `HasKey(c => c.Id)`.

**`HasMaxLength()`** — Limita la longitud máxima de una columna string. Genera `NVARCHAR(200)` en SQL Server.

**`HasOne()` / `WithMany()` / `WithOne()`** — Configura relaciones en Fluent API. `HasOne(p => p.Categoria).WithMany(c => c.Productos)` = Producto N:1 Categoria.

**`Include()`** — Carga relaciones (eager loading) con un JOIN. Sin Include, la propiedad de navegación es null. En `ProductoRepositorio`: `Include(p => p.Categoria)` genera `INNER JOIN Categorias`.

**`IsRequired()`** — Marca una columna como NOT NULL en la BD. En `AppDbContext`: `Property(p => p.Nombre).IsRequired()`.

**`OnDelete(DeleteBehavior)`** — Configura la acción en cascada al eliminar. `DeleteBehavior.Cascade` elimina hijos automáticamente. `DeleteBehavior.Restrict` previene eliminar si hay hijos.

**`OnModelCreating()`** — Método de `DbContext` donde se configura el modelo usando Fluent API (relaciones, índices, tipos, seed data). En `AppDbContext.cs`.

**`OrderBy()` / `OrderByDescending()`** — Ordena la colección ascendente o descendentemente. En `ProductoRepositorio`: `OrderByDescending(p => p.FechaCreacion)` para obtener los más nuevos primero.

**`Remove()`** — Marca una entidad como `Deleted` en el DbContext. EF genera `DELETE FROM` en el próximo `SaveChangesAsync()`. En `RepositorioBase<T>`.

**`SaveChangesAsync()`** — Persiste todos los cambios pendientes (Added/Modified/Deleted) en una sola transacción. Retorna el número de filas afectadas. En `UnidadDeTrabajo.GuardarCambiosAsync()`.

**`Select()`** — Proyecta cada elemento a un nuevo tipo. En `OrdenesController`: `.Select(o => new OrdenDto {...})` mapea entidades a DTOs.

**`Set<T>()`** — Obtiene el `DbSet<T>` de un tipo dado. En `RepositorioBase`: `_dbSet = contexto.Set<T>()`. Permite trabajar con cualquier entidad genéricamente.

**`Sum()`** — Suma todos los valores de una colección LINQ. En `Carrito.CalcularTotal()`: `Items.Sum(i => i.CalcularSubtotal())`.

**`ThenInclude()`** — Carga relaciones anidadas (eager loading de segundo nivel). En `OrdenRepositorio`: `.Include(o => o.Detalles).ThenInclude(d => d.Producto)` carga la cadena Orden → Detalles → Producto.

**`ToListAsync()`** — Materializa una query `IQueryable` a una lista en memoria, ejecutando el SQL. El punto donde EF envía la consulta a la BD. Equivale a `ToList()` pero asíncrono.

**`Update()`** — Marca una entidad como `Modified` en el DbContext. EF genera `UPDATE` en el próximo `SaveChangesAsync()`. En `RepositorioBase<T>`: `Actualizar()`.

**`UseInMemoryDatabase()`** — Configura EF Core para usar una base de datos en memoria (sin SQL Server). Usado en producción AWS (`UseInMemoryDatabase: true` en config). Ventaja: no requiere RDS.

**`UseSqlServer()`** — Configura EF Core para usar SQL Server con la connection string dada. Usado en desarrollo local.

**`Where()`** — Filtra elementos que cumplen una condición. En `ProductoRepositorio`: `.Where(p => p.Activo && p.Nombre.Contains(termino))`. EF Core lo traduce a `WHERE Activo = 1 AND Nombre LIKE '%termino%'`.

---

## 9. Métodos HTTP y códigos de estado

**200 OK** — Solicitud exitosa. La respuesta incluye el recurso o los datos solicitados. Devuelto por `Ok()` en los controladores. La respuesta más común de la API.

**201 Created** — Recurso creado exitosamente. Devuelto por `CreatedAtAction()` en `ProductosController.Crear()`. Incluye el header `Location` con la URL del nuevo recurso.

**204 No Content** — Solicitud exitosa sin cuerpo de respuesta. Típico en DELETE cuando no hay nada que retornar. No usado explícitamente en este proyecto (se devuelve 200 con `Resultado<bool>`).

**301 Redirect** — Redirección permanente. `UseHttpsRedirection()` redirige de HTTP a HTTPS con 301. Desactivado en producción en EB.

**400 Bad Request** — La solicitud tiene datos inválidos o faltantes. Devuelto por `BadRequest()`. En el proyecto: cuando la validación falla (stock insuficiente, nombre vacío, etc.).

**401 Unauthorized** — Sin autenticación o token inválido. Devuelto por `Unauthorized()` en `AuthController.Login()` cuando las credenciales son incorrectas. El interceptor de Axios redirige a `/login`.

**403 Forbidden** — Autenticado pero sin permisos. Devuelto por `Forbid()` en `OrdenesController.Detalle()` cuando el usuario intenta ver una orden que no es suya.

**404 Not Found** — Recurso no encontrado. Devuelto por `NotFound()`. En `ProductosController.ObtenerPorId()`: cuando el producto no existe en la BD.

**500 Internal Server Error** — Error inesperado del servidor. `ManejadorErroresMiddleware` captura excepciones no manejadas y las convierte en 500 con JSON estándar.

**BadRequest()** — Helper de ASP.NET Core que devuelve HTTP 400. En el proyecto siempre envuelve un `Resultado<T>.Error(mensaje)` como body.

**CreatedAtAction()** — Helper que devuelve HTTP 201 con el header `Location` apuntando al endpoint que obtiene el recurso creado. En `ProductosController.Crear()`.

**DELETE** — Verbo HTTP para eliminar un recurso. Mapeado con `[HttpDelete]`. En `CarritoController`: `DELETE /api/carrito/{productoId}` elimina un item.

**Forbid()** — Helper de ASP.NET Core que devuelve HTTP 403. En `OrdenesController` cuando el usuario no es el dueño de la orden.

**GET** — Verbo HTTP para leer/obtener recursos. Idempotente (múltiples llamadas igual resultado). Mapeado con `[HttpGet]`. En `ProductosController`: `GET /api/productos`, `GET /api/productos/{id}`.

**NotFound()** — Helper de ASP.NET Core que devuelve HTTP 404. En `ProductosController.ObtenerPorId()` cuando `_uow.Productos.ObtenerPorIdAsync(id)` retorna `null`.

**Ok()** — Helper de ASP.NET Core que devuelve HTTP 200. En el proyecto siempre envuelve un `Resultado<T>.Ok(datos)` como body.

**OPTIONS** — Verbo HTTP usado en las peticiones preflight de CORS. El navegador envía OPTIONS antes de la petición real para verificar si el servidor permite CORS. `UseCors()` debe ir antes de `UseAuthentication()` para que OPTIONS pase.

**PATCH** — Verbo HTTP para actualizar parcialmente un recurso. No implementado en este proyecto (se usa PUT para actualización completa).

**POST** — Verbo HTTP para crear recursos o enviar datos. Mapeado con `[HttpPost]`. En `AuthController`: `POST /api/auth/login`, `POST /api/auth/registrar`.

**PUT** — Verbo HTTP para actualizar completamente un recurso. Mapeado con `[HttpPut]`. En `ProductosController`: `PUT /api/productos/{id}` reemplaza todos los campos.

**Unauthorized()** — Helper de ASP.NET Core que devuelve HTTP 401. En `AuthController.Login()` cuando las credenciales son incorrectas.

---

## 10. Patrones de diseño y arquitectura

**Clean Architecture** — Arquitectura que organiza el código en capas concéntricas con dependencias hacia adentro. En el proyecto: `Core` (sin deps) → `Data` (depende de Core) → `API` (depende de Data). El negocio nunca depende de frameworks.

**Code First** — Estrategia de EF Core donde se definen las clases C# primero y EF genera el SQL. El proyecto usa Code First con Fluent API en `AppDbContext.OnModelCreating()` y migraciones.

**Composition (Composition API)** — Patrón de Vue 3 que organiza la lógica por funcionalidad usando funciones composables (`ref`, `computed`, `onMounted`) en vez de opciones del componente.

**Database First** — Estrategia de EF Core donde existe la BD primero y EF genera las clases. No usada en este proyecto (se usa Code First).

**DDD (Domain-Driven Design)** — La lógica de negocio vive en las entidades del dominio. `Producto.ReducirStock()` valida y lanza excepción. `Carrito.AgregarProducto()` maneja toda la lógica del carrito. El servicio solo orquesta.

**Dependency Injection (DI)** — Las dependencias se inyectan en el constructor en vez de instanciarse dentro de la clase. En `CarritoServicio`: `private readonly IUnidadDeTrabajo _uow;` inyectado por constructor.

**DTO (Data Transfer Object)** — Clase simple para transferir datos entre capas sin lógica. `ProductoDto`, `CarritoDto`, `OrdenDto`. Evita exponer entidades de BD y serialización circular.

**Eager Loading** — Cargar relaciones junto con la entidad principal en una sola query (con JOIN). En `ProductoRepositorio`: `Include(p => p.Categoria)` = Eager Loading. Opuesto a Lazy Loading.

**Factory** — Patrón que delega la creación de objetos. Los métodos estáticos de `Resultado<T>`: `Resultado<T>.Ok(datos)` y `Resultado<T>.Error(mensaje)` actúan como Factory Methods.

**Fire-and-forget** — Patrón donde se lanza una operación asíncrona sin esperar su resultado. En `ProductosController.Buscar()`: `_ = _historial.RegistrarBusquedaAsync(...)` — no bloquea la respuesta si MongoDB falla.

**Guard** — Navigation guard en Vue Router. En `router/index.js`: `router.beforeEach()` verifica autenticación y rol antes de navegar. Redirige a `/login` si no está autenticado.

**Interceptor** — Función que intercepta requests/responses antes de que lleguen al destino. En `api.js`: interceptor de request agrega JWT; interceptor de response maneja 401.

**Lazy Initialization** — Crear un objeto solo cuando se necesita por primera vez. En `UnidadDeTrabajo.cs`: `_productos ??= new ProductoRepositorio(_contexto)` — el repositorio se crea solo al acceder a `Productos`.

**Lazy Loading** — Cargar relaciones a demanda (cuando se acceden por primera vez). EF Core tiene lazy loading desactivado por defecto. Sin `Include()`, `producto.Categoria` sería `null`.

**Middleware** — Componente en el pipeline HTTP que procesa requests y puede pasar al siguiente o cortocircuitar. `ManejadorErroresMiddleware` captura excepciones. El orden en `Program.cs` es crítico.

**MVVM** — Model-View-ViewModel. Las stores de Pinia son el ViewModel: conectan el modelo (API) con la vista (componentes Vue). Computed properties son getters del ViewModel.

**MVC** — Model-View-Controller. Los controladores de ASP.NET Core son la C. En una API pura (sin Razor) no hay Vista. El Modelo son las entidades y DTOs.

**Observer** — Patrón donde múltiples observadores reaccionan a cambios en un sujeto. Vue.js usa Observer internamente para detectar cambios en `ref()` y `reactive()` y actualizar el DOM.

**Repository** — Abstrae el acceso a datos detrás de una interfaz. `IRepositorio<T>` define el contrato CRUD. `ProductoRepositorio` implementa queries especializados. El servicio no sabe si los datos vienen de SQL, InMemory o MongoDB.

**Singleton** — Una sola instancia de una clase durante toda la vida de la aplicación. `HistorialBusquedaServicio` es Singleton en DI porque `MongoClient` gestiona su propio pool de conexiones.

**Soft Delete** — Marcar un registro como inactivo en vez de eliminarlo de la BD. En `CategoriasController.Desactivar()`: `categoria.Activa = false` en vez de `_uow.Categorias.Eliminar(categoria)`.

**Unit of Work** — Agrupa múltiples operaciones en una sola transacción. `UnidadDeTrabajo` contiene todos los repositorios y el método `GuardarCambiosAsync()`. Si falla algo, nada se persiste.

---

## 11. Instrucciones de Dockerfile

**`ARG`** — Define un argumento de build (solo disponible durante la construcción). No usado en el Dockerfile del proyecto, pero disponible en multi-stage builds.

**`AS`** — Nombra una etapa en multi-stage build. En el `Dockerfile`: `FROM sdk:10.0 AS build` nombra la etapa de compilación. La segunda etapa copia de `--from=build`.

**`CMD`** — Comando por defecto al iniciar el contenedor. A diferencia de `ENTRYPOINT`, puede ser sobreescrito. El proyecto usa `ENTRYPOINT` en vez de `CMD`.

**`COPY`** — Copia archivos del contexto de build al contenedor. En el Dockerfile: `COPY EcommerceNet.slnx .` y `COPY src/...csproj src/...` (primero solo los csproj para cachear el restore).

**`ENTRYPOINT`** — Comando que se ejecuta cuando el contenedor inicia. En el Dockerfile: `ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]`. No puede ser sobreescrito fácilmente (a diferencia de `CMD`).

**`ENV`** — Define una variable de entorno en el contenedor. En el Dockerfile: `ENV ASPNETCORE_URLS=http://+:80` y `ENV ASPNETCORE_ENVIRONMENT=Production`.

**`EXPOSE`** — Documenta que el contenedor escucha en un puerto (solo documentación, no publica el puerto). En el Dockerfile: `EXPOSE 80`. El puerto real se mapea con `docker run -p 5000:80`.

**`FROM`** — Define la imagen base. En el Dockerfile: `FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build` (etapa 1) y `FROM mcr.microsoft.com/dotnet/aspnet:10.0` (etapa 2, solo runtime).

**`LABEL`** — Agrega metadatos a la imagen (autor, versión, descripción). No usado en el Dockerfile del proyecto.

**`RUN`** — Ejecuta un comando durante el build del contenedor. En el Dockerfile: `RUN dotnet restore` y `RUN dotnet publish src/EcommerceNet.API -c Release -o /publish`.

**`VOLUME`** — Declara un punto de montaje para datos persistentes. En `docker-compose.yml`: `volumes: sqlserver-data:/var/opt/mssql` para persistir la BD entre reinicios.

**`WORKDIR`** — Establece el directorio de trabajo dentro del contenedor. Todos los `COPY`, `RUN` posteriores se ejecutan desde este directorio. En el Dockerfile: `WORKDIR /app`.

---

## 12. Tipos de datos y palabras reservadas de C#

**`abstract`** — Clase o método que no tiene implementación y debe ser implementado por subclases. No usado directamente en el proyecto, pero `IRepositorio<T>` y `RepositorioBase<T>` siguen ese principio con interfaces.

**`async`** — Marca un método como asíncrono. Permite usar `await` dentro de él. Todos los métodos de acceso a datos en el proyecto son `async`. Ejemplo: `public async Task<Resultado<CarritoDto>> ObtenerCarritoAsync(...)`.

**`await`** — Suspende la ejecución del método async hasta que el Task se complete, sin bloquear el hilo. Ejemplo: `var carrito = await _uow.Carritos.ObtenerPorUsuarioAsync(usuarioId)`.

**`base`** — Referencia a la clase padre. En `ProductoRepositorio`: `base(contexto)` llama al constructor de `RepositorioBase`. En `AppDbContext`: `base.OnModelCreating(modelBuilder)`.

**`bool`** — Tipo primitivo: `true` o `false`. En entidades: `public bool Activo { get; set; } = true`. En DTOs: `public bool Exito { get; set; }`. En resultados: `Resultado<bool>.Ok(true, "Eliminado")`.

**`catch`** — Captura una excepción lanzada en el bloque `try`. En `CarritoServicio.ActualizarCantidadAsync()`: `catch (InvalidOperationException ex) { return Resultado<CarritoDto>.Error(ex.Message); }`.

**`class`** — Define una clase (tipo de referencia) en C#. Todas las entidades, DTOs, servicios y repositorios del proyecto son clases. `public class Producto { ... }`.

**`DateTime`** — Tipo para fecha y hora. En entidades: `public DateTime FechaCreacion { get; set; } = DateTime.UtcNow`. En `AppDbContext.AgregarDatosIniciales()`: `var fechaSeed = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)` (fija para evitar cambios en migraciones).

**`decimal`** — Tipo numérico exacto de 128 bits. Ideal para valores monetarios porque no tiene errores de punto flotante. Usado para `Precio`, `PrecioUnitario`, `Subtotal`, `Total` en todas las entidades.

**`double`** — Tipo numérico de punto flotante de 64 bits. NO se usa para dinero (errores de redondeo). El proyecto usa `decimal` para todos los valores monetarios.

**`else`** — Rama alternativa del `if`. En el proyecto: en controladores para verificar condiciones de error antes de procesar.

**`enum`** — Tipo que define un conjunto de constantes con nombre. En el proyecto: `EstadoOrden` (Pendiente, Pagada, EnPreparacion, Enviada, Entregada, Cancelada) y `RolUsuario` (Cliente, Admin).

**`finally`** — Bloque que siempre se ejecuta después de `try/catch`. En las stores de Pinia: `finally { cargando.value = false }` para siempre ocultar el spinner.

**`get` / `set`** — Accessors de propiedades. `{ get; set; }` = lectura y escritura pública. `private set` = solo la clase puede escribir. `init` = solo asignable en construcción.

**`if`** — Condicional básico. En el proyecto: validaciones en controladores y servicios. Ejemplo: `if (carrito == null || carrito.EstaVacio()) return Resultado<OrdenDto>.Error("El carrito está vacío")`.

**`init`** — Accessor que permite asignar solo durante inicialización (con `new` o constructor). No usado explícitamente en el proyecto, pero disponible en C# 9+.

**`int`** — Tipo entero de 32 bits (-2,147,483,648 a 2,147,483,647). Usado como PK en todas las entidades: `public int Id { get; set; }`. También para `Cantidad`, `Stock`, `UsuarioId`.

**`interface`** — Define un contrato sin implementación. En el proyecto: `IRepositorio<T>`, `IUnidadDeTrabajo`, `ICarritoServicio`, `IAuthServicio`. Permiten DI y desacoplamiento de capas.

**`internal`** — Modificador de acceso: visible solo dentro del mismo ensamblado. No usado directamente en el proyecto, pero `public` es el modificador predominante.

**`namespace`** — Agrupa clases relacionadas bajo un nombre. El proyecto usa file-scoped namespaces: `namespace EcommerceNet.Core.Entidades;` (sin llaves).

**`new`** — Crea una instancia de una clase. En C# 10+: `new()` infiere el tipo del contexto. En el proyecto: `Items = new()` equivale a `Items = new List<CarritoItem>()`.

**`null`** — Ausencia de valor para tipos de referencia. Propiedades de navegación son nullable: `public Categoria? Categoria { get; set; }`. El operador `?.` protege contra `NullReferenceException`.

**`override`** — Sobreescribe un método `virtual` de la clase padre con una implementación diferente. En `ProductoRepositorio`: `override ObtenerPorIdAsync()` agrega `Include(p => p.Categoria)`.

**`private`** — Solo accesible dentro de la misma clase. Los campos privados usan prefijo `_`: `private readonly IUnidadDeTrabajo _uow`. Los métodos de mapeo privados: `private static ProductoDto MapearADto(...)`.

**`protected`** — Accesible en la clase y sus subclases. En `RepositorioBase<T>`: `protected readonly AppDbContext _contexto` — accesible desde `ProductoRepositorio` y demás repositorios concretos.

**`public`** — Accesible desde cualquier lugar. Todas las propiedades de entidades y DTOs son `public`. Los endpoints de controladores son `public`.

**`readonly`** — El campo solo puede asignarse en el constructor o en la declaración. En `RepositorioBase`: `protected readonly AppDbContext _contexto` — no cambia después de la inyección.

**`record`** — Tipo de C# 9+ para datos inmutables con igualdad por valor. No usado en este proyecto pero disponible para DTOs simples.

**`required`** — Modificador de C# 11 que exige que la propiedad sea inicializada. No usado en este proyecto.

**`return`** — Retorna un valor de un método. En métodos async: `return Resultado<T>.Ok(datos)`. En expresiones: `return await _dbSet.ToListAsync()`.

**`sealed`** — Previene que una clase sea heredada o que un método sea sobreescrito. No usado en este proyecto.

**`static`** — Miembro que pertenece a la clase, no a las instancias. En `Resultado<T>`: `public static Resultado<T> Ok(...)` — se llama como `Resultado<T>.Ok(datos)` sin instanciar. En controladores: los métodos de mapeo `MapearADto` son `private static`.

**`string`** — Tipo de cadena de texto (inmutable) en C#. En entidades: `public string Nombre { get; set; } = string.Empty` para evitar `null` por defecto.

**`switch`** — Selecciona entre múltiples casos. En `ManejadorErroresMiddleware`: switch expression de C# 8+ para mapear tipo de excepción a código HTTP: `ex switch { InvalidOperationException => (400, msg), ... }`.

**`Task<T>`** — Representa una operación asíncrona que retornará un valor de tipo T. Todos los métodos async del proyecto retornan `Task<T>`. Ejemplo: `Task<Resultado<CarritoDto>>`.

**`this`** — Referencia a la instancia actual de la clase. No usado explícitamente en el proyecto (todos los campos tienen nombre distinto a los parámetros del constructor).

**`throw`** — Lanza una excepción. En `Producto.ReducirStock()`: `throw new InvalidOperationException($"Stock insuficiente para '{Nombre}'")`. En `ManejadorErroresMiddleware`: capturado como `ex`.

**`try`** — Bloque que puede lanzar excepciones capturadas por `catch`. En `CarritoServicio`: `try { carrito.ActualizarCantidad(...) } catch (InvalidOperationException ex) {...}`.

**`using`** — Importa un namespace (`using EcommerceNet.Core.DTOs;`) o libera recursos al salir del scope (`using var scope = app.Services.CreateScope()`).

**`var`** — Inferencia de tipo. El compilador deduce el tipo en tiempo de compilación. En el proyecto: `var jwtKey = builder.Configuration["Jwt:Key"]!`. Igual de type-safe que declarar el tipo explícitamente.

**`virtual`** — Marca un método como sobreescribible por subclases. En `RepositorioBase<T>`: `public virtual async Task<T?> ObtenerPorIdAsync(int id)` — permite que `ProductoRepositorio` lo sobreescriba con `Include()`.

**`void`** — Tipo de retorno cuando un método no retorna ningún valor. En el proyecto: los métodos `Eliminar()`, `Actualizar()` en los repositorios son `void` (sincrónicos, no retornan nada).

**`where`** — Restricción de tipo genérico. En `IRepositorio<T>`: `where T : class` restringe T a tipos de referencia (clases). En LINQ: `.Where(p => p.Activo)` filtra colecciones.

---

## 13. Nomenclaturas del proyecto EcommerceNet

**AppDbContext** — El DbContext del proyecto. Hereda de `DbContext` de EF Core. Contiene 7 DbSets (uno por entidad), toda la configuración Fluent API en `OnModelCreating()` y el seed data. Registrado como Scoped en DI.

**AuthRespuestaDto** — DTO de respuesta de login y registro. Contiene: `Token` (JWT), `Nombre`, `Email`, `Rol`, `Expira`. Lo que el frontend guarda en `localStorage` al autenticarse.

**AuthServicio** — Servicio de autenticación en `EcommerceNet.Data`. Implementa `IAuthServicio`. Hashea passwords con BCrypt, genera JWT con `JwtSecurityTokenHandler`. Tiene acceso directo a `AppDbContext` (no vía repositorios).

**BusquedaHistorial** — Entidad/documento de MongoDB. Representa una búsqueda realizada. Campos: `Id` (ObjectId), `Termino`, `UsuarioId` (nullable), `ResultadosEncontrados`, `Fecha`. Usa atributos `[BsonId]`, `[BsonRepresentation]`.

**Carrito** — Entidad del dominio. Relación 1:1 con Usuario. Contiene la lista de `CarritoItem`. Métodos de dominio: `AgregarProducto()`, `ActualizarCantidad()`, `EliminarProducto()`, `Vaciar()`, `CalcularTotal()`.

**CarritoItem** — Entidad del dominio. Un producto dentro del carrito. Guarda `PrecioUnitario` al momento de agregar (histórico). Método: `CalcularSubtotal()`. Relación con `Carrito` y `Producto`.

**CarritoRepositorio** — Repositorio concreto para `Carrito`. Implementa `ICarritoRepositorio`. Método especial: `ObtenerPorUsuarioAsync(int usuarioId)`.

**CarritoServicio** — Servicio de negocio del carrito. Orquesta el `IUnidadDeTrabajo`. Contiene la lógica del checkout: validar stock, crear orden, reducir stock, vaciar carrito, todo en una transacción.

**Categoria** — Entidad del dominio. Campos: `Id`, `Nombre`, `Descripcion`, `Activa`. Relación 1:N con `Producto`. Método: `TotalProductosActivos()`. Seed data: 5 categorías (Electrónica, Ropa, Hogar, Deportes, Libros).

**CategoriaRepositorio** — Repositorio concreto para `Categoria`. Método adicional: `ObtenerActivasAsync()` (solo categorías con `Activa = true`).

**EcommerceNet.API** — Capa 2. Controladores ASP.NET Core, middleware, configuración JWT y CORS, `Program.cs`. Depende de `EcommerceNet.Data` (y transitivamente de `Core`).

**EcommerceNet.Core** — Capa 0. Entidades de dominio, interfaces de repositorios y servicios, DTOs, enums. Sin dependencias externas (solo abstracciones). El corazón de la aplicación.

**EcommerceNet.Data** — Capa 1. EF Core, repositorios concretos, `AppDbContext`, `UnidadDeTrabajo`, `AuthServicio`, MongoDB. Depende de `EcommerceNet.Core`.

**EcommerceNet.Tests** — Proyecto de pruebas xUnit. Depende solo de `EcommerceNet.Core`. Prueba entidades sin BD ni mocks: 23 tests en `ProductoTests`, `CarritoTests`, `OrdenTests`.

**EcommerceNet.Web** — Proyecto frontend Vue.js 3. Independiente del backend — lo consume vía HTTP. Carpeta `src/EcommerceNet.Web/`. Compilado como SPA con Vite.

**EstadoOrden** — Enum con los estados de una orden: `Pendiente=0`, `Pagada=1`, `EnPreparacion=2`, `Enviada=3`, `Entregada=4`, `Cancelada=5`. La entidad `Orden` solo puede cancelarse si está en `Pendiente` o `Pagada`.

**HistorialBusquedaServicio** — Servicio MongoDB para guardar y consultar búsquedas. Registrado como Singleton. Métodos: `RegistrarBusquedaAsync()`, `ObtenerMasBuscadosAsync()` (con pipeline Aggregate), `ObtenerPorUsuarioAsync()`.

**IAuthServicio** — Interfaz del servicio de autenticación. Define `RegistrarAsync(RegistroDto)` y `LoginAsync(LoginDto)`. Implementado por `AuthServicio`. Permite cambiar la implementación sin modificar los controladores.

**ICarritoRepositorio** — Interfaz del repositorio de carritos. Define `ObtenerPorUsuarioAsync(int usuarioId)`, `AgregarAsync()`, `Actualizar()`.

**ICarritoServicio** — Interfaz del servicio de carrito. Define todas las operaciones: obtener, agregar, actualizar, eliminar, vaciar, checkout. Implementado por `CarritoServicio`.

**ICategoriaRepositorio** — Interfaz del repositorio de categorías. Hereda `IRepositorio<Categoria>` y agrega `ObtenerActivasAsync()`.

**IOrdenRepositorio** — Interfaz del repositorio de órdenes. Agrega `ObtenerPorUsuarioAsync()` y `ObtenerConDetallesAsync()`.

**IProductoRepositorio** — Interfaz del repositorio de productos. Agrega `BuscarPorNombreAsync()`, `ObtenerPorCategoriaAsync()`, `ObtenerConStockBajoAsync()`, `ObtenerActivosAsync()`.

**IRepositorio<T>** — Interfaz genérica CRUD. Define: `ObtenerPorIdAsync(int id)`, `ObtenerTodosAsync()`, `AgregarAsync(T)`, `Actualizar(T)`, `Eliminar(T)`. Constraint: `where T : class`.

**IUnidadDeTrabajo** — Interfaz del Unit of Work. Expone: `Productos`, `Carritos`, `Ordenes`, `Usuarios`, `Categorias`, `GuardarCambiosAsync()`. Hereda `IDisposable` para liberar el DbContext.

**ManejadorErroresMiddleware** — Middleware global de manejo de errores. Se registra primero en el pipeline. Captura cualquier excepción no manejada y devuelve JSON con `Resultado<string>.Error(mensaje)` y el código HTTP correspondiente.

**Orden** — Entidad del dominio. Registro permanente de una compra. Campos: `NumeroOrden` (formato `ORD-YYYYMMDD-XXXX`), `Estado`, `Total`, `DireccionEnvio`. Métodos: `GenerarNumeroOrden()`, `RecalcularTotal()`, `SePuedeCancelar()`, `Cancelar()`.

**OrdenDetalle** — Entidad del dominio. Una línea de la orden (producto + cantidad + precio). `PrecioUnitario` se fija al momento de la compra (histórico). Método: `CalcularSubtotal()`.

**OrdenRepositorio** — Repositorio concreto para `Orden`. Métodos: `ObtenerPorUsuarioAsync()` y `ObtenerConDetallesAsync()` (con `Include().ThenInclude()`).

**Producto** — Entidad central del dominio. Campos: `Nombre`, `Descripcion`, `Precio` (decimal), `Stock`, `ImagenUrl`, `Activo`, `CategoriaId`. Métodos de dominio: `TieneStockSuficiente()`, `ReducirStock()`, `AumentarStock()`.

**ProductoRepositorio** — Repositorio concreto para `Producto`. Hace `override` de `ObtenerPorIdAsync` para agregar `Include(p => p.Categoria)`. Agrega: `BuscarPorNombreAsync()`, `ObtenerPorCategoriaAsync()`, `ObtenerActivosAsync()`.

**RepositorioBase<T>** — Clase genérica que implementa `IRepositorio<T>` con EF Core. Métodos `virtual` para permitir override. Todos los repositorios concretos heredan de esta clase.

**Resultado<T>** — Clase genérica que envuelve todas las respuestas de la API. Campos: `Exito` (bool), `Datos` (T), `Mensaje` (string), `Errores` (List<string>). Factory methods: `Ok()`, `Error()`, `ErrorValidacion()`.

**RolUsuario** — Enum con los roles del sistema: `Cliente=0` (puede comprar) y `Admin=1` (puede administrar productos y categorías). Almacenado como `int` en SQL Server.

**UnidadDeTrabajo** — Implementa `IUnidadDeTrabajo`. Agrupa todos los repositorios usando lazy initialization con `??=`. Todos comparten el mismo `AppDbContext`. `GuardarCambiosAsync()` llama a `_contexto.SaveChangesAsync()`.

**Usuario** — Entidad del dominio. Campos: `Nombre`, `Email`, `PasswordHash` (BCrypt), `Rol` (enum), `FechaRegistro`. Relaciones: 1:1 con `Carrito`, 1:N con `Orden`. Método: `EsAdmin()`.

**UsuarioRepositorio** — Repositorio concreto para `Usuario`. Hereda CRUD de `RepositorioBase<T>`.

---

## 14. Términos de la industria tech y modelos de trabajo

**AI & Data Studio** — Tipo de equipo especializado en soluciones de inteligencia artificial, machine learning y análisis de datos. Incluye el GenAI Accelerator.

**AWS Migration Pod** — Equipo especializado de empresas tech que ayuda a empresas a migrar sus sistemas on-premise a AWS con arquitectura cloud moderna. Las empresas AWS Partner oficiales.

**Cloud & DevOps Studio** — Tipo de equipo enfocado en infraestructura cloud (AWS), DevOps, CI/CD y arquitectura de sistemas distribuidos.

**DaCoders** — Nombre coloquial para empleados de empresas que usan el modelo de Studios/Pods. Implica cultura de aprendizaje continuo y excelencia técnica.

**Discovery Sprint** — Metodología ágil para definir el alcance y la arquitectura de un producto antes de comenzar el desarrollo. Parte del proceso del Launch Pod.

**GenAI Accelerator** — Servicio/herramienta para acelerar la adopción de IA Generativa en empresas clientes. Parte del AI & Data Studio.

**Great Place to Work** — Certificación de cultura organizacional. Esta certificación — indicador de ambiente laboral positivo.

**ISO 27001** — Estándar internacional de seguridad de la información. Esta certificación implica procesos rigurosos de seguridad para proyectos de clientes.

**Launch Pod** — Modelo de equipo que construye MVPs y productos nuevos desde cero para startups y empresas. Fullstack development desde ideación hasta producción.

**Product Strategy & Design Studio** — Tipo de equipo enfocado en estrategia de producto, UX/UI design y discovery. Trabaja con los Pods para definir qué construir.

**Software Engineering & QA Studio** — Tipo de equipo enfocado en desarrollo de software de alta calidad, arquitectura y aseguramiento de calidad.

**Studios** — Forma de organizar equipos especializados en empresas tech modernas: Software Engineering & QA, Cloud & DevOps, AI & Data, Product Strategy & Design. Cada Studio tiene Pods.

---

## 15. Términos de Git y control de versiones

**`.gitignore`** — Archivo que lista patrones de archivos que Git no debe rastrear. En el proyecto: `bin/`, `obj/`, `.vs/`, `node_modules/`, `dist/`, `appsettings.Production.json`, `.aws/`, `*.pfx`.

**branch** — Rama del repositorio. El proyecto usa: `main` (producción), `desarrollo` (integración). Estrategia: feature branches del tipo `dia-01/fundamentos-csharp`.

**CHANGELOG** — Archivo que registra los cambios de versión a versión. No incluido en este proyecto, pero es buena práctica en proyectos open source.

**clone** — Copia un repositorio remoto al disco local, incluyendo todo el historial.

**commit** — Snapshot del estado del proyecto en un momento. El proyecto usa prefijos: `feat:` (nueva funcionalidad), `fix:` (corrección), `docs:` (documentación), `test:` (pruebas), `refactor:` (refactorización), `security:` (seguridad).

**desarrollo** — Rama de integración del proyecto. Donde se integran las features antes de ir a `main`.

**diff** — Muestra las diferencias entre versiones de archivos. `git diff` muestra cambios no staged. `git diff HEAD` muestra cambios vs último commit.

**feature branch** — Rama temporal para desarrollar una característica específica. Nomenclatura del proyecto: `dia-01/fundamentos-csharp`, `dia-02/api-rest`, etc.

**fork** — Copia de un repositorio en tu propia cuenta de GitHub. Permite contribuir a proyectos sin acceso directo al repositorio original.

**log** — Historial de commits. `git log --all --oneline` muestra todos los commits en formato compacto.

**main** — Rama principal del proyecto. Representa el estado de producción. Los commits a `main` disparan el pipeline de CI/CD.

**merge** — Fusiona los cambios de una rama con otra. `git merge desarrollo` en `main` integra las features probadas.

**origin** — Nombre por defecto del repositorio remoto (GitHub). `git push origin main` sube la rama `main` a GitHub.

**pull** — Descarga y fusiona cambios del repositorio remoto. `git pull origin main`.

**push** — Sube commits locales al repositorio remoto. `git push origin main`.

**README** — Archivo `README.md` que documenta el proyecto. En el proyecto: describe el stack, URLs de producción, arquitectura y cómo ejecutar localmente.

**remote** — Repositorio remoto. `origin` apunta a `github.com/Ramiro671/EcommerceNet`.

**stash** — Guarda cambios temporalmente sin commitear. `git stash` guarda, `git stash pop` recupera.

**tag** — Etiqueta en un commit específico para marcar versiones. `git tag v1.0.0` crea un tag.

---

## 16. Archivos de configuración y especiales

**`.csproj`** — Archivo XML de proyecto de .NET. Define el `TargetFramework`, paquetes NuGet, referencias a otros proyectos. En el proyecto: `EcommerceNet.API.csproj`, `EcommerceNet.Core.csproj`, etc.

**`.dockerignore`** — Archivo que excluye archivos del contexto de build de Docker (como `.gitignore` pero para Docker). No presente explícitamente en el proyecto — `.ebignore` cumple función similar para EB.

**`.ebignore`** — Archivo que indica a EB qué archivos excluir del ZIP de deploy. Toma precedencia sobre `.gitignore`. En el proyecto: excluye `docker-compose.yml` para que EB use el `Dockerfile` raíz directamente.

**`.slnx`** — Formato de archivo de solución de .NET 10+ (reemplaza al `.sln`). Más limpio y legible. `EcommerceNet.slnx` es la solución del proyecto. El Dockerfile copia `EcommerceNet.slnx`.

**`.sln`** — Formato legacy de archivo de solución de Visual Studio (XML). .NET Framework y versiones anteriores de .NET Core lo usaban. `.NET 10` crea `.slnx` por defecto.

**`appsettings.Development.json`** — Configuración para el entorno de desarrollo. En el proyecto: solo contiene overrides de logging. Rastreado en git porque no tiene secretos. ASPNETCORE_ENVIRONMENT=Development lo activa.

**`appsettings.json`** — Configuración base de ASP.NET Core. En el proyecto: JWT key de desarrollo, ConnectionString a LocalDB, configuración MongoDB. No excluido de git (contiene key de desarrollo no crítica).

**`appsettings.Production.json`** — Configuración para producción. En el proyecto: después de la auditoría de seguridad, tiene un placeholder para la JWT key. Excluido de git con `.gitignore`. La key real se configura con `eb setenv`.

**`bucket-policy.json`** — Política de acceso al bucket S3. Permite lectura pública (`s3:GetObject`) para servir el frontend. Se aplica con `aws s3api put-bucket-policy --policy file://bucket-policy.json`.

**`ci-cd.yml`** — Pipeline de GitHub Actions en `.github/workflows/`. Dos jobs paralelos: `backend` (dotnet build/test/publish) y `frontend` (npm ci/build). Se dispara en push a `main` y `desarrollo`.

**`CLAUDE.md`** — Archivo de instrucciones para Claude Code (Anthropic). Define la arquitectura del proyecto, reglas de código, stack técnico, endpoints de la API. Leído por Claude Code en cada sesión.

**`docker-compose.yml`** — Orquesta múltiples contenedores para desarrollo local: API (.NET) + SQL Server + MongoDB. Define puertos, variables de entorno, volúmenes y dependencias entre servicios.

**`Dockerfile`** — Instrucciones para construir la imagen Docker de la API. El de la raíz es el que usa EB (multi-stage: `sdk:10.0` para compilar, `aspnet:10.0` para ejecutar). El de `src/EcommerceNet.API/` es para desarrollo local.

**`Dockerrun.aws.json`** — Archivo de configuración alternativo de EB para Docker. No usado en este proyecto (EB detecta el Dockerfile directamente gracias al `.ebignore`).

**`package.json`** — Archivo de configuración de Node.js/npm. Define dependencias (`axios`, `pinia`, `vue`, `vue-router`), devDependencies (`vite`, `eslint`, `prettier`) y scripts (`dev`, `build`, `lint`).

**`Program.cs`** — Punto de entrada de la aplicación ASP.NET Core. Configura todos los servicios (DI) en la primera mitad y el pipeline de middleware en la segunda. Reemplaza `Startup.cs` y `Global.asax` de versiones anteriores.

**`requests.http`** — Archivo con requests HTTP para probar la API directamente desde VS Code (extensión REST Client). No presente en el proyecto, pero es herramienta común en proyectos .NET.

---

## 17. Términos adicionales de seguridad y arquitectura

**Atomicidad** — Propiedad ACID que garantiza que una transacción se ejecuta completa o no se ejecuta. En `CarritoServicio.CheckoutAsync()`: crear orden + reducir stock + vaciar carrito es una sola transacción con `SaveChangesAsync()`.

**Bearer token** — Esquema de autenticación HTTP donde el token (JWT) se envía en el header `Authorization: Bearer <token>`. Configurado en `Program.cs` con `JwtBearerDefaults.AuthenticationScheme`.

**Claim** — Dato incrustado en un token JWT que describe al usuario. En `AuthServicio.cs`: `ClaimTypes.NameIdentifier` (ID), `ClaimTypes.Name` (nombre), `ClaimTypes.Email`, `ClaimTypes.Role`. Se leen en controladores con `User.FindFirst(ClaimTypes.NameIdentifier)`.

**Connection pool** — Conjunto de conexiones a la BD reutilizadas para evitar el costo de crear una nueva conexión por cada request. `MongoClient` gestiona su propio pool (razón para registrarlo como Singleton).

**Debounce** — Técnica para retrasar la ejecución de una función hasta que el usuario deja de invocarla. Útil en búsquedas en tiempo real para no hacer un API call por cada tecla. No implementado explícitamente en el proyecto.

**Hash** — Resultado de aplicar una función hash a un dato. Los passwords se almacenan como hash BCrypt en `PasswordHash` de `Usuario`. Un hash es irreversible (no se puede obtener el password original).

**Idempotente** — Operación que produce el mismo resultado sin importar cuántas veces se ejecute. GET es idempotente (leer un producto N veces da el mismo resultado). POST no lo es (crear N veces crea N recursos).

**Issuer** — Quien emitió el token JWT. En `appsettings.json`: `"Issuer": "EcommerceNet.API"`. El middleware de autenticación valida que el token fue emitido por el Issuer esperado.

**Audience** — Para quién está destinado el token JWT. En `appsettings.json`: `"Audience": "EcommerceNet.Web"`. El middleware valida que el token fue creado para el Audience correcto.

**Middleware pipeline** — Cadena de componentes que procesan cada request HTTP en orden. En `Program.cs`: ManejadorErrores → Swagger → CORS → UseAuthentication → UseAuthorization → MapControllers. Cada middleware puede pasar al siguiente o cortocircuitar.

**Migration** — Archivo C# generado por EF Core que describe cambios al esquema de BD. Tiene `Up()` (aplicar cambio) y `Down()` (revertir). Creada con `dotnet ef migrations add NombreMigracion`.

**Pool de conexiones** — Ver "Connection pool". EF Core gestiona el pool de conexiones a SQL Server automáticamente.

**Salt** — Valor aleatorio añadido al password antes de hashear para que dos passwords iguales tengan hashes diferentes. BCrypt incluye el salt automáticamente en el hash resultante.

**Schema** — Esquema de base de datos: definición de tablas, columnas, tipos y restricciones. En Code First, el schema se define en las clases C# y se genera vía migraciones.

**Seed data** — Datos iniciales insertados en la BD cuando se crea. En `AppDbContext.AgregarDatosIniciales()`: 5 categorías, 12 productos, 1 usuario admin. Se usan IDs fijos para evitar conflictos en migraciones.

**Singleton** — Patrón que garantiza una sola instancia de una clase. En DI: `AddSingleton<HistorialBusquedaServicio>()`. `MongoClient` es thread-safe y fue diseñado para ser singleton.

**Soft delete** — Borrado lógico: marcar un registro como inactivo (`Activa = false`) en vez de eliminarlo. En `CategoriasController.Desactivar()`. Preserva el historial y la integridad referencial.

**Stateless** — Sin estado del lado del servidor entre requests. JWT es stateless: toda la información del usuario está en el token, el servidor no guarda sesión. Permite escalar horizontalmente sin sincronizar sesiones.

**Thread-safe** — Diseñado para funcionar correctamente cuando múltiples hilos lo usan simultáneamente. `MongoClient` es thread-safe (razón para ser Singleton). `DbContext` NO es thread-safe (razón para ser Scoped).

**Transacción** — Conjunto de operaciones de BD que se ejecutan todas o ninguna (atomicidad). EF Core inicia una transacción implícita en cada `SaveChangesAsync()`. Si falla, hace rollback automático.

**Virtual DOM** — Representación en memoria del DOM real que Vue.js mantiene. Compara el Virtual DOM anterior con el nuevo (diffing) y actualiza solo los nodos que cambiaron, haciendo la UI eficiente.

**Work factor** — Parámetro de BCrypt que define cuántas iteraciones se realizan al hashear. El admin hash del proyecto usa `$2a$11$...` — work factor 11. Mayor work factor = más lento = más seguro contra fuerza bruta.

---

*Total de entradas: ver conteo con `findstr /c:"**" docs\diccionario-tecnico.md`*
