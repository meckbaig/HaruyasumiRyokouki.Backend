using HaruyasumiRyokouki.Backend.Models.Db;
using HaruyasumiRyokouki.Backend.Models.Dtos;

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

	public static MediaFileDto ToDto(this MediaFile source)
	{
		return new MediaFileDto
		{
			FileName = source.FileName,
			Type = source.Type.ToString(),
			Latitude = source.Latitude,
			Longitude = source.Longitude,
			IsApproved = source.IsApproved,
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
}
