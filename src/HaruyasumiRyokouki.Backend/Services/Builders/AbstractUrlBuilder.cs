using System.Text.RegularExpressions;

namespace HaruyasumiRyokouki.Backend.Services.Builders;

public abstract class AbstractUrlBuilder
{
	protected static readonly Regex TokenRegex = new(@"\{(?<token>[^\}]+)\}", RegexOptions.Compiled);
	public abstract HashSet<string> RequiredTokens { get; }

	protected readonly string _template;

	protected AbstractUrlBuilder(string template)
	{
		_template = template;
	}

	public virtual bool Validate(out IReadOnlyCollection<string> missingTokens)
	{
		var tokens = TokenRegex
			.Matches(_template)
			.Select(x => x.Groups["token"].Value)
			.ToHashSet();

		missingTokens = RequiredTokens
			.Where(x => !tokens.Contains(x))
			.ToArray();

		return missingTokens.Count == 0;
	}
}
