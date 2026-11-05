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

Requiere PostgreSQL local y una cadena compatible en `src/Web/appsettings.Development.json`:

```powershell
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
