using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;
using HaruyasumiRyokouki.Backend.Services.Interfaces;

namespace HaruyasumiRyokouki.Backend.Services;

internal class MediaUrlsProvider : IMediaUrlsProvider
{
	private readonly IMediaPreviewService _previewService;

	public MediaUrlsProvider(IMediaPreviewService previewService)
	{
		_previewService = previewService;
	}

	public string GetImageUrl(string fileName, ImageUrlType linkType, ClientDisplay? clientDisplay = null, float? aspectRatio = default)
	{
		return _previewService.GetImageUrl(fileName, linkType, clientDisplay, aspectRatio);
	}

	public VideoUrlsDto GetVideoUrls(string fileName, MediaSource source, string? externalUrl, ICollection<string> additionalFiles, ClientDisplay? clientDisplay = null, float? aspectRatio = default)
	{
		string preview = _previewService.GetVideoUrl(fileName, VideoUrlType.Preview, additionalFiles, clientDisplay, aspectRatio);

        switch (source)
        {
            case MediaSource.YouTube:
                return new VideoUrlsDto
                {
                    Download = null,
                    Stream = externalUrl,
                    Preview = preview
                };
            case MediaSource.Local:
                return new VideoUrlsDto
                {
                    Download = _previewService.GetVideoUrl(fileName, VideoUrlType.Download, additionalFiles, clientDisplay, aspectRatio),
                    Stream = _previewService.GetVideoUrl(fileName, VideoUrlType.Stream, additionalFiles, clientDisplay, aspectRatio),
                    Preview = preview
                };
            default:
				throw new NotImplementedException();
        }
	}
}
