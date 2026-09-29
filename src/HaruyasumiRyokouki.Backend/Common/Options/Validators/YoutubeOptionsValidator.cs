using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.Common.Options.Validators;

sealed class YoutubeOptionsValidator : IValidateOptions<YoutubeOptions>
{
	public ValidateOptionsResult Validate(string? name, YoutubeOptions options)
	{
		if (options?.Proxy is not { Enabled: true })
			return ValidateOptionsResult.Success;

		if (string.IsNullOrWhiteSpace(options.Proxy.Address))
		{
			return ValidateOptionsResult.Fail($"'{YoutubeOptions.ConfigurationSectionName}:Proxy:" +
				$"{nameof(YoutubeOptions.YoutubeProxyOptions.Address)}' must not be empty when proxy is enabled.");
		}

		return ValidateOptionsResult.Success;
	}
}
