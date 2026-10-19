using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using Microsoft.Net.Http.Headers;

namespace HaruyasumiRyokouki.Backend.Common.Helpers;

/// <summary>
/// Resolves the content language from an <c>Accept-Language</c> header.
/// </summary>
internal static class AcceptLanguageParser
{
	/// <summary>
	/// Returns the supported language with the highest preference in <paramref name="acceptLanguage"/>,
	/// or <see cref="LanguageCode.Default"/> if there is none.
	/// </summary>
	/// <param name="acceptLanguage">Header value, e.g. <c>ru</c> or <c>en-US,en;q=0.9,ja;q=0.8</c>.</param>
	/// <remarks>Regional variants are reduced to the language (<c>en-US</c> → <c>en</c>).</remarks>
	public static string Resolve(string? acceptLanguage)
	{
		if (string.IsNullOrWhiteSpace(acceptLanguage)
			|| !StringWithQualityHeaderValue.TryParseList([acceptLanguage], out var values))
		{
			return LanguageCode.Default;
		}

		// OrderByDescending is stable, so languages with equal quality keep the header order.
		var language = values
			.Where(v => v.Quality is not 0)
			.OrderByDescending(v => v.Quality ?? 1)
			.Select(v => v.Value.Value?.Split('-')[0].ToLowerInvariant())
			.FirstOrDefault(LanguageCode.IsSupported);

		return language ?? LanguageCode.Default;
	}
}
