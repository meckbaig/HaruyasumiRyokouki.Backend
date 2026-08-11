using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HaruyasumiRyokouki.Backend.Common.Filters;

public sealed class RemoveEmptyRequestBodyOperationFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		if (operation.RequestBody is null)
			return;

		var schema = operation.RequestBody.Content.Values
			.FirstOrDefault()?.Schema;

		if (schema?.Properties is { Count: 0 })
		{
			operation.RequestBody = null;
		}
	}
}
