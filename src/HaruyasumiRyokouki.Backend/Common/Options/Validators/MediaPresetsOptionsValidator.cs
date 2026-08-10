using Microsoft.Extensions.Options;
using System.Text;

namespace HaruyasumiRyokouki.Backend.Common.Options.Validators;

sealed class MediaPresetsOptionsValidator : IValidateOptions<MediaPresetsOptions>
{
	public ValidateOptionsResult Validate(string? name, MediaPresetsOptions options)
	{
		var failures = new StringBuilder();

		if (options == null)
		{
			return ValidateOptionsResult.Fail($"'{MediaPresetsOptions.ConfigurationSectionName}' must not be null.");
		}

		if (options.Video.Count == 0)
		{
			failures.AppendLine($"'{MediaPresetsOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaPresetsOptions.Video)}' must have a least 1 preset.");
		}
		if (options.Image.Count == 0)
		{
			failures.AppendLine($"'{MediaPresetsOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaPresetsOptions.Image)}' must have a least 1 preset.");
		}
		if (options.VideoThumbnailPrefix.Count == 0)
		{
			failures.AppendLine($"'{MediaPresetsOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaPresetsOptions.VideoThumbnailPrefix)}' must have a least 1 preset.");
		}
		if (options.Miniature.Count == 0)
		{
			failures.AppendLine($"'{MediaPresetsOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaPresetsOptions.Miniature)}' must have a least 1 preset.");
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
