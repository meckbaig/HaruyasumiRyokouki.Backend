using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos.Days;
using HaruyasumiRyokouki.Backend.Models.Dtos.Media;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Admin;

public record GetPendingQuery : IRequest<GetPendingResponse>, ILocalizableRequest, IDisplayAwareRequest
{
	[SwaggerIgnore]
	[JsonIgnore]
	public string? AcceptLanguage { get; set; }

	[SwaggerIgnore]
	[JsonIgnore]
	public ClientDisplay? ClientDisplay { get; set; }
}

public class GetPendingResponse
{
	public List<MediaFileEditDto> Media { get; set; }
	public List<DayEditDto> Days { get; set; }
}

internal class GetPendingHandler : IRequestHandler<GetPendingQuery, GetPendingResponse>
{
	private readonly IAppDbContext _context;
	private readonly IMediaPreviewService _previewService;

	public GetPendingHandler(IAppDbContext context, IMediaPreviewService previewService)
	{
		_context = context;
		_previewService = previewService;
	}

	public async Task<GetPendingResponse> Handle(GetPendingQuery request, CancellationToken cancellationToken)
	{
		var pendingDays = await _context.Days
			.AsNoTracking()
			.Include(d => d.Translations/*.Where(t => t.LanguageCode == request.AcceptLanguage)*/)
			.Where(d => !d.IsReady)
			.OrderBy(d => d.Date)
			.ToListAsync(cancellationToken);
		var pendingMedia = await _context.MediaFiles
			.AsNoTracking()
			.Include(m => m.Translations/*.Where(t => t.LanguageCode == request.AcceptLanguage)*/)
			.Where(m => !m.IsApproved)
			.OrderBy(m => m.Created)
			.ToListAsync(cancellationToken);

		var pendingMediaDtos = pendingMedia.Select(m => m.ToEditDto().AddUrls(m.AdditionalFiles, _previewService, request.ClientDisplay));

		return new GetPendingResponse
		{
			Media = pendingMediaDtos.ToList(),
			Days = pendingDays.ToEditDtos().ToList(),
		};
	}
}
