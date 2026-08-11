using Microsoft.Extensions.Options;
using System.Text;

namespace HaruyasumiRyokouki.Backend.Common.Options.Validators;

sealed class MediaSizesOptionsValidator : IValidateOptions<MediaSizesOptions>
{
	public ValidateOptionsResult Validate(string? name, MediaSizesOptions options)
	{
		var failures = new StringBuilder();

		if (options == null)
		{
			return ValidateOptionsResult.Fail($"'{MediaSizesOptions.ConfigurationSectionName}' must not be null.");
		}

		if (options.SizeBuckets.Count < 2)
		{
			failures.AppendLine($"'{MediaSizesOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaSizesOptions.SizeBuckets)}' must have at least 2 image sizes.");
		}
		else if (options.SizeBuckets.Any(x => x <= 0))
		{
			failures.AppendLine($"'{MediaSizesOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaSizesOptions.SizeBuckets)}' must have values greater than 0.");
		}
		if (options.PreviewTargetCss <= 0)
		{
			failures.AppendLine($"'{MediaSizesOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaSizesOptions.PreviewTargetCss)}' must be greater than 0.");
		}
		if (options.DefaultScreenResolution <= 0)
		{
			failures.AppendLine($"'{MediaSizesOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaSizesOptions.DefaultScreenResolution)}' must be greater than 0.");
		}
		if (options.DefaultAspectRatio <= 0)
		{
			failures.AppendLine($"'{MediaSizesOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaSizesOptions.DefaultAspectRatio)}' must be greater than 0.");
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
