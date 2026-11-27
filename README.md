# Wedding Planner

Plantilla monolítica para planificar eventos con .NET 10, ASP.NET Core MVC y vistas Razor. La aplicación web contiene la interfaz, autenticación y acceso a datos en el mismo proceso. No incluye Angular ni un proyecto de API.

## Estructura

```text
src/
  Core/            Entidades y reglas de dominio
  Application/     Casos de uso, DTOs y contratos
  Infrastructure/ DbContext, Identity y persistencia EF Core
  Web/             Aplicación ASP.NET Core MVC + Razor
tests/
  Unit/            Pruebas unitarias de casos de uso y lógica de aplicación
```

## Contexto rápido para nuevas sesiones

Usa este archivo como punto de partida y consulta después solo los archivos de la parte que vayas a cambiar. Es un mapa del proyecto, no sustituye al código actual: verifica la implementación relevante antes de editar.

- `src/Web/Program.cs`: composición de dependencias y arranque HTTP; aplica las migraciones al iniciar.
- `src/Web/Controllers/`, `Views/` y `Configuration/`: interfaz MVC/Razor y configuración web.
- `src/Application/UseCases/`: handlers de casos de uso; `Abstractions/` define contratos y `Implementations/` contiene servicios de aplicación.
- `src/Core/Domain/`: entidades y enums del dominio, sin dependencias de infraestructura.
- `src/Infrastructure/Data/`: `ApplicationDbContext` y migraciones EF Core; `Repositories/` implementa persistencia.
- `src/Application/DependencyInjection.cs` y `src/Infrastructure/DependencyInjection.cs`: registro de servicios de cada capa.
- `tests/Unit/`: pruebas unitarias; revisa los tests del handler o servicio que modifiques.

### Flujo y límites entre capas

`Web` compone y presenta; `Application` coordina casos de uso y contratos; `Core` contiene el dominio; `Infrastructure` implementa persistencia y servicios externos. Mantén las dependencias apuntando hacia el dominio y la aplicación: los casos de uso no deben depender de EF Core ni de ASP.NET Core. La persistencia usa EF Core con PostgreSQL y la autenticación local usa ASP.NET Core Identity.

Al retomar trabajo, revisa también el estado actual del repositorio antes de cambiar archivos: puede haber cambios locales que deban conservarse. Para un cambio concreto, busca primero sus usos y pruebas relacionados en vez de leer todo el proyecto.

## Requisitos

- .NET SDK 10.0.0 o una versión compatible según `global.json`.
- Docker Desktop con Docker Compose.
- Copiar `.env.example` a `.env` y completar sus valores. El `.env` real no se versiona.

## Ejecutar con Docker

```powershell
docker compose up --build
```

La aplicación Razor queda disponible en `http://localhost:${WEB_PORT}` y PostgreSQL en el puerto `${DB_PORT}`. Las migraciones se aplican automáticamente al arrancar el contenedor web.

Para iniciar también pgAdmin:

```powershell
docker compose --profile tools up --build
```

## Ejecutar sin Docker

Requiere PostgreSQL accesible desde la máquina y un `.env` en la raíz del repositorio. La aplicación carga sus valores `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER` y `DB_PASSWORD` al iniciar:

```powershell
if (-not (Test-Path .env)) { Copy-Item .env.example .env }
dotnet run --project src/Web
```

Para crear migraciones manualmente:

```powershell
dotnet tool install --global dotnet-ef
cd src/Web
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Las cuentas locales actuales usan ASP.NET Core Identity con cookies.

## Autenticación externa

La configuración usa `Authentication:Provider`, con valores `None`, `Firebase` o `Supabase`. Los valores se inyectan desde `.env` mediante Docker y ya están enlazados a `ExternalAuthenticationOptions`. La validación de tokens y el alta de usuarios se implementarán detrás de esta configuración cuando se elija el proveedor definitivo; no se guardan claves privadas ni tokens en el repositorio.

## Documentos y RustFS

Los archivos nuevos usan `IFileStorageService` (Application) y `S3FileStorageService`
(Infrastructure, AWS SDK); PostgreSQL guarda únicamente metadatos y una clave aleatoria.
Los enlaces `FileUrl` anteriores siguen funcionando como enlaces externos, con el mismo
acceso previo; no se importan ni se protegen automáticamente. Los archivos nuevos
requieren una contratación del proveedor en la boda y una sesión Identity cuyo ID
coincida con `Wedding.CoordinatorId`. Actualmente la interfaz no asigna coordinadores:
una boda sin coordinador no puede recibir archivos hasta que un administrador asigne
uno mediante un procedimiento externo autorizado. No se modificó la autenticación.

Para desarrollo local:

1. Copia `.env.example` a `.env` si todavía no existe y configura valores únicos para
   `RUSTFS_ACCESS_KEY` y `RUSTFS_SECRET_KEY`. No uses las credenciales de ejemplo
   conocidas de RustFS ni publiques `.env`.
2. Ejecuta `docker compose -f docker-compose.yml -f docker-compose.rustfs.yml up -d --build`.
   RustFS expone S3 y la consola solo en localhost, en los puertos configurados;
   los datos se conservan en el volumen `rustfs_data`.
3. Crea el bucket privado mediante la consola (`http://localhost:9001`) o AWS CLI:
   configura sus credenciales y región localmente y ejecuta
   `aws s3 mb s3://wedding-documents --endpoint-url http://localhost:9000`.
   Ajusta el nombre y puerto si cambiaste `RUSTFS_BUCKET` o `RUSTFS_PORT`.
4. Verifica el servicio con `curl.exe --fail http://localhost:9000/health`.
   Compose comprueba ese endpoint y espera a que RustFS esté saludable antes
   de iniciar Web.

Para ejecutar Web fuera de Docker, configura `FileStorage__Endpoint` (por ejemplo
`http://localhost:9000`), `FileStorage__Bucket`, `FileStorage__Region`,
`FileStorage__AccessKey`, `FileStorage__SecretKey`, `FileStorage__UseSSL` y
`FileStorage__ForcePathStyle` como variables de entorno o User Secrets. Usa HTTPS
fuera de desarrollo. Cambiar endpoint, región, credenciales, SSL y path-style permite
usar otro proveedor compatible con S3; el bucket debe existir y permanecer privado.
`FileStorage:MaxFileSizeBytes` limita cada archivo a 10 MiB por defecto y debe estar
entre 1 y 10 485 760 bytes: valores mayores impiden el inicio. En la creación de
documentos, Kestrel limita la solicitud completa a 11 MiB para incluir el overhead
multiparte y cada sección a 10 MiB; solo se acepta un archivo. El servicio conserva
su propia validación y puede aplicar un límite configurado menor. Una solicitud
excesiva puede recibir HTTP 413, HTTP 400 si falla el procesamiento del formulario
o un cierre de conexión; el resultado depende de cuándo el servidor detecta el
exceso. El resto de los endpoints no recibe estos límites específicos.

`FileStorage:AllowedExtensions` permite restringir PDF, JPG y PNG. El servidor
comprueba nombre, extensión, MIME y estructura: cabecera, `startxref` y terminador
para PDF; chunks IHDR, IDAT e IEND para PNG; marcadores, dimensiones y segmentos
de imagen para JPEG. Los PDF deben terminar con `%%EOF` y tener `startxref` en los
últimos 2048 bytes; los JPEG deben terminar en el marcador EOI. Estas comprobaciones
no decodifican imágenes ni verifican CRC, integridad semántica completa o malware:
algunos archivos inválidos pueden pasar y algunos archivos válidos con relleno
atípico pueden rechazarse. No hay antivirus integrado; para producción considera
un escáner independiente antes de incorporar archivos al almacenamiento definitivo.
El selector del navegador es solo una ayuda; ampliar la lista requiere implementar
la validación del nuevo formato.

Las descargas pasan por el servidor, verifican el coordinador y se entregan como
adjuntos sin URL pública. El stream de descarga libera también la respuesta S3
cuando el consumidor termina o falla la lectura. Los listados filtran por proveedor,
asignación y coordinador en PostgreSQL, sin seguimiento EF ni consultas por documento;
los enlaces externos conservan su acceso anterior. Si falla el registro después de subir, se intenta borrar el
objeto; si la compensación falla, se registra la clave para su limpieza manual.
La eliminación primero marca el documento como pendiente en la base de datos; si
falla S3, vuelve a pulsar Eliminar para reintentar. No se puede borrar un proveedor
con documentos vinculados: elimina primero sus documentos. La migración
`AddVendorDocumentStorage` conserva los enlaces y no admite reversión automática
porque descartaría referencias a objetos; realiza una copia antes de aplicarla.

En producción, usa HTTPS, credenciales secretas y un bucket privado con permisos
mínimos. Configura cualquier proxy inverso, IIS o balanceador para aceptar al menos
11 MiB en esta ruta; un límite inferior impediría subir archivos válidos. Kestrel no
tiene un límite global personalizado en este proyecto. Establece límites y tiempos
de espera adecuados en el proxy y supervisa los errores de almacenamiento y los
documentos pendientes de eliminación.

## Validación

```powershell
dotnet build WeddingPlanner.slnx
dotnet test WeddingPlanner.slnx
```

## Decisiones de esta plantilla

- `Web` es el único ejecutable y no publica controladores API.
- `Core` contiene el modelo de negocio y no conoce ninguna capa externa.
- `Application` contiene casos de uso y contratos; no conoce EF Core ni ASP.NET Core.
- `Infrastructure` implementa los contratos de persistencia y expone `AddInfrastructure`.
- `Web` compone las dependencias y solo consume casos de uso.
- Identity se integra en el monolito mediante `AddDefaultIdentity` y Razor Pages.
- PostgreSQL se ejecuta como servicio `postgres` y la aplicación se conecta usando el nombre DNS interno `postgres`.
