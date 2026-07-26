using FluentValidation;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaruyasumiRyokouki.Backend.Features.Media;

public record GetEditMediaQuery : IRequest<GetEditMediaResponse>
{
	[FromQuery]
	public required ICollection<int> Ids { get; init; }
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

	public GetEditMediaQueryHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<GetEditMediaResponse> Handle(GetEditMediaQuery request, CancellationToken cancellationToken)
	{
		var result = await _context.MediaFiles
			.AsNoTracking()
			.Include(m => m.Translations)
			.Where(m => request.Ids.Contains(m.Id))
			.Select(m => m.ToEditDto())
			.ToListAsync(cancellationToken);

		return new GetEditMediaResponse { Items = result };
	}
}

