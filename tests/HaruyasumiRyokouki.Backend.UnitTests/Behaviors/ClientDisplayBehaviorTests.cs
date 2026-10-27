using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Behaviors;
using HaruyasumiRyokouki.Backend.Features.Media;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using Microsoft.AspNetCore.Http;

namespace HaruyasumiRyokouki.Backend.UnitTests.Behaviors;

public class ClientDisplayBehaviorTests
{
	private static async Task<GetFavoriteMediaQuery> SendAsync(string? header, bool withHttpContext = true)
	{
		var accessor = new HttpContextAccessor();
		if (withHttpContext)
		{
			accessor.HttpContext = new DefaultHttpContext();
			if (header != null)
				accessor.HttpContext.Request.Headers["X-Display"] = header;
		}

		var request = new GetFavoriteMediaQuery();
		var behavior = new ClientDisplayBehavior<GetFavoriteMediaQuery, GetFavoriteMediaResponse>(accessor, new ClientDisplayValidator());
		await behavior.Handle(request, _ => Task.FromResult(new GetFavoriteMediaResponse { Items = [] }), CancellationToken.None);
		return request;
	}

	[Fact]
	public async Task ValidHeader_ParsesDprAndMinSide()
	{
		var request = await SendAsync("dpr=1.25; min-side=864");

		Assert.Equal(1.25f, request.ClientDisplay!.Dpr);
		Assert.Equal(864, request.ClientDisplay.MinSide);
	}

	[Fact]
	public async Task KeysAreCaseInsensitive_UnknownKeysIgnored()
	{
		var request = await SendAsync("foo=bar;DPR=2;Min-Side=1080;broken");

		Assert.Equal(2f, request.ClientDisplay!.Dpr);
		Assert.Equal(1080, request.ClientDisplay.MinSide);
	}

	[Theory]
	[InlineData("dpr=abc; min-side=wide")]
	[InlineData("dpr=1,25; min-side=1.5")]
	[InlineData("dpr=; min-side=")]
	public async Task InvalidValues_AreSkipped(string header)
	{
		var request = await SendAsync(header);

		Assert.NotNull(request.ClientDisplay);
		Assert.Null(request.ClientDisplay.Dpr);
		Assert.Null(request.ClientDisplay.MinSide);
	}

	[Theory]
	[InlineData("dpr=-1")]
	[InlineData("dpr=0")]
	[InlineData("min-side=-864")]
	public async Task NonPositiveValues_ThrowValidationException(string header)
	{
		await Assert.ThrowsAsync<ValidationException>(() => SendAsync(header));
	}

	[Fact]
	public async Task NoHeader_ClientDisplayStaysNull()
	{
		Assert.Null((await SendAsync(null)).ClientDisplay);
	}

	[Fact]
	public async Task NoHttpContext_ClientDisplayStaysNull()
	{
		Assert.Null((await SendAsync("dpr=2", withHttpContext: false)).ClientDisplay);
	}
}
