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
	public DbSet<Tag> Tags => Set<Tag>();

	/// <inheritdoc/>
	public DbSet<MediaFileTag> MediaFileTags => Set<MediaFileTag>();

	/// <inheritdoc/>
	public DbSet<TagTranslation> TagTranslations => Set<TagTranslation>();

	/// <inheritdoc/>
	public DbSet<MediaEmbedding> MediaEmbeddings => Set<MediaEmbedding>();

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

			entity.HasMany(m => m.Tags)
				  .WithMany(t => t.Media)
				  .UsingEntity<MediaFileTag>(
					right => right
						.HasOne(x => x.Tag)
						.WithMany(x => x.MediaTags)
						.HasForeignKey(x => x.TagId),

					left => left
						.HasOne(x => x.Media)
						.WithMany(x => x.MediaTags)
						.HasForeignKey(x => x.MediaId),

					join =>
					{
						join.HasKey(x => new { x.MediaId, x.TagId });

						join.ToTable("media_file_tag");
					});

			entity.Property(t => t.AdditionalFiles)
				  .HasColumnType("text[]");
		});

		modelBuilder.Entity<MediaTranslation>(entity =>
		{
			entity.HasIndex(t => t.Title)
				  .HasMethod("gin")
				  .HasOperators("gin_trgm_ops");

			entity.HasIndex(t => t.Description)
				  .HasMethod("gin")
				  .HasOperators("gin_trgm_ops");

			entity.HasIndex(t => new { t.MediaFileId, t.LanguageCode })
				  .IsUnique();

			entity.HasOne(mt => mt.MediaFile)
				  .WithMany(m => m.Translations)
				  .HasForeignKey(mt => mt.MediaFileId)
				  .OnDelete(DeleteBehavior.Cascade);
		});


		modelBuilder.Entity<Tag>(entity =>
		{
			entity.HasIndex(t => t.Slug)
				  .IsUnique();
		});


		modelBuilder.Entity<TagTranslation>(entity =>
		{
			entity.HasIndex(l => new { l.TagId, l.LanguageCode })
				  .IsUnique()
				  .HasFilter("is_primary")
				  .HasDatabaseName("ux_tag_labels_primary_per_language");

			entity.HasIndex(l => new { l.TagId, l.Text })
				  .IsUnique();

			entity.HasIndex(l => l.Text)
				  .HasMethod("gin")
				  .HasOperators("gin_trgm_ops");

			entity.HasOne(l => l.Tag)
				  .WithMany(t => t.Translations)
				  .HasForeignKey(l => l.TagId)
				  .OnDelete(DeleteBehavior.Cascade);
		});

		modelBuilder.Entity<MediaEmbedding>(entity =>
		{
			entity.HasKey(e => e.MediaFileId);

			entity.Property(e => e.Vector)
				  .HasColumnType("real[]");

			entity.HasOne(e => e.MediaFile)
				  .WithOne()
				  .HasForeignKey<MediaEmbedding>(e => e.MediaFileId)
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
