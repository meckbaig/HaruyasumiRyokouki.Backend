using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Abstractions;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Features.Days;

public record GetDaysQuery : IRequest<GetDaysResponse>, ILocalizableRequest
{
	[SwaggerIgnore]
	[JsonIgnore]
	public string? AcceptLanguage { get; set; }
}

public class GetDaysResponse
{
	public List<DayShortDto> Items { get; set; }
}

internal class GetDaysQueryValidator : AbstractValidator<GetDaysQuery>
{
	public GetDaysQueryValidator()
	{

	}
}

internal class GetDaysQueryHandler : IRequestHandler<GetDaysQuery, GetDaysResponse>
{
	private readonly IAppDbContext _context;

	public GetDaysQueryHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<GetDaysResponse> Handle(GetDaysQuery request, CancellationToken cancellationToken)
	{
		var days = await _context.Days.ToListAsync(cancellationToken);

		var result = days.ToShortDtos();

		return new GetDaysResponse
		{
			Items = result.ToList(),
		};
	}
}

