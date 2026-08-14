using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Extensions;
using HaruyasumiRyokouki.Backend.Models.Dtos.Tags;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HaruyasumiRyokouki.Backend.Features.Tags;

public record AddTagCommand : IRequest<AddTagResponse>
{
	[FromBody]
	public required BodyParameters Body { get; init; }

	public record BodyParameters
	{
		public required CreateTagDto Tag { get; init; }
	}
}

public class AddTagResponse
{
	public required TagDto Tag { get; set; }
}

internal class AddTagQueryHandler : IRequestHandler<AddTagCommand, AddTagResponse>
{
	private readonly IAppDbContext _context;

	public AddTagQueryHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<AddTagResponse> Handle(AddTagCommand request, CancellationToken cancellationToken)
	{
		var newTag = request.Body.Tag.FromDto();
		_context.Tags.Add(newTag);
		await _context.SaveChangesAsync(cancellationToken);

		return new AddTagResponse
		{
			Tag = newTag.ToDto()
		};
	}
}

