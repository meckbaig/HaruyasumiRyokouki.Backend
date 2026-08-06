using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using System.Text.RegularExpressions;

namespace HaruyasumiRyokouki.Backend.Common.Filters;

public class HeaderParameterFilter : IParameterFilter
{
	public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
	{
		if (parameter.In != ParameterLocation.Header)
			return;
		parameter.Name = PascalCaseToHeader(parameter.Name);
	}

	private static readonly Regex WordRegex = new(@"[A-Z]+(?![a-z])|[A-Z][a-z]*|\d+", RegexOptions.Compiled);

	public static string PascalCaseToHeader(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return value;

		var matches = WordRegex.Matches(value);

		var sb = new StringBuilder();

		for (int i = 0; i < matches.Count; i++)
		{
			if (i > 0)
				sb.Append('-');

			var word = matches[i].Value;
			sb.Append(char.ToUpperInvariant(word[0]));

			if (word.Length > 1)
				sb.Append(word.Substring(1).ToLowerInvariant());
		}

		return sb.ToString();
	}
}
