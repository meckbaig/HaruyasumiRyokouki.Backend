using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.Dtos;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;
using HaruyasumiRyokouki.Backend.Services.Interfaces;

namespace HaruyasumiRyokouki.Backend.Extensions;

internal static class MappingExtensions
{
	public static DayShortDto ToShortDto(this Day source)
	{
		return new DayShortDto
		{
			Date = source.Date,
			IsReady = source.IsReady,
			MediaCount = source.Media.Count
		};
	}

	public static IEnumerable<DayShortDto> ToShortDtos(this IEnumerable<Day> source)
	{
		return source.Select(ToShortDto);
	}

	public static DayDto ToDto(this Day source, bool admin = false)
	{
		return new DayDto
		{
			Date = source.Date,
			IsReady = source.IsReady,
			LanguageCode = source.Translations.FirstOrDefault()?.LanguageCode,
			Note = source.Translations.FirstOrDefault()?.Note,
			Media = source.Media.ToDtos(admin, admin).ToList()
		};
	}

	public static IEnumerable<DayDto> ToDtos(this IEnumerable<Day> source, bool admin = false)
	{
		return source.Select(x => x.ToDto(admin));
	}

	public static DayEditDto ToEditDto(this Day source)
	{
		return new DayEditDto
		{
			Date = source.Date,
			IsReady = source.IsReady,
			Translations = source.Translations.ToEditDtos().ToList()
		};
	}

	public static IEnumerable<DayEditDto> ToEditDtos(this IEnumerable<Day> source)
	{
		return source.Select(ToEditDto);
	}

	public static DayTranslationEditDto ToEditDto(this DayTranslation source)
	{
		return new DayTranslationEditDto
		{
			Id = source.Id,
			LanguageCode = source.LanguageCode, 
			Note = source.Note
		};
	}

	public static IEnumerable<DayTranslationEditDto> ToEditDtos(this IEnumerable<DayTranslation> source)
	{
		return source.Select(ToEditDto);
	}

	public static MediaFileDto ToDto(this MediaFile source, bool includeFavorite = false, bool admin = false)
	{
		return new MediaFileDto
		{
			Id = source.Id,
			Created = source.Created,
			FileName = source.FileName,
			AspectRatio = source.AspectRatio,
			Type = source.Type.ToString(),
			Latitude = source.Latitude,
			Longitude = source.Longitude,
			IsApproved = source.IsApproved,
			Miniature = source.Miniature,
			LanguageCode = source.Translations.FirstOrDefault()?.LanguageCode,
			Title = source.Translations.FirstOrDefault()?.Title,
			Description = source.Translations.FirstOrDefault()?.Description,
			Tags = source.Translations.FirstOrDefault()?.Tags,
			Private = admin ? source.Private : null,
			Favorite = includeFavorite ? source.Favorite : null
		};
	}

	public static IEnumerable<MediaFileDto> ToDtos(this IEnumerable<MediaFile> source, bool includeFavorite = false, bool admin = false)
	{
		return source.Select(x => x.ToDto(includeFavorite, admin));
	}

	public static MediaFileEditDto ToEditDto(this MediaFile source)
	{
		return new MediaFileEditDto
		{
			Id = source.Id,
			Created = source.Created,
			FileName = source.FileName,
			AspectRatio = source.AspectRatio,
			Type = source.Type.ToString(),
			Latitude = source.Latitude,
			Longitude = source.Longitude,
			Miniature = source.Miniature,
			Translations = source.Translations.ToEditDtos().ToList(),
			Private = source.Private,
			Favorite = source.Favorite
		};
	}

	public static IEnumerable<MediaFileEditDto> ToEditDtos(this IEnumerable<MediaFile> source)
	{
		return source.Select(ToEditDto);
	}

	public static MediaTranslationEditDto ToEditDto(this MediaTranslation source)
	{
		return new MediaTranslationEditDto
		{
			Id = source.Id,
			Description = source.Description,
			LanguageCode = source.LanguageCode,
			Title = source.Title,
			Tags = source.Tags
		};
	}

	public static IEnumerable<MediaTranslationEditDto> ToEditDtos(this IEnumerable<MediaTranslation> source)
	{
		return source.Select(ToEditDto);
	}

	public static Day FromEditDto(this Day source, DayEditDto dto)
	{
		source.IsReady = dto.IsReady;
		source.Translations = dto.Translations.FromEditDtos().ToList();
		return source;
	}

	public static DayTranslation FromEditDto(this DayTranslationEditDto source)
	{
		return new DayTranslation
		{
			Id = source.Id, 
			LanguageCode = source.LanguageCode, 
			Note = source.Note
		};
	}

	public static IEnumerable<DayTranslation> FromEditDtos(this IEnumerable<DayTranslationEditDto> source)
	{
		return source.Select(FromEditDto);
	}

	public static MediaTranslation FromEditDto(this MediaTranslationEditDto source)
	{
		return new MediaTranslation
		{
			Id = source.Id, 
			LanguageCode = source.LanguageCode,
			Title = source.Title,
			Description = source.Description,
			Tags = source.Tags
		};
	}

	public static IEnumerable<MediaTranslation> FromEditDtos(this IEnumerable<MediaTranslationEditDto> source)
	{
		return source.Select(FromEditDto);
	}

	public static IEnumerable<DayDto> AddUrls(this IEnumerable<DayDto> dayDtos, IMediaPreviewService previewService, ClientDisplay? clientDisplay = default)
	{
		return dayDtos.Select(d => d.AddUrls(previewService, clientDisplay));
	}

	public static DayDto AddUrls(this DayDto dayDto, IMediaPreviewService previewService, ClientDisplay? clientDisplay = default)
	{
		foreach (var media in dayDto.Media)
		{
			media.AddUrls(previewService, clientDisplay);
		}
		return dayDto;
	}

	public static MediaFileDto AddUrls(this MediaFileDto mediaDto, IMediaPreviewService previewService, ClientDisplay? clientDisplay = default)
	{
		switch (mediaDto.Type)
		{
			case nameof(MediaType.Image):
				mediaDto.ImageUrls = CreateImageUrls(mediaDto, previewService, clientDisplay);
				break;
			case nameof(MediaType.Video):
				mediaDto.VideoUrls = CreateVideoUrls(mediaDto, previewService, clientDisplay);
				break;
			default:
				break;
		}
		return mediaDto;
	}

	public static TPreviewDto AddUrls<TPreviewDto>(this TPreviewDto mediaDto, IMediaPreviewService previewService, ClientDisplay? clientDisplay = default)
		where TPreviewDto: IPreviewDto
	{
		switch (mediaDto.Type)
		{
			case nameof(MediaType.Image):
				mediaDto.ImageUrls = CreateImageUrls(mediaDto, previewService, clientDisplay);
				break;
			case nameof(MediaType.Video):
				mediaDto.VideoUrls = CreateVideoUrls(mediaDto, previewService, clientDisplay);
				break;
			default:
				break;
		}
		return mediaDto;
	}

	private static ImageUrlsDto? CreateImageUrls(MediaFileDto media, IMediaPreviewService previewService, ClientDisplay? clientDisplay = default)
	{
		return new ImageUrlsDto
		{
			Download = previewService.GetImageUrl(media.FileName, ImageUrlType.Download, clientDisplay, media.AspectRatio),
			FullScreen = previewService.GetImageUrl(media.FileName, ImageUrlType.FullScreen, clientDisplay, media.AspectRatio),
			Preview = previewService.GetImageUrl(media.FileName, ImageUrlType.Preview, clientDisplay, media.AspectRatio)
		};
	}

	private static VideoUrlsDto? CreateVideoUrls(MediaFileDto media, IMediaPreviewService previewService, ClientDisplay? clientDisplay = default)
	{
		return new VideoUrlsDto
		{
			Download = previewService.GetVideoUrl(media.FileName, VideoUrlType.Download, clientDisplay, media.AspectRatio),
			Stream = previewService.GetVideoUrl(media.FileName, VideoUrlType.Stream, clientDisplay, media.AspectRatio),
			Preview = previewService.GetVideoUrl(media.FileName, VideoUrlType.Preview, clientDisplay, media.AspectRatio)
		};
	}

	private static ImageUrlsDto? CreateImageUrls(IPreviewDto media, IMediaPreviewService previewService, ClientDisplay? clientDisplay = default)
	{
		return new ImageUrlsDto
		{
			Preview = previewService.GetImageUrl(media.FileName, ImageUrlType.Preview, clientDisplay, media.AspectRatio)
		};
	}

	private static VideoUrlsDto? CreateVideoUrls(IPreviewDto media, IMediaPreviewService previewService, ClientDisplay? clientDisplay = default)
	{
		return new VideoUrlsDto
		{
			Preview = previewService.GetVideoUrl(media.FileName, VideoUrlType.Preview, clientDisplay, media.AspectRatio)
		};
	}
}
