using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.RopSharp;

/// <summary>
/// An immutable value that is either a success carrying <typeparamref name="T"/> or a failure carrying a
/// non-empty list of <typeparamref name="TErr"/>.
/// </summary>
/// <typeparam name="T">The success value type.</typeparam>
/// <typeparam name="TErr">The error element type.</typeparam>
/// <remarks>
/// <para>
/// Validation accumulates every error in argument or element order. It never short-circuits like
/// <see cref="Result{TOk,TErr}"/>. Use <see cref="Collapse"/> to fold failures back onto the short-circuit track.
/// </para>
/// <code><![CDATA[
/// var validated = Validation.Zip(
///         Result.Ok<Order, RopError>(OrderFixtures.Sample),
///         Result.Err<decimal>(RopError.Create("E_AMOUNT", "the amount must be positive")),
///         static (order, amount) => order.Id);
///
/// // Success("ord-1001") when every input is Ok,
/// // Failure([E_AMOUNT]) when a check failed - Zip collects every error.
/// ]]></code>
/// </remarks>
[DebuggerDisplay("{ToString(),nq}")]
public readonly struct Validation<T, TErr> : IEquatable<Validation<T, TErr>>
{
    internal const byte TagUninitialized = 0;
    internal const byte TagSuccess = 1;
    internal const byte TagFailure = 2;

    private readonly T _value;
    private readonly IReadOnlyList<TErr> _errors;
    private readonly byte _tag;

    internal Validation(T value, IReadOnlyList<TErr> errors, byte tag)
    {
        _value = value;
        _errors = errors;
        _tag = tag;
    }

    /// <summary>Gets a value indicating whether this instance is a success.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = Validation.From(Result.Ok(OrderFixtures.Sample));
    /// var succeeded = validated.IsSuccess;
    ///
    /// // Success(order) makes IsSuccess true,
    /// // Failure([E_X]) makes IsSuccess false - errors are read with TryGetErrors.
    /// ]]></code>
    /// </remarks>
    public bool IsSuccess => _tag == TagSuccess;

    /// <summary>
    /// Gets a value indicating whether this instance is a failure or an uninitialized default.
    /// </summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = Validation.From(Result.Err<Order, RopError>(RopError.Create("E_ORDER", "missing")));
    /// var failed = validated.IsFailure;
    ///
    /// // Failure([E_ORDER]) makes IsFailure true,
    /// // Success(order) makes IsFailure false - the list is never empty on the failure track.
    /// ]]></code>
    /// </remarks>
    public bool IsFailure => _tag != TagSuccess;

    /// <summary>
    /// Attempts to read the success value without throwing.
    /// </summary>
    /// <param name="value">The success value when this method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> on the success track; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = Validation.From(Result.Ok(OrderFixtures.Sample));
    /// if (validated.TryGetSuccess(out var order))
    /// {
    ///     var id = order.Id;
    /// }
    ///
    /// // Success(order) fills 'order',
    /// // Failure([E_X]) leaves 'order' at default - TryGetSuccess never throws.
    /// ]]></code>
    /// </remarks>
    public bool TryGetSuccess([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return _tag == TagSuccess;
    }

    /// <summary>
    /// Attempts to read the non-empty error list.
    /// </summary>
    /// <param name="errors">The errors when this method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> on the failure track; otherwise <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">This instance is an uninitialized default.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = Validation.From(Result.Err<Order, RopError>(RopError.Create("E_ORDER", "missing")));
    /// if (validated.TryGetErrors(out var errors))
    /// {
    ///     var code = errors[0].Code;
    /// }
    ///
    /// // Failure([E_ORDER]) fills 'errors',
    /// // Success(order) returns false - TryGetErrors never throws for a real validation value.
    /// ]]></code>
    /// </remarks>
    public bool TryGetErrors([MaybeNullWhen(false)] out IReadOnlyList<TErr> errors)
    {
        ThrowIfUninitialized();
        errors = _errors;
        return _tag == TagFailure;
    }

    /// <summary>
    /// Maps the success value when this instance is a success; failures pass through unchanged.
    /// </summary>
    /// <typeparam name="TOut">The mapped success type.</typeparam>
    /// <param name="mapper">Maps the success payload.</param>
    /// <returns>A validation with the mapped success or the original errors.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mapper"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">This instance is an uninitialized default.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = Validation.From(Result.Ok(OrderFixtures.Sample))
    ///     .Map(static order => order.Id);
    ///
    /// // Success("ord-1001") when the source was Success(order),
    /// // Failure([E_X]) stays Failure([E_X]) - Map never runs on the failure track.
    /// ]]></code>
    /// </remarks>
    public Validation<TOut, TErr> Map<TOut>(Func<T, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ThrowIfUninitialized();

        return _tag == TagFailure
            ? new Validation<TOut, TErr>(default!, _errors, TagFailure)
            : global::Sandlada.RopSharp.Validation.Success<TOut, TErr>(mapper(_value));
    }

    /// <summary>
    /// Folds this validation back onto the short-circuit <see cref="Result{TOk,TErr}"/> track.
    /// </summary>
    /// <param name="merge">Combines the non-empty error list into a single error.</param>
    /// <returns><c>Ok</c> on success; otherwise <c>Err(merge(errors))</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="merge"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// This instance is uninitialized, or <paramref name="merge"/> returned <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var placed = Validation.Zip(
    ///         Result.Ok<Order, RopError>(OrderFixtures.Sample),
    ///         Result.Err<decimal>(RopError.Create("E_AMOUNT", "the amount must be positive")),
    ///         static (order, amount) => order.Id)
    ///     .Collapse(RopError.Combine);
    ///
    /// // Ok("ord-1001") when every check passed,
    /// // Err(E_MULTIPLE) when Collapse merged the failure list.
    /// ]]></code>
    /// </remarks>
    public Result<T, TErr> Collapse(Func<IReadOnlyList<TErr>, TErr> merge)
    {
        ArgumentNullException.ThrowIfNull(merge);
        ThrowIfUninitialized();

        if (TryGetSuccess(out var value))
        {
            return Result.OkUnchecked<T, TErr>(value);
        }

        if (_errors is null || _errors.Count == 0)
        {
            throw new InvalidOperationException(
                "A validation failure must carry at least one error.");
        }

        return Result.Err<T, TErr>(Result.RequireError(merge(_errors)));
    }

    /// <summary>
    /// Projects the success value or the first error on the failure track.
    /// </summary>
    /// <typeparam name="TResult">The projected type.</typeparam>
    /// <param name="ok">Projects a success.</param>
    /// <param name="err">Projects the first collected error.</param>
    /// <returns>The projected value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ok"/> or <paramref name="err"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">This instance is uninitialized or the failure list is empty.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// var label = Validation.Zip(
    ///         Result.Ok<Order, RopError>(OrderFixtures.Sample),
    ///         Result.Err<decimal>(RopError.Create("E_AMOUNT", "the amount must be positive")),
    ///         static (order, amount) => order.Id)
    ///     .MatchFirst(
    ///         ok: static id => $"placed {id}",
    ///         err: static error => $"failed {error.Code}");
    ///
    /// // "placed ord-1001" for Success(id),
    /// // "failed E_AMOUNT" for Failure([E_AMOUNT]) - MatchFirst uses only the first error.
    /// ]]></code>
    /// </remarks>
    public TResult MatchFirst<TResult>(Func<T, TResult> ok, Func<TErr, TResult> err)
    {
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(err);

        if (TryGetSuccess(out var value))
        {
            return ok(value);
        }

        return err(FirstError());
    }

    /// <summary>
    /// Runs an action for the success value or the first error on the failure track.
    /// </summary>
    /// <param name="ok">The action for a success.</param>
    /// <param name="err">The action for the first collected error.</param>
    /// <exception cref="ArgumentNullException"><paramref name="ok"/> or <paramref name="err"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">This instance is uninitialized or the failure list is empty.</exception>
    /// <remarks>
    /// <code><![CDATA[
    /// Validation.Zip(
    ///         Result.Ok<Order, RopError>(OrderFixtures.Sample),
    ///         Result.Err<decimal>(RopError.Create("E_AMOUNT", "the amount must be positive")),
    ///         static (order, amount) => order.Id)
    ///     .SwitchFirst(
    ///         ok: static id => Console.WriteLine($"placed {id}"),
    ///         err: static error => Console.WriteLine($"failed {error.Code}"));
    ///
    /// // Success(id) runs the success action,
    /// // Failure([E_AMOUNT]) runs the error action for the first error only.
    /// ]]></code>
    /// </remarks>
    public void SwitchFirst(Action<T> ok, Action<TErr> err)
    {
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(err);

        if (TryGetSuccess(out var value))
        {
            ok(value);
            return;
        }

        err(FirstError());
    }

    private TErr FirstError()
    {
        ThrowIfUninitialized();

        if (_errors is null || _errors.Count == 0)
        {
            throw new InvalidOperationException(
                "A validation failure must carry at least one error when it is on the failure track.");
        }

        return _errors[0];
    }

    internal IReadOnlyList<TErr> ErrorsUnsafe
    {
        get
        {
            ThrowIfUninitialized();
            return _errors;
        }
    }

    internal void ThrowIfUninitialized()
    {
        if (_tag == TagUninitialized)
        {
            throw new InvalidOperationException(
                "The validation is an uninitialized default value; construct values with Validation.Success or Validation.Failure.");
        }
    }

    private static bool ErrorsEqual(IReadOnlyList<TErr> left, IReadOnlyList<TErr> right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left.Count != right.Count)
        {
            return false;
        }

        for (var index = 0; index < left.Count; index++)
        {
            if (!EqualityComparer<TErr>.Default.Equals(left[index], right[index]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var left = Validation.From(Result.Ok(OrderFixtures.Sample));
    /// var right = Validation.From(Result.Ok(OrderFixtures.Sample));
    /// var same = left.Equals(right);
    ///
    /// // Success(order) equals another Success(order) with the same payload,
    /// // Failure([E_X]) equals another Failure with the same error list.
    /// ]]></code>
    /// </remarks>
    public bool Equals(Validation<T, TErr> other) =>
        _tag == other._tag
        && (_tag switch
        {
            TagUninitialized => true,
            TagSuccess => EqualityComparer<T>.Default.Equals(_value, other._value),
            TagFailure => ErrorsEqual(_errors, other._errors),
            _ => false,
        });

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = Validation.From(Result.Ok(OrderFixtures.Sample));
    /// var same = validated.Equals((object)validated);
    ///
    /// // Success(order) equals its own boxed copy,
    /// // Failure([E_X]) equals its own boxed copy - the boxed form compares by value.
    /// ]]></code>
    /// </remarks>
    public override bool Equals(object? obj) => obj is Validation<T, TErr> other && Equals(other);

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = Validation.From(Result.Ok(OrderFixtures.Sample));
    /// var bucket = validated.GetHashCode() % 16;
    ///
    /// // Success(order) hashes by its payload,
    /// // Failure([E_X]) hashes by its error count - equal values land in the same bucket.
    /// ]]></code>
    /// </remarks>
    public override int GetHashCode() =>
        _tag switch
        {
            TagUninitialized => HashCode.Combine(TagUninitialized),
            TagSuccess => HashCode.Combine(TagSuccess, _value),
            TagFailure => HashCode.Combine(TagFailure, _errors.Count),
            _ => HashCode.Combine(_tag),
        };

    /// <inheritdoc />
    /// <remarks>
    /// <code><![CDATA[
    /// var validated = Validation.From(Result.Ok(OrderFixtures.Sample));
    /// var label = validated.ToString();
    ///
    /// // Success(order) renders as Success(order),
    /// // Failure([E_X]) renders as Failure(1) - the text names the active track.
    /// ]]></code>
    /// </remarks>
    public override string ToString() =>
        _tag switch
        {
            TagSuccess => $"Success({_value})",
            TagFailure => $"Failure({_errors.Count})",
            _ => "Validation(uninitialized)",
        };

    /// <summary>Determines whether two validations are equal.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var left = Validation.From(Result.Ok(OrderFixtures.Sample));
    /// var right = Validation.From(Result.Ok(OrderFixtures.Sample));
    /// var equal = left == right;
    ///
    /// // Success(order) compares equal to another Success(order) with the same payload,
    /// // Failure([E_X]) compares equal to another Failure with the same error list.
    /// ]]></code>
    /// </remarks>
    public static bool operator ==(Validation<T, TErr> left, Validation<T, TErr> right) => left.Equals(right);

    /// <summary>Determines whether two validations differ.</summary>
    /// <remarks>
    /// <code><![CDATA[
    /// var success = Validation.From(Result.Ok(OrderFixtures.Sample));
    /// var failure = Validation.From(Result.Err<Order, RopError>(RopError.Create("E_ORDER", "missing")));
    /// var different = success != failure;
    ///
    /// // Success(order) differs from Failure([E_ORDER]),
    /// // Failure([E_X]) differs from another Failure([E_Y]) when the lists differ.
    /// ]]></code>
    /// </remarks>
    public static bool operator !=(Validation<T, TErr> left, Validation<T, TErr> right) => !left.Equals(right);
}
