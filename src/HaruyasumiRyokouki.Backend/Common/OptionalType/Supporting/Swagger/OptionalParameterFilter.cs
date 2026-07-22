using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HaruyasumiRyokouki.Backend.Common.OptionalType.Supporting.Swagger;

/// <summary>
/// Filter for <see cref="Optional{T}"/> parameters in the query API request.
/// </summary>
internal class OptionalParameterFilter : IParameterFilter
{
	public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
	{
		var type = context?.PropertyInfo?.PropertyType;

		if (type != null && IsOptional(type))
		{
			var innerType = type.GetGenericArguments()[0];

			// Replace the parameter schema with an internal type schema
			var schema = context.SchemaGenerator.GenerateSchema(innerType, context.SchemaRepository);
			parameter.Schema = schema;

			// Optional parameter
			parameter.Required = false;
		}
	}

	private static bool IsOptional(Type type)
	{
		if (!type.IsGenericType)
			return false;
		return type.GetGenericTypeDefinition() == typeof(Optional<>);
	}
}
