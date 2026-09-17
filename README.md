# Wedding Planner

Plantilla monolítica para planificar bodas con .NET 10, ASP.NET Core MVC y vistas Razor. La aplicación web contiene la interfaz, autenticación y acceso a datos en el mismo proceso. No incluye Angular ni un proyecto de API.

## Estructura

```text
src/
  Core/            Entidades y reglas de dominio
  Application/     Casos de uso, DTOs y contratos
  Infrastructure/ DbContext, Identity y persistencia EF Core
  Web/             Aplicación ASP.NET Core MVC + Razor
tests/
  Unit/            Pruebas unitarias del dominio
```

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
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Web --output-dir Data/Migrations
dotnet ef database update --project src/Infrastructure --startup-project src/Web
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
