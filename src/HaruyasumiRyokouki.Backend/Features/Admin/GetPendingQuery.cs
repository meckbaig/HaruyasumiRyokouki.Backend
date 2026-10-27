using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Db.Enums;
using HaruyasumiRyokouki.Backend.Models.Dtos.Days;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Admin;

/// <summary>
/// Request of <c>GET /v1/admin/pending</c>.
/// </summary>
public record GetPendingQuery : IRequest<GetPendingResponse>, ILocalizableRequest, IDisplayAwareRequest
{
	/// <inheritdoc/>
	[SwaggerIgnore]
	[JsonIgnore]
	public string AcceptLanguage { get; set; } = LanguageCode.Default;

	/// <inheritdoc/>
	[SwaggerIgnore]
	[JsonIgnore]
	public ClientDisplay? ClientDisplay { get; set; }
}

/// <summary>
/// Days that are not ready and media that are not approved.
/// </summary>
public class GetPendingResponse
{
	/// <summary>
	/// Media that are not approved, oldest first, with all translations.
	/// </summary>
	public List<MediaFileEditDto> Media { get; set; }

	/// <summary>
	/// Days that are not ready, by date, with all translations.
	/// </summary>
	public List<DayEditDto> Days { get; set; }
}

internal class GetPendingQueryHandler : IRequestHandler<GetPendingQuery, GetPendingResponse>
{
	private readonly IAppDbContext _context;
	private readonly IMediaUrlsProvider _urlsProvider;

	public GetPendingQueryHandler(IAppDbContext context, IMediaUrlsProvider urlsProvider)
	{
		_context = context;
		_urlsProvider = urlsProvider;
	}

	public async Task<GetPendingResponse> Handle(GetPendingQuery request, CancellationToken cancellationToken)
	{
		var pendingDays = await _context.Days
			.AsNoTracking()
			.Include(d => d.Translations)
			.Where(d => !d.IsReady)
			.OrderBy(d => d.Date)
			.ToListAsync(cancellationToken);
		var pendingMedia = await _context.MediaFiles
			.AsNoTracking()
			.Include(m => m.Translations)
			.Include(m => m.Tags)
				.ThenIncludeFiltered(t => t.Translations, request.AcceptLanguage.LocalizedTags())
			.Where(m => !m.IsApproved)
			.OrderBy(m => m.Created)
			.ToListAsync(cancellationToken);

		var pendingMediaDtos = pendingMedia.Select(m => m.ToEditDto().AddUrls(m, _urlsProvider, request.ClientDisplay));

		return new GetPendingResponse
		{
			Media = pendingMediaDtos.ToList(),
			Days = pendingDays.ToEditDtos().ToList(),
		};
	}
}
