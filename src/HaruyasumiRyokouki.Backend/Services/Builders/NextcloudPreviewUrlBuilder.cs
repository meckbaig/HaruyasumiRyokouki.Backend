namespace HaruyasumiRyokouki.Backend.Services.Builders;

public class NextcloudPreviewUrlBuilder : AbstractUrlBuilder
{
	public override HashSet<string> RequiredTokens =>
	[
		"fileName",
		"xAxis",
		"yAxis"
	];

	public NextcloudPreviewUrlBuilder(string template) : base(template)
	{
	}

	public string Build(string fileName, int xAxis, int yAxis)
	{
		return _template
			.Replace("{fileName}", Uri.EscapeDataString(fileName))
			.Replace("{xAxis}", xAxis.ToString())
			.Replace("{yAxis}", yAxis.ToString());
	}
}
