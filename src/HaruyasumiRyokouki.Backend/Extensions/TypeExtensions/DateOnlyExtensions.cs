namespace HaruyasumiRyokouki.Backend.Extensions.TypeExtensions;

public static class DateOnlyExtensions
{
	public static DateTime ToLocalDateTime(this DateOnly dateOnly, TimeOnly? timeOnly = null)
	{
		if (timeOnly == null)
			timeOnly = TimeOnly.MinValue;
		return DateTime.SpecifyKind(dateOnly.ToDateTime(timeOnly.Value), DateTimeKind.Unspecified);
	}
}
