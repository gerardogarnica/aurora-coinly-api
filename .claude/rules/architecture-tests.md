---
paths:
  - "test/Aurora.Coinly.ArchitectureTests/**/*.cs"
  - "Directory.Packages.props"
---

# Architecture test conventions

Stack: xUnit **v2** + NetArchTest.Rules 1.3.2 + Shouldly (**not** FluentAssertions — that stays in the unit test project). One file per layer: `DomainLayerTests`, `ApplicationLayerTests`, `ApiLayerTests`, plus `LayerDependencyTests` for the inter-assembly rules. The project references only `Aurora.Coinly.Api`; `BaseTest` exposes the four assemblies.

What they enforce is listed in `CLAUDE.md` ("Conventions enforced by architecture tests"). When a convention changes, change the test in the same commit.

## NetArchTest pitfalls

- **`.Or()` starts a new predicate sequence.** A later `.And()` applies only to the *last* sequence, so `.ImplementInterface(A).Or().ImplementInterface(B).And().AreNotAbstract()` leaves branch A unfiltered. The existing `Command_*` and `CommandHandler_*` tests are written that way; they pass today because no abstract type implements `ICommand<>`/`ICommandHandler<,>`. In a new test, write one test per interface instead of chaining with `.Or()`.
- **Never use `.BeImmutable()` on records.** `init` accessors compile to non-readonly backing fields, so every record is reported as mutable. Detect a record by the synthesized `<Clone>$` method instead.
- **`Type.Name` carries the generic arity** (``Result`1``), so trim at the backtick before any suffix check.
- **A reflection test that selects zero types passes silently.** When adding one, verify the selector actually matches something before trusting the green (e.g. assert the selected count is greater than zero).
- `Domain_Should_NotHaveDependencyOn_*` only checks the other Coinly assemblies. It cannot express "Domain has no third-party dependencies"; if that rule is ever wanted, use `Assembly.GetReferencedAssemblies()` (Domain legitimately references Newtonsoft.Json for `SerializerSettings`).

## Scope

The selectors only see `BaseEntity` subclasses, `ICommand*`/`IQuery*` implementations, `AbstractValidator<>` subclasses, `IDomainEventHandler<>` implementations and `IBaseEndpoint` implementations. Child entities (`WalletHistory`), value objects and models are not checked.
