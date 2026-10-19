using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.Services;

/// <inheritdoc/>
internal class LoginAttemptLimiter : ILoginAttemptLimiter
{
	private const string CacheKeyPrefix = "failed-logins:";

	private readonly IMemoryCache _cache;
	private readonly ApplicationOptions _options;
	private readonly object _lock = new();

	public LoginAttemptLimiter(IMemoryCache cache, IOptions<ApplicationOptions> options)
	{
		_cache = cache;
		_options = options.Value;
	}

	/// <inheritdoc/>
	public bool IsBlocked(string clientKey)
	{
		return _cache.TryGetValue(CacheKeyPrefix + clientKey, out FailedAttempts? attempts)
			&& attempts!.Count >= _options.MaxFailedLoginAttempts;
	}

	/// <inheritdoc/>
	public void RegisterFailure(string clientKey)
	{
		lock (_lock)
		{
			// The entry expires a fixed time after the first failure, which both bounds
			// the counting window and lifts the lockout.
			var attempts = _cache.GetOrCreate(CacheKeyPrefix + clientKey, entry =>
			{
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.FailedLoginLockoutMinutes);
				return new FailedAttempts();
			})!;
			attempts.Count++;
		}
	}

	/// <inheritdoc/>
	public void Reset(string clientKey)
	{
		_cache.Remove(CacheKeyPrefix + clientKey);
	}

	private sealed class FailedAttempts
	{
		public int Count;
	}
}
