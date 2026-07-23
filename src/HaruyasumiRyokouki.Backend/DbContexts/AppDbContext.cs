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

		modelBuilder.Entity<Day>(entity =>
		{
			entity.HasKey(d => d.Id);

			entity.HasIndex(d => d.Date)
				  .IsUnique();
		});


		modelBuilder.Entity<DayTranslation>(entity =>
		{
			entity.HasIndex(t => t.Note)
				  .HasMethod("gin")
				  .HasOperators("gin_trgm_ops");

			entity.HasIndex(t => new { t.DayId, t.LanguageCode })
				  .IsUnique();

			entity.HasOne(dt => dt.Day)
				  .WithMany(d => d.Translations)
				  .HasForeignKey(dt => dt.DayId)
				  .OnDelete(DeleteBehavior.Cascade);
		});


		modelBuilder.Entity<MediaFile>(entity =>
		{
			entity.HasOne(m => m.Day)
				  .WithMany(d => d.Media)
				  .HasForeignKey(m => m.DayId)
				  .OnDelete(DeleteBehavior.Cascade);

			entity.Property(m => m.Type)
				  .HasConversion<string>();

			entity.Property(m => m.IsApproved)
				  .HasDefaultValue(false);

			entity.Property(m => m.Created)
				  .HasColumnType("timestamp without time zone");
		});
			

		modelBuilder.Entity<MediaTranslation>(entity =>
		{
			entity.HasIndex(t => t.Title)
				  .HasMethod("gin")
				  .HasOperators("gin_trgm_ops");

			entity.HasIndex(t => t.Description)
				  .HasMethod("gin")
				  .HasOperators("gin_trgm_ops");

			entity.Property(t => t.Tags)
				  .HasColumnType("text[]");

			entity.HasIndex(t => t.Tags)
				  .HasMethod("gin");

			entity.HasIndex(t => new { t.MediaFileId, t.LanguageCode })
				  .IsUnique();

			entity.HasOne(mt => mt.MediaFile)
				  .WithMany(m => m.Translations)
				  .HasForeignKey(mt => mt.MediaFileId)
				  .OnDelete(DeleteBehavior.Cascade);
		});
			

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
