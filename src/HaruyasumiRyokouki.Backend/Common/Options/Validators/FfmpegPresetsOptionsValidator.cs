using Microsoft.Extensions.Options;
using System.Text;

namespace HaruyasumiRyokouki.Backend.Common.Options.Validators;

sealed class FfmpegPresetsOptionsValidator : IValidateOptions<FfmpegPresetsOptions>
{
	public ValidateOptionsResult Validate(string? name, FfmpegPresetsOptions options)
	{
		var failures = new StringBuilder();

		if (options == null)
		{
			return ValidateOptionsResult.Fail($"'{FfmpegPresetsOptions.ConfigurationSectionName}' must not be null.");
		}

		if (options.Video.Count == 0)
		{
			failures.AppendLine($"'{FfmpegPresetsOptions.ConfigurationSectionName}:" +
				$"{nameof(FfmpegPresetsOptions.Video)}' must have a least 1 preset.");
		}
		if (options.Image.Count == 0)
		{
			failures.AppendLine($"'{FfmpegPresetsOptions.ConfigurationSectionName}:" +
				$"{nameof(FfmpegPresetsOptions.Image)}' must have a least 1 preset.");
		}
		if (options.VideoThumbnailPrefix.Count == 0)
		{
			failures.AppendLine($"'{FfmpegPresetsOptions.ConfigurationSectionName}:" +
				$"{nameof(FfmpegPresetsOptions.VideoThumbnailPrefix)}' must have a least 1 preset.");
		}
		if (options.Miniature.Count == 0)
		{
			failures.AppendLine($"'{FfmpegPresetsOptions.ConfigurationSectionName}:" +
				$"{nameof(FfmpegPresetsOptions.Miniature)}' must have a least 1 preset.");
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
