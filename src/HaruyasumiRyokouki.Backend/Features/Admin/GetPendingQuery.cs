using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Admin;

public record GetPendingQuery : IRequest<GetPendingResponse>, ILocalizableRequest
{
	[SwaggerIgnore]
	[JsonIgnore]
	public string? AcceptLanguage { get; set; }
}

public class GetPendingResponse
{
	public List<MediaFileEditDto> Media { get; set; }
	public List<DayEditDto> Days { get; set; }
}


internal class GetPendingHandler : IRequestHandler<GetPendingQuery, GetPendingResponse>
{
	private readonly IAppDbContext _context;

	public GetPendingHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<GetPendingResponse> Handle(GetPendingQuery request, CancellationToken cancellationToken)
	{
		var pendingDays = _context.Days
			.AsNoTracking()
			.Include(d => d.Translations/*.Where(t => t.LanguageCode == request.AcceptLanguage)*/)
			.Where(d => !d.IsReady).ToList();
		var pendingMedia = _context.MediaFiles
			.AsNoTracking()
			.Include(m => m.Translations/*.Where(t => t.LanguageCode == request.AcceptLanguage)*/)
			.Where(m => !m.IsApproved).ToList();

		return new GetPendingResponse
		{
			Media = pendingMedia.ToEditDtos().ToList(),
			Days = pendingDays.ToEditDtos().ToList(),
		};
	}
}
