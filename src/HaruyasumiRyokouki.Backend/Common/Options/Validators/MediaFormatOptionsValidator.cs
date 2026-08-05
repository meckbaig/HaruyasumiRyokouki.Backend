using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;
using Microsoft.Extensions.Options;
using System;
using System.Text;

namespace HaruyasumiRyokouki.Backend.Common.Options.Validators;

sealed class MediaFormatOptionsValidator : IValidateOptions<MediaFormatOptions>
{
	public ValidateOptionsResult Validate(string? name, MediaFormatOptions options)
	{
		var failures = new StringBuilder();

		if (options == null)
		{
			return ValidateOptionsResult.Fail($"'{MediaFormatOptions.ConfigurationSectionName}' must not be null.");
		}

		if (!Enum.IsDefined(options.TargetImagePreset))
		{
			failures.AppendLine($"'{MediaFormatOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaFormatOptions.TargetImagePreset)}' is not valid. Available options: {string.Join(", ", Enum.GetNames(typeof(FfmpegImagePreset)))}");
		}
		if (!Enum.IsDefined(options.TargetVideoPreset))
		{
			failures.AppendLine($"'{MediaFormatOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaFormatOptions.TargetImagePreset)}' is not valid. Available options: {string.Join(", ", Enum.GetNames(typeof(FfmpegVideoPreset)))}");
		}
		if (options.PreviewSize <= 0)
		{
			failures.AppendLine($"'{MediaFormatOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaFormatOptions.PreviewSize)}' must be greater than 0.");
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
