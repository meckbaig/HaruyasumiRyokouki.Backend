using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace HaruyasumiRyokouki.Backend.Common.Options.Validators;

sealed class ApplicationOptionsValidator : IValidateOptions<ApplicationOptions>
{
	public ValidateOptionsResult Validate(string? name, ApplicationOptions options)
	{
		var failures = new StringBuilder();

		if (options == null)
		{
			return ValidateOptionsResult.Fail($"'{ApplicationOptions.ConfigurationSectionName}' must not be null.");
		}

		if (options.CheckDbRetryCount <= 0)
		{
			failures.AppendLine($"'{ApplicationOptions.ConfigurationSectionName}:" +
				$"{nameof(ApplicationOptions.CheckDbRetryCount)}' must be greater than 0.");
		}
		if (options.CheckDbRetryDelay <= 0)
		{
			failures.AppendLine($"'{ApplicationOptions.ConfigurationSectionName}:" +
				$"{nameof(ApplicationOptions.CheckDbRetryDelay)}' must be greater than 0.");
		}
		if (string.IsNullOrWhiteSpace(options.AdminLogin))
		{
			failures.AppendLine($"'{ApplicationOptions.ConfigurationSectionName}:" +
				$"{nameof(ApplicationOptions.AdminLogin)}' cannot be null or empty.");
		}
		if (string.IsNullOrWhiteSpace(options.AdminPassword))
		{
			failures.AppendLine($"'{ApplicationOptions.ConfigurationSectionName}:" +
				$"{nameof(ApplicationOptions.AdminPassword)}' cannot be null or empty.");
		}
		if (options.MaxFailedLoginAttempts <= 0)
		{
			failures.AppendLine($"'{ApplicationOptions.ConfigurationSectionName}:" +
				$"{nameof(ApplicationOptions.MaxFailedLoginAttempts)}' must be greater than 0.");
		}
		if (options.FailedLoginLockoutMinutes <= 0)
		{
			failures.AppendLine($"'{ApplicationOptions.ConfigurationSectionName}:" +
				$"{nameof(ApplicationOptions.FailedLoginLockoutMinutes)}' must be greater than 0.");
		}
		if (options.KnownProxies == null)
		{
			failures.AppendLine($"'{ApplicationOptions.ConfigurationSectionName}:" +
				$"{nameof(ApplicationOptions.KnownProxies)}' must not be null.");
		}
		else
		{
			foreach (var proxy in options.KnownProxies)
			{
				if (!IPAddress.TryParse(proxy, out _))
				{
					failures.AppendLine($"'{ApplicationOptions.ConfigurationSectionName}:" +
						$"{nameof(ApplicationOptions.KnownProxies)}' contains not valid ip address ({proxy}).");
				}
			}
		}

		return failures.Length > 0
			? ValidateOptionsResult.Fail(failures.ToString())
			: ValidateOptionsResult.Success;
	}
}
