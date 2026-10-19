namespace HaruyasumiRyokouki.Backend.Common.Exceptions;

/// <summary>
/// Exception that returns <see cref="StatusCodes.Status502BadGateway"/>:
/// an external service or tool (yt-dlp, ffmpeg, remote HTTP resource) failed,
/// as opposed to invalid input from the client.
/// </summary>
internal class ExternalServiceException : Exception
{
	public ExternalServiceException(string message) : base(message)
	{
	}
}
