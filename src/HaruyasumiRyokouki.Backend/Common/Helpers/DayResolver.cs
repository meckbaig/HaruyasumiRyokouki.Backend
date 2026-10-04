using HaruyasumiRyokouki.Backend.Models.Db;

namespace HaruyasumiRyokouki.Backend.Common.Helpers;

internal static class DayResolver
{
	public static (Day Day, bool Created) GetOrCreate(ICollection<Day> datesFromDb, DateOnly date)
	{
		if (datesFromDb.FirstOrDefault(d => d.Date == date) is Day existing)
			return (existing, false);

		var created = new Day { Date = date };
		datesFromDb.Add(created);
		return (created, true);
	}
}
