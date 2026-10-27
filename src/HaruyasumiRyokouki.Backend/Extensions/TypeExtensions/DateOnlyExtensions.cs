namespace HaruyasumiRyokouki.Backend.Extensions.TypeExtensions;

/// <summary>
/// <see cref="DateOnly"/> helpers.
/// </summary>
public static class DateOnlyExtensions
{
	/// <summary>
	/// Combines the date with a time (midnight by default) into a <see cref="DateTime"/> of unspecified kind,
	/// matching the local trip time stored in the database.
	/// </summary>
	public static DateTime ToLocalDateTime(this DateOnly dateOnly, TimeOnly? timeOnly = null)
	{
		if (timeOnly == null)
			timeOnly = TimeOnly.MinValue;
		return DateTime.SpecifyKind(dateOnly.ToDateTime(timeOnly.Value), DateTimeKind.Unspecified);
	}
}
