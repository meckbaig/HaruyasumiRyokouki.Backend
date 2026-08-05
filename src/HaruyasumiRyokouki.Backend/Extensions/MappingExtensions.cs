using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.Dtos;
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

	public static DayDto ToDto(this Day source)
	{
		return new DayDto
		{
			Date = source.Date,
			IsReady = source.IsReady,
			LanguageCode = source.Translations.FirstOrDefault()?.LanguageCode,
			Note = source.Translations.FirstOrDefault()?.Note,
			Media = source.Media.ToDtos().ToList()
		};
	}

	public static IEnumerable<DayDto> ToDtos(this IEnumerable<Day> source)
	{
		return source.Select(ToDto);
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

	public static MediaFileDto ToDto(this MediaFile source)
	{
		return new MediaFileDto
		{
			Id = source.Id,
			Created = source.Created,
			FileName = source.FileName,
			Type = source.Type.ToString(),
			Latitude = source.Latitude,
			Longitude = source.Longitude,
			IsApproved = source.IsApproved,
			Miniature = source.Miniature,
			LanguageCode = source.Translations.FirstOrDefault()?.LanguageCode,
			Title = source.Translations.FirstOrDefault()?.Title,
			Description = source.Translations.FirstOrDefault()?.Description,
			Tags = source.Translations.FirstOrDefault()?.Tags
		};
	}

	public static IEnumerable<MediaFileDto> ToDtos(this IEnumerable<MediaFile> source)
	{
		return source.Select(ToDto);
	}

	public static MediaFileEditDto ToEditDto(this MediaFile source)
	{
		return new MediaFileEditDto
		{
			Id = source.Id,
			Created = source.Created,
			FileName = source.FileName,
			Type = source.Type.ToString(),
			Latitude = source.Latitude,
			Longitude = source.Longitude,
			Miniature = source.Miniature,
			Translations = source.Translations.ToEditDtos().ToList()
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

	public static IEnumerable<DayDto> AddUrls(this IEnumerable<DayDto> dayDtos, IMediaPreviewService previewService)
	{
		return dayDtos.Select(d => d.AddUrls(previewService));
	}

	public static DayDto AddUrls(this DayDto dayDto, IMediaPreviewService previewService)
	{
		foreach (var media in dayDto.Media)
		{
			media.AddUrls(previewService);
		}
		return dayDto;
	}

	public static MediaFileDto AddUrls(this MediaFileDto mediaDto, IMediaPreviewService previewService)
	{
		switch (mediaDto.Type)
		{
			case nameof(MediaType.Image):
				mediaDto.ImageUrls = CreateImageUrls(mediaDto, previewService);
				break;
			case nameof(MediaType.Video):
				mediaDto.VideoUrls = CreateVideoUrls(mediaDto, previewService);
				break;
			default:
				break;
		}
		return mediaDto;
	}

	private static ImageUrlsDto? CreateImageUrls(MediaFileDto media, IMediaPreviewService previewService)
	{
		return new ImageUrlsDto
		{
			Desktop = new()
			{
				Original = previewService.GetImageUrl(media.FileName, ImageUrlType.Original),
				Preview = previewService.GetImageUrl(media.FileName, ImageUrlType.Preview),
			},
			Mobile = new()
			{
				Original = previewService.GetImageUrl(media.FileName, ImageUrlType.MobileOriginal),
				Preview = previewService.GetImageUrl(media.FileName, ImageUrlType.MobilePreview)
			}
		};
	}

	private static VideoUrlsDto? CreateVideoUrls(MediaFileDto media, IMediaPreviewService previewService)
	{
		return new VideoUrlsDto
		{
			Download = previewService.GetVideoUrl(media.FileName, VideoUrlType.Download),
			Stream = previewService.GetVideoUrl(media.FileName, VideoUrlType.Stream),
			Desktop = new()
			{
				Preview = previewService.GetVideoUrl(media.FileName, VideoUrlType.Preview),
			},
			Mobile = new()
			{
				Preview = previewService.GetVideoUrl(media.FileName, VideoUrlType.MobilePreview)
			},
		};
	}
}
