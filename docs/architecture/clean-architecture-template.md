# .NET Clean Architecture — plantilla de arranque

> **Qué es esto.** Plantilla de referencia para arrancar **otro repositorio** con la misma arquitectura que Aurora Coinly. No es una guía de trabajo para este repo: la solución ya está scaffoldeada, y para trabajar sobre ella se usan las skills `domain-entity`, `create-feature` y `unit-testing`.
>
> Usa los nombres y decisiones de Coinly. Si arrancas un servicio nuevo, empieza por este documento y después reconstruye las convenciones leyendo `src/` y `.claude/rules/`.

Clean Architecture (también Onion o Hexagonal): separación por capas con dependencias unidireccionales hacia dentro.

## Capas

| Capa | Alcance |
|------|---------|
| Domain | Entidades, value objects, domain events, Result |
| Application | Commands, queries, handlers, validators, models, behaviors, handlers de eventos |
| Infrastructure | EF Core, outbox + Quartz, autenticación, cifrado, servicios externos, cross-cutting |
| Api | Minimal APIs, middlewares, formato de respuesta |

**Regla de dependencia:** las dependencias apuntan hacia dentro. Domain no depende de nada. Application depende solo de Domain. Infrastructure implementa las interfaces declaradas en Application. Api depende de Infrastructure.

## Estructura de proyectos

```
{SolutionName}/
├── src/
│   ├── {name}.Domain/
│   │   ├── Abstractions/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── BaseError.cs
│   │   │   ├── BaseErrorType.cs
│   │   │   ├── IDomainEvent.cs       # IDomainEvent + DomainEvent
│   │   │   ├── Result.cs
│   │   │   └── SerializerSettings.cs # Json.NET para el outbox
│   │   ├── {Aggregate}/
│   │   │   ├── {Entity}.cs
│   │   │   ├── {Entity}Errors.cs
│   │   │   └── {Entity}DomainEvents.cs
│   │   ├── Shared/
│   │   │   └── {ValueObject}.cs
│   │   └── {name}.Domain.csproj
│   │
│   ├── {name}.Application/
│   │   ├── Abstractions/
│   │   │   ├── Authentication/       # IUserContext, ITokenProvider
│   │   │   ├── Behaviors/
│   │   │   │   ├── LoggingBehavior.cs
│   │   │   │   └── ValidationBehavior.cs
│   │   │   ├── Data/                 # I{Name}DbContext, I{Name}DbContextFactory
│   │   │   ├── Exceptions/
│   │   │   ├── Messaging/
│   │   │   │   ├── ICommand.cs
│   │   │   │   ├── ICommandHandler.cs
│   │   │   │   ├── IDomainEventHandler.cs
│   │   │   │   ├── IQuery.cs
│   │   │   │   └── IQueryHandler.cs
│   │   │   ├── Time/
│   │   │   │   └── IDateTimeService.cs
│   │   │   └── Validations/
│   │   │       ├── RuleBuilderOptionsExtensions.cs
│   │   │       └── ValidationError.cs
│   │   ├── {Feature}/
│   │   │   ├── {Entity}Model.cs
│   │   │   ├── Create/
│   │   │   ├── Update/
│   │   │   ├── GetById/
│   │   │   └── GetList/
│   │   ├── DependencyInjection.cs
│   │   ├── GlobalUsings.cs
│   │   └── {name}.Application.csproj
│   │
│   ├── {name}.Infrastructure/
│   │   ├── Authentication/
│   │   ├── Configurations/
│   │   │   └── {Entity}Configuration.cs
│   │   ├── Database/
│   │   │   ├── Migrations/
│   │   │   └── ApplicationDbContext.cs
│   │   ├── DomainEvents/
│   │   ├── Interceptors/
│   │   │   └── InsertOutboxMessagesInterceptor.cs
│   │   ├── Outbox/
│   │   ├── Time/
│   │   ├── DependencyInjection.cs
│   │   ├── GlobalUsings.cs
│   │   └── {name}.Infrastructure.csproj
│   │
│   └── {name}.Api/
│       ├── Endpoints/
│       │   └── {Feature}/
│       ├── Extensions/
│       ├── Middlewares/
│       ├── Responses/
│       ├── Program.cs
│       ├── DependencyInjection.cs
│       ├── GlobalUsings.cs
│       ├── appsettings.json
│       └── {name}.Api.csproj
│
├── test/
│   ├── {name}.Domain.UnitTests/
│   └── {name}.ArchitectureTests/
│
├── Directory.Build.props
├── Directory.Packages.props
└── {SolutionName}.slnx
```

## Crear solución y proyectos

```bash
dotnet new sln -n {SolutionName}

dotnet new classlib -n {name}.Domain -o src/{name}.Domain
dotnet new classlib -n {name}.Application -o src/{name}.Application
dotnet new classlib -n {name}.Infrastructure -o src/{name}.Infrastructure
dotnet new webapi -n {name}.Api -o src/{name}.Api

dotnet sln add src/{name}.Domain/{name}.Domain.csproj
dotnet sln add src/{name}.Application/{name}.Application.csproj
dotnet sln add src/{name}.Infrastructure/{name}.Infrastructure.csproj
dotnet sln add src/{name}.Api/{name}.Api.csproj

cd src/{name}.Application
dotnet add reference ../{name}.Domain/{name}.Domain.csproj

cd ../{name}.Infrastructure
dotnet add reference ../{name}.Application/{name}.Application.csproj

cd ../{name}.Api
dotnet add reference ../{name}.Infrastructure/{name}.Infrastructure.csproj
```

> **Nunca uses un espacio en el nombre de un proyecto.** Un espacio en el destino de un `ProjectReference` rompe la resolución copy-local del SDK para los `PackageReference` *transitivos* de ese proyecto: `dotnet build` los copia bien, pero `dotnet publish` los descarta en silencio y solo aparece como `FileNotFoundException` en runtime. (El fichero de solución sí puede tenerlo, como `Aurora Coinly.slnx`, pero obliga a citarlo en cada comando.)

## Convenciones de nombres

| Elemento | Convención | Ejemplo |
|------|------------|---------|
| Proyectos | PascalCase con puntos | `Aurora.Coinly.Domain` |
| Namespaces | PascalCase, igual que la carpeta | `Aurora.Coinly.Domain.Wallets` |
| Clases | PascalCase | `Wallet` |
| Interfaces | IPascalCase | `IDomainEvent` |
| Commands | `[Acción][Entidad]Command` | `CreateWalletCommand` |
| Queries | `Get[Entidad][ById\|List]Query` | `GetWalletByIdQuery` |
| Handlers | `[Command\|Query]Handler` | `CreateWalletCommandHandler` |
| Validators | `[Command]Validator` | `CreateWalletCommandValidator` |
| Models | `[Entidad]Model` | `WalletModel` |
| Domain events | `[Entidad][Hecho]Event` | `WalletSavingsUpdatedEvent` |
| Handlers de eventos | `[Efecto]On[Hecho]EventHandler` | `UpdateSummaryOnTransactionPaidEventHandler` |
| Errores | `[Entidad]Errors` | `WalletErrors` |

## Reglas críticas

1. **Domain no tiene dependencias** — ni de otras capas ni de paquetes externos (salvo Json.NET para `SerializerSettings`)
2. **Application depende solo de Domain** — sin preocupaciones de infraestructura
3. **Infrastructure implementa las interfaces** declaradas en Application
4. **Api solo referencia Infrastructure**
5. **Inyección por constructor**, usando constructor primario
6. **Result pattern** en lugar de excepciones para errores de negocio
7. **Commands modifican estado, queries leen** (CQRS sin MediatR: interfaces propias decoradas con Scrutor)
8. **Un handler por command/query** — nada compartido
9. **Sin repository pattern** — los handlers usan la interfaz del DbContext
10. **Los domain events se levantan en el dominio**, se persisten en un outbox y se manejan en Application
11. **Siempre `CancellationToken`** en métodos async
12. **`record` para DTOs** y datos inmutables
13. **Constantes**, nunca magic numbers ni strings
14. **Tests de arquitectura** (NetArchTest) para las dependencias entre capas y las convenciones de nombres, `sealed` y visibilidad

## Cadena de behaviors

Se registran con Scrutor mediante `services.Decorate(...)`, sin registro manual de cada handler. Cada `Decorate` envuelve al anterior, así que el orden de registro es el inverso al de ejecución:

```
LoggingBehavior → ValidationBehavior → handler
```

Logging envuelve a validation para que el log capture el round-trip completo, incluidos los fallos de validación. Decide explícitamente si `ValidationBehavior` decora también a los queries o solo a los commands — en Aurora Coinly decora **solo los commands**.

## Errores frecuentes

- **N+1**: usa `.Include()` o joins explícitos
- **Over-fetching**: proyecta solo las columnas necesarias
- **Índices ausentes**: revisa los planes de ejecución y añade índices para los filtros comunes
- **Deadlocks**: no mezcles sync y async
- **Timeouts**: configura timeouts en los clientes HTTP
- **Cache stampede**: usa locks distribuidos al poblar la caché
