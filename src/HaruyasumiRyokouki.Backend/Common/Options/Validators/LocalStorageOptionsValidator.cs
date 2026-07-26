using Microsoft.Extensions.Options;
using System.Text;

namespace HaruyasumiRyokouki.Backend.Common.Options.Validators;

sealed class LocalStorageOptionsValidator : IValidateOptions<LocalStorageOptions>
{
	public ValidateOptionsResult Validate(string? name, LocalStorageOptions options)
	{
		var failures = new StringBuilder();

		if (options == null)
		{
			return ValidateOptionsResult.Fail($"'{LocalStorageOptions.ConfigurationSectionName}' must not be null.");
		}

		if (string.IsNullOrWhiteSpace(options.Path))
		{
			failures.AppendLine($"'{LocalStorageOptions.ConfigurationSectionName}:" +
				$"{nameof(LocalStorageOptions.Path)}' cannot be null or empty.");
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
