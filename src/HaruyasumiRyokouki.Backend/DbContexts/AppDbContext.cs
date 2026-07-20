using HaruyasumiRyokouki.Backend.Models.Db;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace HaruyasumiRyokouki.Backend.DbContexts;

/// <inheritdoc/>
internal class AppDbContext : DbContext, IAppDbContext
{
	/// <inheritdoc/>
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	/// <inheritdoc/>
	public DbSet<Day> Days => Set<Day>();

	/// <inheritdoc/>
	public DbSet<DayTranslation> DayTranslations => Set<DayTranslation>();

	/// <inheritdoc/>
	public DbSet<MediaFile> MediaFiles => Set<MediaFile>();

	/// <inheritdoc/>
	public DbSet<MediaTranslation> MediaTranslations => Set<MediaTranslation>();

	/// <inheritdoc/>
	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		return await base.SaveChangesAsync(cancellationToken);
	}

	/// <inheritdoc/>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasPostgresExtension("pg_trgm");

		// === Конфигурация Day ===
		modelBuilder.Entity<Day>()
			.HasKey(d => d.Date);

		// === Конфигурация DayTranslation ===
		modelBuilder.Entity<DayTranslation>()
			.HasIndex(t => t.Note)
			.HasMethod("gin")
			.HasOperators("gin_trgm_ops");

		// Требование: Уникальность пары Дата + Язык
		modelBuilder.Entity<DayTranslation>()
			.HasIndex(t => new { t.DayDate, t.LanguageCode })
			.IsUnique();

		modelBuilder.Entity<DayTranslation>()
			.HasOne(dt => dt.Day)
			.WithMany(d => d.Translations)
			.HasForeignKey(dt => dt.DayDate)
			.OnDelete(DeleteBehavior.Cascade);


		// === Конфигурация MediaFile ===
		modelBuilder.Entity<MediaFile>()
			.HasOne(m => m.Day)
			.WithMany(d => d.Media)
			.HasForeignKey(m => m.DayDate)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<MediaTranslation>()
			.HasIndex(t => t.Title)
			.HasMethod("gin")
			.HasOperators("gin_trgm_ops");

		modelBuilder.Entity<MediaTranslation>()
			.HasIndex(t => t.Description)
			.HasMethod("gin")
			.HasOperators("gin_trgm_ops");

		modelBuilder.Entity<MediaTranslation>()
			.Property(t => t.Tags)
			.HasColumnType("text[]");

		modelBuilder.Entity<MediaTranslation>()
			.HasIndex(t => t.Tags)
			.HasMethod("gin");

		modelBuilder.Entity<MediaTranslation>()
			.HasIndex(t => new { t.MediaFileId, t.LanguageCode })
			.IsUnique();

		modelBuilder.Entity<MediaTranslation>()
			.HasOne(mt => mt.MediaFile)
			.WithMany(m => m.Translations)
			.HasForeignKey(mt => mt.MediaFileId)
			.OnDelete(DeleteBehavior.Cascade);

		base.OnModelCreating(modelBuilder);
	}

	/// <inheritdoc/>
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseSnakeCaseNamingConvention();
		base.OnConfiguring(optionsBuilder);
	}

	public static string ToSnakeCase(string input)
	{
		return Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2")
					.ToLower();
	}
}
