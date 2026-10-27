namespace HaruyasumiRyokouki.Backend.Common.Exceptions;

/// <summary>
/// Exception that returns <see cref="StatusCodes.Status503ServiceUnavailable"/>: the feature is turned off in the configuration.
/// </summary>
internal class ServiceUnavailableException : Exception
{
	public ServiceUnavailableException(string message) : base(message)
	{
	}
}
