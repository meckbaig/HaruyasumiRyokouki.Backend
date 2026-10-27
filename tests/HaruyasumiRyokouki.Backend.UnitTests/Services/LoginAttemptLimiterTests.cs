using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

namespace HaruyasumiRyokouki.Backend.UnitTests.Services;

public class LoginAttemptLimiterTests
{
	private const int MaxAttempts = 3;
	private const int LockoutMinutes = 15;

	private readonly FakeClock _clock = new();
	private readonly LoginAttemptLimiter _limiter;

	public LoginAttemptLimiterTests()
	{
		var cache = new MemoryCache(new MemoryCacheOptions { Clock = _clock });
		_limiter = new LoginAttemptLimiter(cache, Options.Create(new ApplicationOptions
		{
			MaxFailedLoginAttempts = MaxAttempts,
			FailedLoginLockoutMinutes = LockoutMinutes
		}));
	}

	private void Fail(string key, int times)
	{
		for (int i = 0; i < times; i++)
			_limiter.RegisterFailure(key);
	}

	[Fact]
	public void BelowLimit_NotBlocked()
	{
		Fail("api:1.2.3.4", MaxAttempts - 1);

		Assert.False(_limiter.IsBlocked("api:1.2.3.4"));
	}

	[Fact]
	public void LimitReached_Blocked()
	{
		Fail("api:1.2.3.4", MaxAttempts);

		Assert.True(_limiter.IsBlocked("api:1.2.3.4"));
	}

	[Fact]
	public void Keys_AreCountedIndependently()
	{
		Fail("api:1.2.3.4", MaxAttempts);
		Fail("swagger:1.2.3.4", MaxAttempts - 1);

		Assert.False(_limiter.IsBlocked("api:5.6.7.8"));
		Assert.False(_limiter.IsBlocked("swagger:1.2.3.4"));
	}

	[Fact]
	public void Reset_ClearsFailures()
	{
		Fail("api:1.2.3.4", MaxAttempts - 1);
		_limiter.Reset("api:1.2.3.4");
		Fail("api:1.2.3.4", MaxAttempts - 1);

		Assert.False(_limiter.IsBlocked("api:1.2.3.4"));
	}

	[Fact]
	public void Lockout_LiftedWhenWindowExpires()
	{
		Fail("api:1.2.3.4", MaxAttempts);

		_clock.Advance(TimeSpan.FromMinutes(LockoutMinutes) - TimeSpan.FromSeconds(1));
		Assert.True(_limiter.IsBlocked("api:1.2.3.4"));

		_clock.Advance(TimeSpan.FromSeconds(2));
		Assert.False(_limiter.IsBlocked("api:1.2.3.4"));
	}

	[Fact]
	public void Window_StartsAtFirstFailure()
	{
		Fail("api:1.2.3.4", MaxAttempts - 1);
		_clock.Advance(TimeSpan.FromMinutes(LockoutMinutes + 1));
		Fail("api:1.2.3.4", 1);

		Assert.False(_limiter.IsBlocked("api:1.2.3.4"));
	}

	private sealed class FakeClock : ISystemClock
	{
		public DateTimeOffset UtcNow { get; private set; } = new(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);

		public void Advance(TimeSpan time) => UtcNow += time;
	}
}
