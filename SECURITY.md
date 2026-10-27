# Security policy

## Supported versions

| Version | Supported |
| --- | --- |
| 1.x | Yes |
| < 1.0 | No |

## Reporting a vulnerability

Please **do not open a public issue** for security problems.

Use GitHub's private vulnerability reporting on the repository
(*Security* → *Advisories* → *Report a vulnerability*). If that channel is unavailable, contact the
`Sandlada` organization maintainers directly.

Include:

- affected version(s) and target framework,
- a minimal reproduction (code snippet or repository),
- the impact you believe is possible,
- any suggested fix.

You can expect an acknowledgement within a few business days. Once a fix is available, a coordinated
disclosure and a patched release will be arranged.

## Scope

`Sandlada.RopSharp` is a pure managed library with:

- no reflection or dynamic code generation,
- no serialization hooks, no `ISerializable`, no `BinaryFormatter`,
- no file system, network or process access,
- no third-party runtime dependencies.

The relevant attack surface is therefore limited to misuse of the public API (for example, passing untrusted
functions to `Rop.Try`, or trusting `RopError.Message` as data). Treat `RopError.Code` as the stable,
machine-readable branch key and `Message`/`Details` as untrusted display text when they contain external input.
