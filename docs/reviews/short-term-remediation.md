# Remediación a corto plazo

**Fecha:** 2026-11-20
**Estado:** Borrador
**Tickets:** —
**Rama:** — · **PR:** —
**Relacionado:** `.claude/rules/` (cada punto enlaza la rule donde está documentado)

Problemas encontrados al revisar los agents, skills y rules de Coinly. Ninguno está corregido. Cada punto dice dónde está en el código, qué riesgo tiene, qué se propone y cómo abordarlo:

- **Acotado:** diseño corto en el chat y se implementa con la skill indicada.
- **Spec:** cambio arquitectónico, que pasa por `spec` → `write-plan` → `execute-plan`.

Cuando se resuelva un punto, márcalo en la tabla, enlaza el PR y actualiza la rule que lo menciona como deuda pendiente.

## Resumen

| # | Prioridad | Problema | Enfoque | Estado |
|---|---|---|---|---|
| 1 | Alta | Tokens guardados en claro en `user_tokens` | Spec | Pendiente |
| 2 | Alta | El outbox no reintenta: un fallo es un efecto perdido | Spec | Pendiente |
| 3 | Alta | El outbox deserializa cualquier `$type` | Acotado | Pendiente |
| 4 | Alta | Swagger se publica en Producción | Acotado | Pendiente |
| 5 | Media | Ninguna sección de opciones se valida al arrancar | Acotado | Pendiente |
| 6 | Media | Producción no aplica migraciones | ADR | Pendiente |
| 7 | Media | Las tablas no tienen índice por `UserId` | Acotado | Pendiente |
| 8 | Media | Los validadores de queries nunca se ejecutan | Acotado | Pendiente |
| 9 | Media | `/health` no comprueba nada | Acotado | Pendiente |
| 10 | Baja | `BudgetUpdatedEvent` lleva una entidad y no se usa | Acotado | Pendiente |
| 11 | Baja | `ProcessOutboxJob` ignora el `CancellationToken` y registra demasiados logs | Acotado (con el punto 2) | Pendiente |
| 12 | Baja | `Update()` sobre entidades ya trackeadas | Acotado | Pendiente |
| 13 | Baja | Tests de arquitectura con el error de `.Or()` | Acotado | Pendiente |
| 14 | Baja | Los datos de prueba de los tests de dominio dependen de la fecha actual | Acotado | Pendiente |

Orden sugerido:

1. **Primero:** los puntos 3, 4, 5 y 10. Son pequeños, independientes y quitan riesgo enseguida.
2. **Después:** el 1 y el 2, cada uno con su spec.
3. **El resto:** cuando se toque cada área.

---

## Prioridad alta

### 1. Tokens guardados en claro en `user_tokens`

- **Dónde:**
  - `src/Aurora.Coinly.Domain/Users/UserToken.cs:6` y `:8`
  - `src/Aurora.Coinly.Infrastructure/Configurations/UserTokenConfiguration.cs:13-14`
  - `src/Aurora.Coinly.Application/Authentication/Refresh/RefreshTokenCommandHandler.cs:15`, que busca el token comparándolo en claro.
- **Riesgo:**
  - Quien lea la tabla (un backup, una réplica o una consulta con permisos amplios) obtiene refresh tokens válidos y puede generar access tokens de cualquier usuario mientras no caduquen.
  - El access token también se guarda, aunque ningún código lo vuelve a leer: solo se escribe en `LoginCommandHandler.cs:44` y `RefreshTokenCommandHandler.cs:51`.
- **Propuesta:**
  1. Guardar solo el hash SHA-256 del refresh token y buscar por ese hash. No hace falta un hash lento: el token es aleatorio y largo.
  2. Dejar de guardar el access token, o justificar para qué se guarda.
  3. Añadir un índice único sobre el hash.
  4. Rotar el refresh token en cada uso. Hoy `RefreshTokenCommandHandler` actualiza el registro y conviene confirmar si emite uno nuevo.
- **Datos:** la migración invalida todos los tokens existentes, así que los usuarios tienen que volver a iniciar sesión. Hay que avisar antes del despliegue.
- **Enfoque:** spec. Afecta a Domain, Application, Infrastructure y a una migración. Al terminar, escribir `.claude/rules/authentication.md`, que se dejó fuera precisamente por esto.

### 2. El outbox no reintenta: un fallo es un efecto perdido

- **Dónde:**
  - `src/Aurora.Coinly.Infrastructure/Outbox/ProcessOutboxJob.cs:37-44`: cualquier excepción se registra y el mensaje se marca como procesado igualmente (`:83-86`).
  - `DomainEventsDispatcher` ejecuta los handlers en secuencia: si uno falla, los siguientes no corren.
- **Riesgo:** una transacción pagada puede quedar sin reflejar en `MonthlySummary` o en el presupuesto, y nadie se entera salvo que consulte la columna `error`.
- **Propuesta:**
  1. Hacer idempotentes los handlers de eventos. Es un requisito previo, porque reintentar convierte la entrega en *at-least-once*:
     - `UpdateSummaryOnTransactionPaidEventHandler`
     - `UpdateBudgetOnTransactionPaidEventHandler`
     - `UpdateSummaryOnSavingsAssignedEventHandler`
  2. Añadir a `outbox_messages` las columnas `attempts` y `next_attempt_on_utc`, con backoff y un máximo de intentos configurable en `OutboxOptions`.
  3. Al agotar los intentos, dejar el mensaje en un estado "fallido" consultable, distinto de "procesado".
  4. Valorar registrar cada handler por separado (una tabla de consumidores por mensaje), para que el fallo de uno no bloquee a los demás.
- **Enfoque:** spec. Incluir aquí el punto 11.
- **Rule:** `domain-events-outbox.md` (secciones "Delivery" y "Job conventions").

### 3. El outbox deserializa cualquier `$type`

- **Dónde:**
  - `src/Aurora.Coinly.Domain/Abstractions/SerializerSettings.cs:9` (`TypeNameHandling.All`)
  - `src/Aurora.Coinly.Infrastructure/Outbox/ProcessOutboxJob.cs:31-33`, que deserializa sin `SerializationBinder`.
- **Riesgo:** hoy solo escribe en la tabla el interceptor. Aun así, quien consiga escribir una fila puede hacer que el job instancie cualquier tipo cargado en el proceso, que es la vía clásica de *gadget chains* en Json.NET.
- **Propuesta:** un `ISerializationBinder` que acepte solo:
  - los tipos del ensamblado Domain que implementan `IDomainEvent`;
  - los tipos concretos del ensamblado Domain que implementan `IValueObject`, porque `WalletSavingsUpdatedEvent` lleva un `Money` con su `Currency` y cada uno viaja con su propio `$type`. Un binder que aceptara solo eventos rechazaría ese evento.

  Rechazar el resto con un error claro. `CurrencyTests` ya comprueba la ida y vuelta de `WalletSavingsUpdatedEvent` con `SerializerSettings.Instance`. Al añadir el binder, ampliarlo a cada evento existente.
- **Enfoque:** acotado (skills `domain-entity` y `unit-testing`).

### 4. Swagger se publica en Producción

- **Dónde:**
  - `src/Aurora.Coinly.Api/Program.cs:29-33`: `UseSwagger` y `UseSwaggerUI` se llaman sin condición de entorno.
  - `src/Aurora.Coinly.Api/DependencyInjection.cs:16-19`: no se declara el esquema de seguridad.
- **Riesgo:** expone el contrato completo de la API en Producción. Además, sin el esquema `Bearer` la UI no sirve para probar los endpoints protegidos.
- **Propuesta:**
  - Servir Swagger solo en Development y Staging, en el mismo bloque que las migraciones.
  - Añadir `AddSecurityDefinition("Bearer", ...)` y el requisito de seguridad global.
- **Enfoque:** acotado.
- **Rule:** `health-and-observability.md` (sección "Swagger").

## Prioridad media

### 5. Ninguna sección de opciones se valida al arrancar

- **Dónde:** `src/Aurora.Coinly.Infrastructure/DependencyInjection.cs`:
  - `:34` (`JwtAuthOptions`)
  - `:63` (`EncryptionOptions`)
  - `:112` (`OutboxOptions`)
- **Riesgo:** si falta `Jwt:Key` o `Encryption:MasterKey`, o si `Outbox:IntervalInSeconds` vale 0, la API arranca bien y falla en la primera petición o en el primer disparo del job.
- **Propuesta:** `AddOptions<T>().BindConfiguration(...).ValidateDataAnnotations().ValidateOnStart()`, o un `IValidateOptions<T>` cuando haga falta una regla propia, con estos mínimos:
  - longitud mínima de `Jwt:Key`;
  - `MasterKey` en base64 válido y con la longitud esperada;
  - `IntervalInSeconds > 0` y `BatchSize > 0`.
- **Enfoque:** acotado.
- **Rule:** `startup-and-migrations.md` (sección "Configuration").

### 6. Producción no aplica migraciones

- **Dónde:** `src/Aurora.Coinly.Api/Program.cs:35-38`, que solo migra en Development y Staging.
- **Riesgo:** un despliegue con migración llega a Producción con el esquema viejo si nadie la aplica a mano, y falla en cuanto se usa una columna nueva. Hoy no hay ningún paso de CI ni de Dokploy que lo haga.
- **Propuesta:** decidirlo en un ADR. Opciones:
  - un *migration bundle* (`dotnet ef migrations bundle`) generado en CI y ejecutado por Dokploy antes de arrancar el contenedor;
  - un script SQL idempotente revisado en el PR;
  - migrar al arrancar también en Producción, aceptando el riesgo.
- **Enfoque:** ADR en `docs/architecture/adr/`, seguido de los cambios en `.github/workflows/build.yml`.

### 7. Las tablas no tienen índice por `UserId`

- **Dónde:** `src/Aurora.Coinly.Infrastructure/Configurations/`. Solo `UserConfiguration.cs:26-27` declara índices.
- **Riesgo:** casi todas las consultas filtran por `UserId`, así que cada una recorre la tabla entera. Con pocos datos no se nota; con `transactions` y `wallet_history` creciendo, sí.
- **Propuesta:**
  - un índice por `UserId` en `wallets`, `categories`, `payment_methods`, `transactions`, `budgets`, `monthly_summaries` y `user_tokens`;
  - índices compuestos donde la consulta lo pida: `transactions (user_id, transaction_date)`, `monthly_summaries (user_id, year, month)`.

  Todo en una sola migración.
- **Enfoque:** acotado (skill `domain-entity`, sección de EF). Conviene que `arch-guard` revise la migración, porque Staging migra al arrancar.

### 8. Los validadores de queries nunca se ejecutan

- **Dónde:**
  - `src/Aurora.Coinly.Application/DependencyInjection.cs:40-41`: `ValidationBehavior` decora solo los comandos.
  - `src/Aurora.Coinly.Application/Wallets/GetHistory/GetWalletHistoryQueryValidator.cs`: es el único validador de query y su regla `DateTo >= DateFrom` no se aplica nunca.
- **Riesgo:** un rango de fechas invertido llega al handler, que devuelve una lista vacía sin decir por qué. El fichero hace creer que la regla está cubierta.
- **Propuesta:** elegir una de estas dos opciones y documentarla en `create-feature`:
  - añadir `ValidationBehavior.QueryHandler<,>` y registrarlo antes de `LoggingBehavior`;
  - borrar el validador y mover la comprobación al handler.
- **Enfoque:** acotado (skill `create-feature`).

### 9. `/health` no comprueba nada

- **Dónde:**
  - `src/Aurora.Coinly.Api/DependencyInjection.cs:73`: `AddHealthChecks()` sin ningún check registrado.
  - `src/Aurora.Coinly.Api/Extensions/HealthCheckExtensions.cs:7`.
- **Riesgo:** el endpoint responde `Healthy` aunque la base de datos esté caída.
- **Propuesta:**
  - Separar `/health/live`, sin dependencias, para Dokploy, y `/health/ready`, con un check de PostgreSQL con timeout y etiqueta `ready`, para monitorización.
  - Respuestas con descripciones fijas, sin host ni mensaje de excepción.
  - Excluir ambos endpoints del tracing.
- **Enfoque:** acotado.
- **Rule:** `health-and-observability.md` (sección "Health").

## Prioridad baja

### 10. `BudgetUpdatedEvent` lleva una entidad y no se usa

- **Dónde:** `src/Aurora.Coinly.Domain/Budgets/BudgetDomainEvents.cs:3`. Ningún código lo lanza y no tiene handler.
- **Riesgo:** ninguno hoy, pero es la única excepción a la regla "los eventos llevan ids, no entidades", y alguien puede copiarlo.
- **Propuesta:** borrarlo. Al no lanzarse nunca, no hay filas pendientes en el outbox con ese tipo. Después, quitar la excepción de `domain-model.md`, `domain-events-outbox.md` y de la skill `domain-entity`.
- **Enfoque:** acotado.

### 11. `ProcessOutboxJob` ignora el `CancellationToken` y registra demasiados logs

- **Dónde:** `src/Aurora.Coinly.Infrastructure/Outbox/ProcessOutboxJob.cs`:
  - `:21`, `:23`, `:35`, `:47` y `:89-91` no reciben `context.CancellationToken`;
  - `:19` y `:49` registran `Information` en cada ejecución, aunque no haya mensajes.
- **Riesgo:** un apagado del contenedor espera a que termine el lote (`WaitForJobsToComplete = true`) sin poder cancelarlo, y Seq se llena de ruido cada intervalo.
- **Propuesta:**
  - pasar el token a cada llamada;
  - usar `ToListAsync`;
  - registrar `Information` solo cuando se procesaron mensajes.
- **Enfoque:** junto con el punto 2.

### 12. `Update()` sobre entidades ya trackeadas

- **Dónde:** 24 llamadas en `src/Aurora.Coinly.Application`, la mitad de ellas en los handlers de `Wallets`, que añaden entidades hijas `WalletHistory`. Por ejemplo:
  - `Categories/Update/UpdateCategoryCommandHandler.cs:36`
  - `Budgets/UpdateLimit/UpdateBudgetLimitCommandHandler.cs:36`
  - `Authentication/ChangePassword/ChangeUserPasswordCommandHandler.cs:37`

  La lista completa sale con `grep -rn "dbContext\.\w*\.Update(" src/Aurora.Coinly.Application`.
- **Riesgo:** hoy son llamadas redundantes. Pasarían a fallar con `DbUpdateConcurrencyException` si una entidad hija llegara a tener una clave asignada por el dominio.
- **Propuesta:** quitarlas, una por handler, en un único PR. No cambia el comportamiento.
- **Enfoque:** acotado.
- **Rule:** `ef-core-queries.md` (sección "Tracking").

### 13. Tests de arquitectura con el error de `.Or()`

- **Dónde:** `test/Aurora.Coinly.ArchitectureTests/ApplicationLayerTests.cs:15`, `:32`, `:49`, `:66`, `:83`, `:91` y `:170`.
- **Riesgo:** el `.And().AreNotAbstract()` final solo filtra la segunda rama. Los tests pasan por casualidad, y uno nuevo escrito igual puede dejar tipos sin comprobar.
- **Propuesta:**
  - un test por interfaz en lugar de encadenar con `.Or()`;
  - en cada selector, comprobar que selecciona al menos un tipo.
- **Enfoque:** acotado.
- **Rule:** `architecture-tests.md`.

### 14. Los datos de prueba de los tests de dominio dependen de la fecha actual

- **Dónde:** `test/Aurora.Coinly.Domain.UnitTests`. Usan `DateTime.UtcNow` o `DateTime.Today` los ficheros `*Data.cs` de Budgets, Categories, Methods, Transactions y Users, y también `BudgetTests`, `BudgetPeriodServiceTests`, `CategoryTests`, `PaymentMethodTests` y `TransactionTests`.
- **Riesgo:** los tests de periodos y fechas pueden fallar según el día en que corran, por ejemplo a fin de mes o en un año bisiesto.
- **Propuesta:** fechas fijas en los datos de prueba, como ya hace `WalletData`.
- **Enfoque:** acotado (skill `unit-testing`). Conviene hacerlo antes o junto con la migración a xUnit v3.
- **Rule:** `unit-tests.md`.
