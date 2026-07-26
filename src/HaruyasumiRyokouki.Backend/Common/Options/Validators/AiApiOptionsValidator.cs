using Microsoft.Extensions.Options;
using System.Text;

namespace HaruyasumiRyokouki.Backend.Common.Options.Validators;

sealed class AiApiOptionsValidator : IValidateOptions<AiApiOptions>
{
	public ValidateOptionsResult Validate(string? name, AiApiOptions options)
	{
		if (options == null)
		{
			return ValidateOptionsResult.Fail(
				$"'{AiApiOptions.ConfigurationSectionName}' must not be null.");
		}

		var failures = new StringBuilder();

		if (string.IsNullOrWhiteSpace(options.ApiKey))
		{
			failures.AppendLine($"'{AiApiOptions.ConfigurationSectionName}:" +
				$"{nameof(AiApiOptions.ApiKey)}' cannot be null or empty.");
		}
		if (string.IsNullOrWhiteSpace(options.Model))
		{
			failures.AppendLine($"'{AiApiOptions.ConfigurationSectionName}:" +
				$"{nameof(AiApiOptions.Model)}' cannot be null or empty.");
		}
		if (options.Temperature < 0)
		{
			failures.AppendLine($"'{AiApiOptions.ConfigurationSectionName}:" +
				$"{nameof(AiApiOptions.Temperature)}' must be greater than 0.");
		}
		else if (options.Temperature > 1)
		{
			failures.AppendLine($"'{AiApiOptions.ConfigurationSectionName}:" +
				$"{nameof(AiApiOptions.Temperature)}' cannot be greater than 1 (100%).");
		}
		if (string.IsNullOrWhiteSpace(options.ApiUrl))
		{
			failures.AppendLine($"'{AiApiOptions.ConfigurationSectionName}:" +
				$"{nameof(AiApiOptions.ApiUrl)}' cannot be null or empty.");
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}

