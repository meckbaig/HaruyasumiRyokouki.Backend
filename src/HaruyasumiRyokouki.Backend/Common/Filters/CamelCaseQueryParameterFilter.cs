using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace HaruyasumiRyokouki.Backend.Common.Filters;

/// <summary>
/// A filter that converts query parameter names to camelCase.
/// </summary>
public class CamelCaseQueryParameterFilter : IParameterFilter
{
	/// <summary>
	/// Applies the camelCase naming convention to query parameter names.
	/// </summary>
	/// <param name="parameter"></param>
	/// <param name="context"></param>
	public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
	{
		if (parameter.In != ParameterLocation.Query && parameter.In != ParameterLocation.Path)
			return;
		parameter.Name = JsonNamingPolicy.CamelCase.ConvertName(parameter.Name);
	}
}
