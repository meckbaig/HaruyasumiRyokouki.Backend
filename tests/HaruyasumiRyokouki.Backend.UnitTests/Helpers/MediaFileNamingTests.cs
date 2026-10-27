using HaruyasumiRyokouki.Backend.Common.Helpers;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;

namespace HaruyasumiRyokouki.Backend.UnitTests.Helpers;

public class MediaFileNamingTests
{
	[Theory]
	[InlineData("photo.jpg")]
	[InlineData("PHOTO.JPG")]
	[InlineData("IMG_0001.HEIC")]
	[InlineData("image.webp")]
	public void IsImage_ImageExtension_AnyCase(string fileName)
	{
		Assert.True(MediaFileNaming.IsImage(fileName));
		Assert.False(MediaFileNaming.IsVideo(fileName));
	}

	[Theory]
	[InlineData("clip.mp4")]
	[InlineData("CLIP.MOV")]
	[InlineData("clip_web.Mp4")]
	public void IsVideo_VideoExtension_AnyCase(string fileName)
	{
		Assert.True(MediaFileNaming.IsVideo(fileName));
		Assert.False(MediaFileNaming.IsImage(fileName));
	}

	[Theory]
	[InlineData("photo.jpg", MediaType.Image)]
	[InlineData("clip.MOV", MediaType.Video)]
	[InlineData("notes.txt", MediaType.Unknown)]
	[InlineData("no_extension", MediaType.Unknown)]
	[InlineData(".jpg.txt", MediaType.Unknown)]
	public void GetMediaType_ByExtension(string fileName, MediaType expected)
	{
		Assert.Equal(expected, MediaFileNaming.GetMediaType(fileName));
	}

	[Fact]
	public void GetVideoWebName_AddsSuffixBeforeExtension()
	{
		Assert.Equal("clip_web.mp4", MediaFileNaming.GetVideoWebName("clip.mp4"));
	}

	[Fact]
	public void GetVideoWebName_AlreadyWebName_ReturnedAsIs()
	{
		Assert.Equal("clip_web.mp4", MediaFileNaming.GetVideoWebName("clip_web.mp4"));
	}

	[Fact]
	public void GetVideoPreviewName_ReplacesExtensionWithLowercasePreset()
	{
		Assert.Equal("clip_preview.webp", MediaFileNaming.GetVideoPreviewName("clip.MOV", "WebP"));
	}

	[Fact]
	public void GetVideoPreviewName_YoutubeId_AddsSuffixAndExtension()
	{
		Assert.Equal("dQw4w9WgXcQ_preview.webp", MediaFileNaming.GetVideoPreviewName("dQw4w9WgXcQ", "webp"));
	}

	[Fact]
	public void GetVideoPreviewName_AlreadyPreviewName_ReturnedAsIs()
	{
		Assert.Equal("clip_preview.webp", MediaFileNaming.GetVideoPreviewName("clip_preview.webp", "webp"));
	}
}
