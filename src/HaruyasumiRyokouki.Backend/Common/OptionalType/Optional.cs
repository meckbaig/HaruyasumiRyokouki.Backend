using HaruyasumiRyokouki.Backend.Common.OptionalType.Supporting.Swagger;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Common.OptionalType;

/// <summary>
/// Wrapper for optional parameters.
/// </summary>
/// <typeparam name="T">The original parameter type.</typeparam>
/// <remarks>
/// Used to distinguish between "parameter not provided" and "parameter provided with a null value".
/// </remarks>
[JsonConverter(typeof(OptionalJsonConverterFactory))]
public readonly struct Optional<T>
{
	/// <summary>
	/// <see langword="true"/> if the parameter value was provided (even if it is <see langword="null"/>).
	/// </summary>
	internal bool HasValue { get; }

	/// <summary>
	/// The parameter value, if it was provided; otherwise <see langword="null"/>.
	/// </summary>
	[MemberNotNullWhen(true, nameof(HasValue))]
	internal T? Value { get; }

	/// <summary>
	/// Initializes a new instance of <see cref="Optional{T}"/> containing the specified value.
	/// </summary>
	/// <param name="value">The parameter value.</param>
	public Optional(T? value)
	{
		Value = value;
		HasValue = true;
	}

	/// <summary>
	/// Implicit conversion from <typeparamref name="T"/> to <see cref="Optional{T}"/>.
	/// </summary>
	/// <param name="value">The value to wrap.</param>
	public static implicit operator Optional<T>(T? value) => new(value);

	/// <summary>
	/// Implicit conversion from <see cref="Optional{T}"/> to <typeparamref name="T"/>.
	/// </summary>
	/// <param name="value">The optional value.</param>
	public static implicit operator T?(Optional<T> value) => value.Value;

	public override string ToString()
	{
		if (!HasValue)
			return "no value";
		if (Value?.GetType() == typeof(string))
			return $"\"{Value}\"";
		return Value?.ToString() ?? "null";
	}
}
