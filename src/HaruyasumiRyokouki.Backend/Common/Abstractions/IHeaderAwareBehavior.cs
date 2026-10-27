namespace HaruyasumiRyokouki.Backend.Common.Abstractions;

/// <summary>
/// A pipeline behavior that reads a request header. <c>HeaderAwareOperationFilter</c> adds the header
/// to Swagger for every request the behavior applies to.
/// </summary>
public interface IHeaderAwareBehavior
{
	/// <summary>
	/// Header name.
	/// </summary>
	static abstract string HeaderName { get; }

	/// <summary>
	/// Header description for Swagger.
	/// </summary>
	static abstract string Description { get; }

	/// <summary>
	/// Example value for Swagger.
	/// </summary>
	static abstract string Example { get; }

	/// <summary>
	/// Whether Swagger marks the header as required.
	/// </summary>
	static abstract bool Required { get; }
}
