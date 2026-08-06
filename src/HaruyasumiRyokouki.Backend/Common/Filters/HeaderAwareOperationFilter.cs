using HaruyasumiRyokouki.Backend.Common.Abstractions;
using MediatR;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace HaruyasumiRyokouki.Backend.Common.Filters;

public class HeaderAwareOperationFilter : IOperationFilter
{
	/// <summary>
	/// Caches mapping: "marker interface (e.g. IDisplayAwareRequest)" -> "header configuration".
	/// </summary>
	private static readonly Dictionary<Type, HeaderConfig> CachedHeaders = new();

	private static bool _isInitialized = false;
	private static readonly object _lock = new();

	/// <summary>
	/// Applies the operation filter: if controller request parameter implements any of the marker interfaces
	/// discovered from <see cref="IHeaderAwareBehavior"/> implementations, adds corresponding header parameters
	/// to the OpenAPI operation description.
	/// </summary>
	/// <param name="operation">The OpenAPI operation to modify.</param>
	/// <param name="context">The operation filter context providing method info.</param>
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		EnsureInitialized();

		// Find the controller method parameter type that is a MediatR request (IRequest / IBaseRequest)
		var requestType = context.MethodInfo.GetParameters()
			.Select(p => p.ParameterType)
			.FirstOrDefault(t => typeof(IBaseRequest).IsAssignableFrom(t));

		if (requestType == null)
			return;

		// Check which marker interfaces (keys in cache) are implemented by our request type
		var matchedInterfaces = CachedHeaders.Keys
			.Where(marker => marker.IsAssignableFrom(requestType))
			.ToList();

		if (!matchedInterfaces.Any())
			return;

		operation.Parameters ??= new List<OpenApiParameter>();

		foreach (var marker in matchedInterfaces)
		{
			var config = CachedHeaders[marker];

			// Avoid duplication if the header is already described manually
			if (operation.Parameters.Any(p => p.Name.Equals(config.HeaderName, StringComparison.OrdinalIgnoreCase)))
				continue;

			operation.Parameters.Add(new OpenApiParameter
			{
				Name = config.HeaderName,
				In = ParameterLocation.Header,
				Description = config.Description,
				Required = config.Required,
				Schema = new OpenApiSchema { Type = "string" },
				Example = new OpenApiString(config.Example)
			});
		}
	}

	/// <summary>
	/// Ensures the header configurations are initialized by scanning the current assembly for
	/// implementations of <see cref="IHeaderAwareBehavior"/> and reading their static properties.
	/// Thread-safe and executed only once.
	/// </summary>
	private static void EnsureInitialized()
	{
		if (_isInitialized) 
			return;

		lock (_lock)
		{
			if (_isInitialized) 
				return;

			// Find all concrete classes implementing IHeaderAwareBehavior
			var behaviorTypes = Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => t.IsClass && !t.IsAbstract)
				.Where(t => t.GetInterfaces().Any(i => i == typeof(IHeaderAwareBehavior)));

			foreach (var type in behaviorTypes)
			{
				// Find TRequest among the generic type parameters
				var genericArgs = type.GetGenericArguments();
				var requestArgInfo = genericArgs.FirstOrDefault(a => a.Name == "TRequest");

				if (requestArgInfo == null)
					continue;

				// Find the marker interface in TRequest generic constraints (for example: where TRequest : IDisplayAwareRequest)
				var markerInterface = requestArgInfo.GetGenericParameterConstraints()
					.FirstOrDefault(c => c.IsInterface && c != typeof(IBaseRequest));

				if (markerInterface == null) 
					continue;

				// To read static properties from an open generic type, we need to close it.
				// Substitute the marker interface for TRequest and 'object' for other generic parameters (e.g., TResponse).
				var typeArguments = new Type[genericArgs.Length];
				for (int i = 0; i < genericArgs.Length; i++)
				{
					typeArguments[i] = genericArgs[i].Name == "TRequest" ? markerInterface : typeof(object);
				}

				var closedType = type.MakeGenericType(typeArguments);

				// Read static properties via reflection without creating an instance
				var headerName = GetStaticPropertyValue<string>(closedType, nameof(IHeaderAwareBehavior.HeaderName));
				var description = GetStaticPropertyValue<string>(closedType, nameof(IHeaderAwareBehavior.Description));
				var example = GetStaticPropertyValue<string>(closedType, nameof(IHeaderAwareBehavior.Example));
				var required = GetStaticPropertyValue<bool>(closedType, nameof(IHeaderAwareBehavior.Required));

				CachedHeaders[markerInterface] = new HeaderConfig
				{
					HeaderName = headerName,
					Description = description,
					Example = example,
					Required = required
				};
			}

			_isInitialized = true;
		}
	}

	/// <summary>
	/// Reads a static property value from <paramref name="type"/> by name using reflection.
	/// Returns <c>default</c> if the property does not exist or cannot be read.
	/// </summary>
	/// <typeparam name="T">Expected property type.</typeparam>
	/// <param name="type">Type from which to read the static property.</param>
	/// <param name="propertyName">Name of the static property.</param>
	/// <returns>The property value cast to <typeparamref name="T"/> or default.</returns>
	private static T? GetStaticPropertyValue<T>(Type type, string propertyName)
	{
		var property = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		return property != null ? (T?)property.GetValue(null) : default;
	}

	/// <summary>
	/// Internal container for header configuration read from <see cref="IHeaderAwareBehavior"/> implementations.
	/// </summary>
	private class HeaderConfig
	{
		/// <summary>
		/// Header name.
		/// </summary>
		public string HeaderName { get; init; } = string.Empty;

		/// <summary>
		/// Header description.
		/// </summary>
		public string Description { get; init; } = string.Empty;

		/// <summary>
		/// Example value for the header.
		/// </summary>
		public string Example { get; init; } = string.Empty;

		/// <summary>
		/// Whether the header is required.
		/// </summary>
		public bool Required { get; init; }
	}
}
