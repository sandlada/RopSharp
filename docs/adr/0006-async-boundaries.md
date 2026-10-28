# ADR 0006 — Asynchronous boundaries and lifts

- **Status:** Accepted
- **Date:** 2026-10-28
- **Supersedes:** none
- **Related:** [ADR 0001](0001-core-design-decisions.md) (D2, D6, D8, D9),
  [ADR 0002](0002-api-gap-remediation.md) (D17, D18), [ADR 0004](0004-validation-traversal.md)

## Context

`Rop.Try` and `Rop.TryAsync` are the only places where an exception becomes a domain value (ADR 0001, D2). The
synchronous surface covered value-returning boundaries only, and the asynchronous surface covered
`Func<Task<T>>` only. Two gaps followed:

- side-effecting boundaries (save, send, delete) had to invent a placeholder payload such as `bool` or `object` to
  use `Try`, which pollutes the result type and the downstream API; and
- `Rop.Switch` / `Rop.TrySwitch` lifted synchronous functions, but an asynchronous function had to be wrapped by
  hand (`value => Rop.TryAsync(() => func(value), errorFactory)`), which is exactly the ceremony the library exists
  to remove.

This ADR records the additive decisions that close both gaps.

## Decisions

### D40 — Payload-free boundaries return `Result<Unit, RopError>`

`Rop.Try(Action, Func<Exception, RopError>)` and `Rop.TryAsync(Func<Task>, Func<Exception, RopError>,
CancellationToken)` execute the action and report `Ok(Unit.Value)` on success. `Unit` already exists in the library
and is the documented way to say "success without a payload" (it is also used by `Ensure`), so no new concept is
introduced.

Both share the existing factory and `string code` shapes, and both rethrow `OperationCanceledException` exactly like
their value-returning counterparts.

**Consequence:** boundary results stay composable (`MapOk`, `BindOk`, `Combine`, ...) without placeholder payloads,
and the error codes (`E_EXN` or the supplied code) stay identical to the value-returning boundary.

### D41 — Asynchronous lifts mirror the synchronous lifts

`Rop.SwitchAsync<T, TOut, TErr>(Func<T, Task<TOut>>)` returns `Func<T, Task<Result<TOut, TErr>>>`, and
`Rop.TrySwitchAsync<T, TOut>(Func<T, Task<TOut>>, Func<Exception, RopError>)` returns
`Func<T, Task<Result<TOut, RopError>>>`. They are the awaited counterparts of `Rop.Switch` and `Rop.TrySwitch` and
delegate to the existing `TryAsync<T>` boundary, so null-task, null-payload (`E_NULL_RESULT`) and cancellation
behaviour are inherited rather than re-implemented.

The lifts take no `CancellationToken` of their own: a lift is a pure adapter, and the caller's token is captured by
the supplied function (ADR 0001, D9 forbids the library from creating tokens).

**Consequence:** an asynchronous function can be lifted once and reused, and the `Switch`/`SwitchAsync` pair is
symmetric. The names collide with the `Result` terminals `Switch`/`SwitchAsync`; the receiver distinguishes them, and
the XML docs state the difference explicitly.

### D42 — Generic inference stays limited to what C# can infer

`TErr` cannot be inferred from `Func<T, Task<TOut>>`, so `SwitchAsync` follows `Switch` and requires explicit type
arguments when the error type is not fixed by the return context. `TrySwitchAsync` fixes `TErr` to `RopError`, so it
only needs `<T, TOut>`.

**Consequence:** the lift API is consistent with the shipped synchronous lifts rather than introducing a second
inference style.

### D43 — The synchronous boundary documents the `Task` footgun

`Rop.Try<T>(Func<T>)` accepts `T = Task`, because C# has no way to exclude a type argument. Passing a task-returning
lambda therefore produces `Ok(Task)` instead of awaiting. The XML remarks and README now direct callers to
`Rop.TryAsync`.

**Consequence:** the hazard is documented at the call site without adding an analyzer or a breaking overload.

## Alternatives considered

| Alternative | Why rejected |
| --- | --- |
| Return `bool` from payload-free boundaries | Loses the error and reintroduces out-parameters/try-catch at call sites |
| Add a new `Rop.Unit`/`Nothing` type | `Unit` already exists and is used by `Ensure` |
| Make `Try(Action)` return `Result<object, RopError>` | Invents a payload that every consumer must ignore |
| Add `ct` to the lifts | A lift has no async work of its own; the captured function owns cancellation |
| Add a separate `LiftAsync` name | Breaks the `Switch`/`SwitchAsync` symmetry (ADR 0001, D6) |
| Add an analyzer that bans `T = Task` | A new package and diagnostics surface for a documented hazard |
| Implicitly await a returned `Task` in `Try<T>` | Changes the meaning of `T` and breaks `Result<Task, _>` users; the type system cannot express the restriction |

## Consequences

- Every boundary shape (sync/async, value/no value, catching/non-catching) now has one obvious entry point.
- `Unit` becomes part of the boundary vocabulary rather than an `Ensure`-only detail.
- The public API baseline grows by six members.
