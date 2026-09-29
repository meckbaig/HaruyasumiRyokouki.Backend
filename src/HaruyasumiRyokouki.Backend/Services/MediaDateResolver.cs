using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Common.ResultType;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.RegularExpressions;

namespace HaruyasumiRyokouki.Backend.Services;

internal class MediaDateResolver : IMediaDateResolver
{
	private static readonly Regex DateTimeTokenRegex = new(
		@"(?<y>\d{4})[-_.]?(?<mo>\d{2})[-_.]?(?<d>\d{2})[-_ .T]?(?<h>\d{2})[-_.:]?(?<mi>\d{2})[-_.:]?(?<s>\d{2})",
		RegexOptions.Compiled);

	private readonly MediaFormatOptions _options;

	public MediaDateResolver(IOptions<MediaFormatOptions> options)
	{
		_options = options.Value;
	}

	public DateTime? TryExtractLocalFromName(string fileName)
	{
		var name = Path.GetFileNameWithoutExtension(fileName);
		var match = DateTimeTokenRegex.Match(name);
		if (!match.Success)
			return null;

		var value = string.Concat(
			match.Groups["y"].Value,
			match.Groups["mo"].Value,
			match.Groups["d"].Value,
			match.Groups["h"].Value,
			match.Groups["mi"].Value,
			match.Groups["s"].Value);

		if (!DateTime.TryParseExact(
				value,
				"yyyyMMddHHmmss",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out var date))
		{
			return null;
		}

		return DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
	}

	public Result<DateTime> ResolveFromUtc(DateTime utcDate)
	{
		var utc = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
		var range = _options.TravelDateOffsets
			.FirstOrDefault(r => (!r.From.HasValue || utc >= r.From.Value) && (!r.To.HasValue || utc <= r.To.Value));

		if (range is null)
			return Result<DateTime>.Failure($"No travel date offset range configured for {utc:O}.");

		var local = utc.AddHours(range.Offset);
		return Result<DateTime>.Success(DateTime.SpecifyKind(local, DateTimeKind.Unspecified));
	}
}
