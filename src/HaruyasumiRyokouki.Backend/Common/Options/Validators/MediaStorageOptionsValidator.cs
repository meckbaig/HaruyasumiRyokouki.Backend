using Microsoft.Extensions.Options;
using System.Text;
using static HaruyasumiRyokouki.Backend.Common.Options.MediaStorageOptions;

namespace HaruyasumiRyokouki.Backend.Common.Options.Validators;

sealed class MediaStorageOptionsValidator : IValidateOptions<MediaStorageOptions>
{
	public ValidateOptionsResult Validate(string? name, MediaStorageOptions options)
	{
		var failures = new StringBuilder();

		if (options == null)
		{
			return ValidateOptionsResult.Fail($"'{MediaStorageOptions.ConfigurationSectionName}' must not be null.");
		}

		if (!Enum.IsDefined(options.Provider))
		{
			failures.AppendLine($"'{MediaStorageOptions.ConfigurationSectionName}:" +
				$"{nameof(MediaStorageOptions.Provider)}' is not valid. Available options: {string.Join(", ", Enum.GetNames(typeof(FileStorageProvider)))}");
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
