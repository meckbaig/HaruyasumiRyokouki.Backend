using Asp.Versioning.ApiExplorer;
using HaruyasumiRyokouki.Backend.Common.Filters;
using HaruyasumiRyokouki.Backend.Common.OptionalType.Supporting.Swagger;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace HaruyasumiRyokouki.Backend.Common.Options.Configurators.Swagger;

/// <summary>
/// Configures Swagger generation options for the application.
/// </summary>
/// <remarks>
/// This class uses the provided <see cref="IApiVersionDescriptionProvider"/> to iterate over all API version descriptions
/// and add Swagger documentation for each API version.
/// </remarks>
/// <seealso cref="IConfigureOptions{SwaggerGenOptions}"/>
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{
	/// <summary>
	/// Configures Swagger generation options for documenting API versions.
	/// </summary>
	/// <param name="options">Swagger generation options to configure.</param>
	public void Configure(SwaggerGenOptions options)
	{
		foreach (var description in provider.ApiVersionDescriptions)
		{
			options.SwaggerDoc(description.GroupName, new OpenApiInfo
			{
				Title = Assembly.GetExecutingAssembly().GetName().Name,
				Version = description.GroupName,
				Description = "Backend project for website to display all my media and thoughts from my Japan trip.",
				Contact = new OpenApiContact()
				{
					Name = "meckbaig"
				}
			});
			options.SchemaFilter<OptionalSchemaFilter>();
			options.ParameterFilter<OptionalParameterFilter>();
			options.IncludeXmlComments(Assembly.GetExecutingAssembly());
			options.EnableAnnotations();
			options.DocInclusionPredicate((docName, apiDesc) =>
			{
				var groupName = apiDesc.GroupName;
				return groupName == docName;
			});
			options.ParameterFilter<CamelCaseQueryParameterFilter>(); 
			options.ParameterFilter<HeaderParameterFilter>(); 
			options.OperationFilter<HeaderAwareOperationFilter>();
			options.OperationFilter<RemoveEmptyRequestParametersOperationFilter>();
			options.DocumentFilter<RemoveSwaggerIgnoredParamsDocumentFilter>();

			options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
			{
				Type = SecuritySchemeType.Http,
				Scheme = "basic",
				Description = "Enter username and password."
			});

			options.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Basic"
						}
					},
					Array.Empty<string>()
				}
			});

			var resolved = new Dictionary<Type, string>();
			var used = new Dictionary<string, List<Type>>(StringComparer.OrdinalIgnoreCase);

			options.CustomSchemaIds(type =>
			{
				if (resolved.TryGetValue(type, out var name))
					return name;

				string shortName = GetGenericName(type);

				if (!used.TryGetValue(shortName, out List<Type>? typesWithSameName))
				{
					typesWithSameName = [];
					used[shortName] = typesWithSameName;
				}

				typesWithSameName.Add(type);

				if (typesWithSameName.Count > 1)
				{
					foreach (var t in typesWithSameName)
					{
						var fullName = t.DeclaringType != null
							? $"{t.DeclaringType.Name}.{t.Name}"
							: t.Name;

						resolved[t] = fullName;
					}

					return resolved[type];
				}

				resolved[type] = shortName;
				return shortName;
			});
		}
	}

	private static string GetGenericName(Type type)
	{
		var args = type.GenericTypeArguments;
		List<string> names = new List<string>();
		foreach (var arg in args)
		{
			names.Add(GetGenericName(arg));
		}

		if (names.Count > 0)
		{
			return $"{type.Name}<{string.Join(",", names)}>";
		}
		return type.Name;
	}
}
