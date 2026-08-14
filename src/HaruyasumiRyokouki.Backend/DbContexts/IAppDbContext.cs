using Microsoft.EntityFrameworkCore;
using HaruyasumiRyokouki.Backend.Models.Db;

namespace HaruyasumiRyokouki.Backend.DbContexts;

internal interface IAppDbContext
{
	DbSet<Day> Days { get; }
	DbSet<MediaFile> MediaFiles { get; }
	DbSet<Tag> Tags { get; }

	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
