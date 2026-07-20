namespace HaruyasumiRyokouki.Backend.Common.Exceptions;

/// <summary>
/// Exception that returns <see cref="StatusCodes.Status404NotFound"/>.
/// </summary>
internal class EntityNotFoundException : Exception
{
	public EntityNotFoundException(string message) : base(message)
	{
	}
}
