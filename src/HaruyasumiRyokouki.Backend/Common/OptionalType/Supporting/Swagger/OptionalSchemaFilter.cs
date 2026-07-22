using HaruyasumiRyokouki.Backend.Common.Conventions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Xml.XPath;

namespace HaruyasumiRyokouki.Backend.Common.OptionalType.Supporting.Swagger;

/// <summary>
/// Filter for <see cref="Optional{T}"/> parameters in the API request body.
/// </summary>
internal class OptionalSchemaFilter : ISchemaFilter
{
	private readonly XPathDocument? _xmlDoc = null;

	public OptionalSchemaFilter()
	{
		var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{AssemblyInfo.AssemblyName}.xml");
		if (File.Exists(xmlPath))
			_xmlDoc = new XPathDocument(xmlPath);
	}

	public void Apply(OpenApiSchema schema, SchemaFilterContext context)
	{
		// Apply the filter only to models with Properties
		if (schema.Properties == null || context.Type.IsPrimitive || context.Type == typeof(string))
			return;

		foreach (var prop in context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			if (!prop.PropertyType.IsGenericType || prop.PropertyType.GetGenericTypeDefinition() != typeof(Optional<>))
				continue;

			var propName = CamelCaseControllerNameConvention.ToCamelCase(prop.Name);

			if (!schema.Properties.TryGetValue(propName, out var existingSchema))
				continue;

			// Get the internal type T from Optional<T>
			var innerType = prop.PropertyType.GetGenericArguments()[0];

			// Generate a schema for T
			var innerSchema = context.SchemaGenerator.GenerateSchema(innerType, context.SchemaRepository);

			// Copy the schema T → model property
			schema.Properties[propName] = new OpenApiSchema
			{
				Type = innerSchema.Type,
				Format = innerSchema.Format,
				Nullable = true,
				Properties = innerSchema.Properties,
				Items = innerSchema.Items,
				Enum = innerSchema.Enum,
				Reference = innerSchema.Reference,
				AllOf = innerSchema.AllOf,
				AnyOf = innerSchema.AnyOf,
				OneOf = innerSchema.OneOf,
				Not = innerSchema.Not,
				Example = innerSchema.Example,
				Description = GetXmlSummary(prop)
			};

			schema.Reference = null;

			var optionsSchemaNames = context.SchemaRepository.Schemas
				.Where(kv => kv.Key.StartsWith(prop.PropertyType.Name))
				.Select(kv => kv.Key)
				.ToList();

			// To work correctly in Swagger, CustomSchemaIds must be overridden
			// with a similar scheme for constructing variable names:
			// if (type.IsGenericType)
			// {
			//	var genericTypeName = type.GetGenericTypeDefinition().Name;
			//	var genericArgs = string.Join(",", type.GetGenericArguments().Select(t => t.Name));
			//	return $"{genericTypeName}<{genericArgs}>";
			// }

			foreach (var schemaName in optionsSchemaNames)
			{
				context.SchemaRepository.Schemas.Remove(schemaName);
			}
		}
	}

	private string? GetXmlSummary(PropertyInfo prop)
	{
		if (_xmlDoc == null)
			return null;

		var memberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(prop);
		var nav = _xmlDoc.CreateNavigator();

		var node = nav.SelectSingleNode($"/doc/members/member[@name='{memberName}']/summary");
		return node?.InnerXml.Trim();
	}
}
