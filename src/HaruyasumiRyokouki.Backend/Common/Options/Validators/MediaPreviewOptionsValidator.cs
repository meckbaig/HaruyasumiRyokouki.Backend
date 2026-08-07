using HaruyasumiRyokouki.Backend.Services.Builders;
using Microsoft.Extensions.Options;
using System.Text;
using static HaruyasumiRyokouki.Backend.Common.Options.MediaPreviewOptions;

namespace HaruyasumiRyokouki.Backend.Common.Options.Validators;

sealed class MediaPreviewOptionsValidator : IValidateOptions<MediaPreviewOptions>
{
	public ValidateOptionsResult Validate(string? name, MediaPreviewOptions options)
	{
		var failures = new StringBuilder();

		if (options == null)
		{
			return ValidateOptionsResult.Fail($"'{MediaPreviewOptions.ConfigurationSectionName}' must not be null.");
		}

		if (!Enum.IsDefined(options.Provider))
		{
			failures.AppendLine($"'{MediaPreviewOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaPreviewOptions.Provider)}' is not valid. Available options: {string.Join(", ", Enum.GetNames(typeof(MediaPreviewProvider)))}");
		}
		if (string.IsNullOrWhiteSpace(options.OriginStorageBase))
		{
			failures.AppendLine($"'{MediaPreviewOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaPreviewOptions.OriginStorageBase)}' cannot be null or empty.");
		}
		switch (options.Provider)
		{
			case MediaPreviewProvider.Nextcloud:
				if (!ValidateNextcloudOptions(options.Nextcloud, out string nextcloudErrors))
					failures.AppendLine(nextcloudErrors);
				break;
			case MediaPreviewProvider.Imgproxy:
				if (!ValidateImgproxyOptions(options.Imgproxy, out string imgproxyErrors))
					failures.AppendLine(imgproxyErrors);
				break;
			default:
				break;
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}

	private bool ValidateNextcloudOptions(NextcloudOptions options, out string errors)
	{
		var failures = new StringBuilder();
		if (string.IsNullOrWhiteSpace(options.PublicPreviewBase))
		{
			failures.AppendLine($"'{MediaPreviewOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaPreviewOptions.Nextcloud)}." +
				$"{nameof(NextcloudOptions.PublicPreviewBase)}' cannot be null or empty.");
		}
		else
		{
			var builder = new NextcloudPreviewUrlBuilder(options.PublicPreviewBase);
			if (!builder.Validate(out var missingTokens))
				failures.AppendLine($"'{MediaPreviewOptions.ConfigurationSectionName}:" +
					$"{nameof(MediaPreviewOptions.Nextcloud)}." +
					$"{nameof(NextcloudOptions.PublicPreviewBase)}' is missing tokens: {string.Join(',', missingTokens)}.");
		}
		errors = failures.ToString();
		return string.IsNullOrEmpty(errors);
	}

	private bool ValidateImgproxyOptions(ImgproxyOptions options, out string errors)
	{
		var failures = new StringBuilder();
		if (string.IsNullOrWhiteSpace(options.Endpoint))
		{
			failures.AppendLine($"'{MediaPreviewOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaPreviewOptions.Imgproxy)}." +
				$"{nameof(ImgproxyOptions.Endpoint)}' cannot be null or empty.");
		}
		if (string.IsNullOrWhiteSpace(options.FilePath))
		{
			failures.AppendLine($"'{MediaPreviewOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaPreviewOptions.Imgproxy)}." +
				$"{nameof(ImgproxyOptions.FilePath)}' cannot be null or empty.");
		}
		if (!options.Insecure)
		{
			if (string.IsNullOrWhiteSpace(options.Key))
			{
				failures.AppendLine($"'{MediaPreviewOptions.ConfigurationSectionName}:" +
					$"{nameof(MediaPreviewOptions.Imgproxy)}." +
					$"{nameof(ImgproxyOptions.Key)}' cannot be null or empty.");
			}
			if (string.IsNullOrWhiteSpace(options.Salt))
			{
				failures.AppendLine($"'{MediaPreviewOptions.ConfigurationSectionName}:" +
					$"{nameof(MediaPreviewOptions.Imgproxy)}." +
					$"{nameof(ImgproxyOptions.Salt)}' cannot be null or empty.");
			}
		}
		errors = failures.ToString();
		return string.IsNullOrEmpty(errors);
	}
}
