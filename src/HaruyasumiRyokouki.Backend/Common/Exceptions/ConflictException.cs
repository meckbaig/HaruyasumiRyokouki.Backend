namespace HaruyasumiRyokouki.Backend.Common.Exceptions;

/// <summary>
/// Exception that returns <see cref="StatusCodes.Status409Conflict"/>.
/// </summary>
internal class ConflictException : Exception
{
	public ConflictException(string message) : base(message)
	{
	}
}
