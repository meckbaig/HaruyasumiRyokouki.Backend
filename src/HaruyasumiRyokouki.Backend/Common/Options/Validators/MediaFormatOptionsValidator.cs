using Microsoft.Extensions.Options;
using System.Text;

namespace HaruyasumiRyokouki.Backend.Common.Options.Validators;

sealed class MediaFormatOptionsValidator : IValidateOptions<MediaFormatOptions>
{
	private readonly FfmpegPresetsOptions _presetsOptions;
	public MediaFormatOptionsValidator(IOptions<FfmpegPresetsOptions> presetsOptions)
	{
		_presetsOptions = presetsOptions.Value;
	}

	public ValidateOptionsResult Validate(string? name, MediaFormatOptions options)
	{
		var failures = new StringBuilder();

		if (options == null)
		{
			return ValidateOptionsResult.Fail($"'{MediaFormatOptions.ConfigurationSectionName}' must not be null.");
		}

		if (!_presetsOptions.Video.ContainsKey(options.VideoPreset))
		{
			failures.AppendLine($"'{MediaFormatOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaFormatOptions.VideoPreset)}' must be valid key from {FfmpegPresetsOptions.ConfigurationSectionName}.{nameof(_presetsOptions.Video)}.");
		}
		if (!_presetsOptions.VideoThumbnailPrefix.ContainsKey(options.VideoThumbnailPreset))
		{
			failures.AppendLine($"'{MediaFormatOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaFormatOptions.VideoThumbnailPreset)}' must be valid key from {FfmpegPresetsOptions.ConfigurationSectionName}.{nameof(_presetsOptions.VideoThumbnailPrefix)}.");
		}
		if (!_presetsOptions.Image.ContainsKey(options.ImagePreset))
		{
			failures.AppendLine($"'{MediaFormatOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaFormatOptions.ImagePreset)}' must be valid key from {FfmpegPresetsOptions.ConfigurationSectionName}.{nameof(_presetsOptions.Image)}.");
		}
		if (!_presetsOptions.Miniature.ContainsKey(options.MiniaturePreset))
		{
			failures.AppendLine($"'{MediaFormatOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaFormatOptions.MiniaturePreset)}' must be valid key from {FfmpegPresetsOptions.ConfigurationSectionName}.{nameof(_presetsOptions.Miniature)}.");
		}
		if (options.MiniatureSize <= 0)
		{
			failures.AppendLine($"'{MediaFormatOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaFormatOptions.MiniatureSize)}' must be greater than 0.");
		}
		if (options.FavoritesReturnCount <= 0)
		{
			failures.AppendLine($"'{MediaFormatOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaFormatOptions.FavoritesReturnCount)}' must be greater than 0.");
		}
		if (options.TravelDateOffsets == null || options.TravelDateOffsets.Count == 0)
		{
			failures.AppendLine($"'{MediaFormatOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaFormatOptions.TravelDateOffsets)}' must contain at least one range.");
		}
		else
		{
			var ordered = options.TravelDateOffsets.OrderBy(o => o.From).ToList();
			for (int i = 0; i < ordered.Count; i++)
			{
				if (ordered[i].From.HasValue && ordered[i].To.HasValue && ordered[i].From > ordered[i].To)
				{
					failures.AppendLine($"'{MediaFormatOptions.ConfigurationSectionName}:" +
						$"{nameof(MediaFormatOptions.TravelDateOffsets)}[{i}]' has From greater than To.");
				}
				if (i > 0)
				{
					if (!ordered[i - 1].To.HasValue)
					{
						failures.AppendLine($"'{MediaFormatOptions.ConfigurationSectionName}:" +
							$"{nameof(MediaFormatOptions.TravelDateOffsets)}[{i - 1}]' is open-ended and overlaps with the following range.");
					}
					else if (ordered[i].From.HasValue && ordered[i].From <= ordered[i - 1].To)
					{
						failures.AppendLine($"'{MediaFormatOptions.ConfigurationSectionName}:" +
							$"{nameof(MediaFormatOptions.TravelDateOffsets)}[{i}]' overlaps with the previous range.");
					}
				}
			}
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
