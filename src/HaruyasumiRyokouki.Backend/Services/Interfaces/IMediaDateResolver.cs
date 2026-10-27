using HaruyasumiRyokouki.Backend.Common.ResultType;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

/// <summary>
/// Creation time of media in the local time of the trip.
/// </summary>
internal interface IMediaDateResolver
{
	/// <summary>
	/// Reads a timestamp from a camera-style file name, e.g. <c>IMG_20260301_120000.jpg</c>.
	/// </summary>
	/// <returns>Local time from the name, or <see langword="null"/> when the name has no valid timestamp.</returns>
	DateTime? TryExtractLocalFromName(string fileName);

	/// <summary>
	/// Converts a UTC time to the local time of the trip with the offset of the matching <c>MediaFormat:TravelDateOffsets</c> range.
	/// </summary>
	/// <returns>Local time, or a failure when no range contains the time.</returns>
	Result<DateTime> ResolveFromUtc(DateTime utcDate);
}
