namespace HaruyasumiRyokouki.Backend.Common.Exceptions;

/// <summary>
/// Exception that returns <see cref="StatusCodes.Status403Forbidden"/>.
/// </summary>
internal class ForbiddenAccessException : Exception
{
	public ForbiddenAccessException(string message) : base(message)
	{
	}
}
