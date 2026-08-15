using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HaruyasumiRyokouki.Backend.Common.Filters;

public sealed class RemoveEmptyRequestParametersOperationFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		if (operation.Parameters.Count == 1 && operation.Parameters.First().Schema.Format == null)
			operation.Parameters = [];
	}
}
