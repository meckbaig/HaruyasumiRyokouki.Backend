using HaruyasumiRyokouki.Backend.Services.Builders;

namespace HaruyasumiRyokouki.Backend.UnitTests.Services;

public class UrlBuilderTests
{
	private const string Endpoint = "https://cloud.example.com/";
	private const string Cdn = "https://cdn.example.com";
	private const string OriginPayload = "/public.php/dav/files/share/{fileName}";
	private const string PreviewPayload = "s/share/preview?file=/{fileName}&x={xAxis}&y={yAxis}";

	[Fact]
	public void Origin_BuildMedia_JoinsEndpointAndPayloadWithSingleSlash()
	{
		var builder = new OriginStorageUrlBuilder(Endpoint, null, OriginPayload);

		Assert.Equal("https://cloud.example.com/public.php/dav/files/share/clip.mp4", builder.BuildMedia("clip.mp4"));
	}

	[Theory]
	[InlineData("IMG 0001.jpg", "IMG%200001.jpg")]
	[InlineData("фото.jpg", "%D1%84%D0%BE%D1%82%D0%BE.jpg")]
	[InlineData("a&b#c.jpg", "a%26b%23c.jpg")]
	public void Origin_FileName_IsEscaped(string fileName, string escaped)
	{
		var builder = new OriginStorageUrlBuilder(Endpoint, null, OriginPayload);

		Assert.Equal($"https://cloud.example.com/public.php/dav/files/share/{escaped}", builder.BuildMedia(fileName));
	}

	[Fact]
	public void Cdn_UsedForMedia_DownloadsUseEndpointByDefault()
	{
		var builder = new OriginStorageUrlBuilder(Endpoint, Cdn, OriginPayload);

		Assert.StartsWith("https://cdn.example.com/", builder.BuildMedia("a.jpg"));
		Assert.StartsWith("https://cloud.example.com/", builder.BuildDownload("a.jpg"));
	}

	[Fact]
	public void Cdn_UsedForDownloadsWhenEnabled()
	{
		var builder = new OriginStorageUrlBuilder(Endpoint, Cdn, OriginPayload, useCdnForDownloads: true);

		Assert.StartsWith("https://cdn.example.com/", builder.BuildDownload("a.jpg"));
	}

	[Fact]
	public void NoCdn_UseCdnForDownloadsIgnored()
	{
		var builder = new OriginStorageUrlBuilder(Endpoint, "", OriginPayload, useCdnForDownloads: true);

		Assert.StartsWith("https://cloud.example.com/", builder.BuildMedia("a.jpg"));
		Assert.StartsWith("https://cloud.example.com/", builder.BuildDownload("a.jpg"));
	}

	[Fact]
	public void Nextcloud_ReplacesAllTokens()
	{
		var builder = new NextcloudPreviewUrlBuilder(Endpoint, Cdn, PreviewPayload);

		Assert.Equal("https://cdn.example.com/s/share/preview?file=/IMG%201.jpg&x=1200&y=800", builder.BuildMedia("IMG 1.jpg", 1200, 800));
		Assert.Equal("https://cloud.example.com/s/share/preview?file=/IMG%201.jpg&x=400&y=300", builder.BuildDownload("IMG 1.jpg", 400, 300));
	}

	[Fact]
	public void Validate_AllTokensPresent_ReturnsTrue()
	{
		Assert.True(new OriginStorageUrlBuilder(Endpoint, null, OriginPayload).Validate(out var originMissing));
		Assert.True(new NextcloudPreviewUrlBuilder(Endpoint, null, PreviewPayload).Validate(out var previewMissing));
		Assert.Empty(originMissing);
		Assert.Empty(previewMissing);
	}

	[Fact]
	public void Validate_MissingTokens_ReturnsThem()
	{
		var builder = new NextcloudPreviewUrlBuilder(Endpoint, null, "preview?file={fileName}&x={xAxis}");

		Assert.False(builder.Validate(out var missing));
		Assert.Equal(["yAxis"], missing);
	}

	[Fact]
	public void Validate_OriginWithoutFileName_Fails()
	{
		Assert.False(new OriginStorageUrlBuilder(Endpoint, null, "/files/").Validate(out var missing));
		Assert.Equal(["fileName"], missing);
	}
}
