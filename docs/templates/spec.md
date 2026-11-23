# <Título> — Diseño

**Fecha:** YYYY-MM-DD
**Estado:** Borrador
**Tickets:** —
**Rama:** `feature/<tema>` · **PR:** —
**Plan:** —

## 1. Contexto y problema

Qué pasa hoy, con referencias al código (`Ruta/Fichero.cs:línea`) y por qué es un problema.

## 2. Objetivo y criterios de éxito

- Qué tiene que ser cierto cuando esté terminado, de forma verificable.

## 3. Alcance

**Dentro:**
-

**Fuera de alcance:** (y dónde se cubrirá: ticket, punto del roadmap o "no se hará")
-

## 4. Supuestos y decisiones

| # | Supuesto o decisión | Motivo |
|---|---|---|
| 1 | | |

## 5. Enfoques considerados

| Enfoque | Ventajas | Inconvenientes |
|---|---|---|
| **A (elegido)** | | |
| B | | |

Por qué se eligió A.

## 6. Diseño

### Domain

### Application

(comandos y consultas, handlers, validators, models y, si los hay, eventos de dominio y sus `IDomainEventHandler`)

### Infrastructure

### API

## 7. Errores y códigos HTTP

| Caso | Error | HTTP |
|---|---|---|
| El recurso no existe o es de otro usuario | `Entity.NotFound` | 404 |
| | `Entity.Code` | 4xx |

## 8. Datos y migraciones

Cambios de esquema, migración, datos existentes y reversibilidad (`Down`). Recuerda que Staging migra al arrancar y Producción no. "Sin cambios de esquema" si aplica.

## 9. Tests

- **Domain:**
- **Arquitectura:** (tipos nuevos que comprueban los tests de arquitectura)
- **Verificación manual:** (lo que no tiene proyecto de tests: handlers, endpoints, configuración de EF, outbox)

## 10. Riesgos y preguntas abiertas

-
