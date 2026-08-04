using Meckbaig.Result.Base;

namespace HaruyasumiRyokouki.Backend.Common.ResultType;

public sealed class StringError : IError
{
	public string Message { get; }

	public StringError(string message)
	{
		Message = message;
	}

	public override string ToString() => Message;

	public static implicit operator StringError(string value) => new(value);
	public static implicit operator string(StringError value) => value.ToString();
}
