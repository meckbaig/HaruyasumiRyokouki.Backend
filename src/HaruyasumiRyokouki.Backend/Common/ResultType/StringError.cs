using Meckbaig.Result.Base;

namespace HaruyasumiRyokouki.Backend.Common.ResultType;

/// <summary>
/// Error of a <see cref="Result{TValue}"/>: a message.
/// </summary>
public sealed class StringError : IError
{
	/// <summary>
	/// Error message.
	/// </summary>
	public string Message { get; }

	/// <summary>
	/// Creates an error with the message.
	/// </summary>
	public StringError(string message)
	{
		Message = message;
	}

	/// <summary>
	/// The message.
	/// </summary>
	public override string ToString() => Message;

	/// <summary>
	/// Creates an error from a message.
	/// </summary>
	public static implicit operator StringError(string value) => new(value);

	/// <summary>
	/// Returns the message.
	/// </summary>
	public static implicit operator string(StringError value) => value.ToString();
}
