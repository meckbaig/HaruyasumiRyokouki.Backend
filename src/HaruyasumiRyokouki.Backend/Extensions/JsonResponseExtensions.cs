using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HaruyasumiRyokouki.Backend.Extensions;

internal static class JsonResponseExtensions
{
	internal static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
		PropertyNameCaseInsensitive = true
	};

	public static ContentResult ToJsonResponse<TResponse>(this TResponse response) where TResponse : class
	{
		var result = new ContentResult();
		result.Content = JsonSerializer.Serialize(response, SerializerOptions);

		if (!string.IsNullOrEmpty(result.Content) && result.Content != "{}")
		{
			result.ContentType = "application/json";
			return result;
		}

		result.ContentType = null;
		result.Content = null;
		return result;
	}
}
