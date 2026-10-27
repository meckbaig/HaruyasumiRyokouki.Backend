using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HaruyasumiRyokouki.Backend.Common.Filters;

/// <summary>
/// Swagger filter that removes the parameter generated for a bound request object with no fields, e.g. <c>GET /v1/days</c>.
/// </summary>
public sealed class RemoveEmptyRequestParametersOperationFilter : IOperationFilter
{
	/// <inheritdoc/>
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		if (operation.Parameters.Count == 1 && operation.Parameters.First().Schema.Format == null)
			operation.Parameters = [];
	}
}
