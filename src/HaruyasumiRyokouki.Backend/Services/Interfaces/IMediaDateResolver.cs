using HaruyasumiRyokouki.Backend.Common.ResultType;

namespace HaruyasumiRyokouki.Backend.Services.Interfaces;

internal interface IMediaDateResolver
{
	DateTime? TryExtractLocalFromName(string fileName);
	Result<DateTime> ResolveFromUtc(DateTime utcDate);
}
