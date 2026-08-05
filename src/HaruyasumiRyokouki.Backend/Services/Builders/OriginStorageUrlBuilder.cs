namespace HaruyasumiRyokouki.Backend.Services.Builders;

public class OriginStorageUrlBuilder : AbstractUrlBuilder
{
	public override HashSet<string> RequiredTokens =>
	[
		"fileName"
	];

	public OriginStorageUrlBuilder(string template) : base(template)
	{
	}

	public string Build(string fileName)
	{
		return _template.Replace("{fileName}", Uri.EscapeDataString(fileName));
	}
}
