using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaruyasumiRyokouki.Backend.Features.Media;

public record GetEditMediaQuery : IRequest<GetEditMediaResponse>, IDisplayAwareRequest
{
	[FromQuery]
	public required ICollection<int> Ids { get; init; }

	public ClientDisplay? ClientDisplay { get; set; }
}

public class GetEditMediaResponse
{
	public required ICollection<MediaFileEditDto> Items { get; init; }
}

internal class GetEditMediaQueryValidator : AbstractValidator<GetEditMediaQuery>
{
	public GetEditMediaQueryValidator()
	{
		RuleFor(x => x.Ids).NotEmpty();
	}
}

internal class GetEditMediaQueryHandler : IRequestHandler<GetEditMediaQuery, GetEditMediaResponse>
{
	private readonly IAppDbContext _context;
	private readonly IMediaPreviewService _previewService;

	public GetEditMediaQueryHandler(IAppDbContext context, IMediaPreviewService previewService)
	{
		_context = context;
		_previewService = previewService;
	}

	public async Task<GetEditMediaResponse> Handle(GetEditMediaQuery request, CancellationToken cancellationToken)
	{
		var mediaFileDtos = await _context.MediaFiles
			.AsNoTracking()
			.Include(m => m.Translations)
			.Where(m => request.Ids.Contains(m.Id))
			.Select(m => m.ToEditDto())
			.ToListAsync(cancellationToken);

		var results = mediaFileDtos.Select(dto => dto.AddUrls(_previewService));

		return new GetEditMediaResponse 
		{
			Items = results.ToList() 
		};
	}
}

