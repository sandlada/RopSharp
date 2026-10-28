# ADR 0007 — Diagnostics: open-ended error kinds and safe input echoing

- **Status:** Accepted
- **Date:** 2026-10-28
- **Supersedes:** none
- **Related:** [ADR 0001](0001-core-design-decisions.md) (D2, D6), [ADR 0002](0002-api-gap-remediation.md)
  (D21, D23), [ADR 0005](0005-lossless-error-aggregation.md)

## Context

Two diagnostics gaps remained after the 1.1 surface:

- `RopErrorKind` stopped at `Unexpected`, so common production categories (deadlines, dependency outages, quotas) had
  to be encoded in `Code` and re-parsed by transport layers — exactly what the kind was introduced to avoid.
- `Rop.Parse<T>` echoed the input in the message, but rendered `null` as an empty pair of quotes (indistinguishable
  from an empty string), put nothing machine-readable in `Details`, and echoed arbitrarily long values with raw
  control characters into logs.

Both are production-quality concerns rather than new features: they change how failures are *reported*, not which
failures exist.

## Decisions

### D44 — `RopErrorKind` is append-only and open-ended

`Timeout = 7`, `Unavailable = 8` and `RateLimited = 9` are appended. Existing values keep their numbers, so the enum
stays binary-compatible. The XML docs and README state that the enum is open-ended: consumers that switch over it
exhaustively should keep a fallback arm.

**Consequence:** transport layers can map deadlines, outages and quotas without parsing codes, and a future category
can be added without renumbering. The trade-off is that exhaustive `switch` expressions in consumer code may need a
new arm (`CS8509`), which the docs call out.

### D45 — Parse failures echo the input in the message and in `Details`

A failed `Rop.Parse<T>` produces:

- `Message`: `'<escaped input>' is not a valid <T>.` — unchanged for ordinary input;
- `Details`: `input=<quoted>` — machine-readable correlation;
- `Message`/`Details` for `null`: `The value is null, which is not a valid <T>.` / `input=null`.

The `code` parameter, `E_PARSE` default and `Kind` (`Failure`) are unchanged.

**Consequence:** a single error carries enough context to reproduce the failure, while "missing" and "empty" remain
distinguishable. Callers that need to suppress the echo can still construct their own `RopError`.

### D46 — Diagnostic text is bounded, escaped and surrogate-safe

Echoed caller text goes through an internal `DiagnosticText` helper:

- at most 64 code points are echoed, followed by `...` when truncated;
- truncation never splits a surrogate pair (a dangling high surrogate is dropped before the marker);
- `\`, `"`, `\r`, `\n`, `\t` and other control characters are escaped (`\uXXXX`), so a value cannot forge log lines
  or break the message structure;
- `Quote` wraps the escaped text in double quotes, or returns the literal `null`.

**Revision (2026-10-28, maintenance):** the implementation was aligned with this contract. Truncation counts Unicode
code points rather than UTF-16 code units, a lone surrogate is rendered as `\uXXXX` so the output is always
well-formed UTF-16, and `U+2028`/`U+2029` plus bidirectional control characters (`U+061C`, `U+200E`, `U+200F`,
`U+202A`–`U+202E`, `U+2066`–`U+2069`) are escaped because they are invisible but can still forge or reorder log output.

The helper is internal: the policy is part of the observable error output, but there is no second public formatting
API to maintain.

**Consequence:** a hostile or accidental payload cannot inflate an error, corrupt logs, or produce invalid text.
Ordinary inputs render exactly as before.

## Alternatives considered

| Alternative | Why rejected |
| --- | --- |
| Keep the enum closed and add more codes | Forces every consumer to re-implement code parsing; defeats D21 |
| Renumber or reorder the enum | Breaks binary compatibility and persisted values |
| Add a `Canceled` kind | Cancellation is not a domain failure (ADR 0001, D9) |
| Echo the raw input without escaping | Log injection and broken single-line messages |
| JSON-encode the details | Adds escaping/parsing burden and size for a field meant to be human-readable diagnostics |
| Truncate by UTF-16 code unit | Can split a surrogate pair and produce invalid text |
| Expose `DiagnosticText` publicly | Invites a second formatting contract that would then need stability guarantees |
| Put the type name in `Details` | `typeof(T).Name` is already in the message and is not needed for correlation |

## Consequences

- Error categories cover the common retry/backoff decisions without code parsing.
- Parse errors are safe to log and correlate by default.
- No new public helper types; the API baseline grows by three enum members only.
