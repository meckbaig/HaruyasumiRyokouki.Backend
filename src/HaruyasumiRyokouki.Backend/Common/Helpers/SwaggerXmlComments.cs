using System.Collections.Concurrent;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;

namespace HaruyasumiRyokouki.Backend.Common.Helpers;

/// <summary>
/// Prepares the XML documentation of the application for Swagger.
/// </summary>
/// <remarks>
/// The compiler writes <c>&lt;see cref="..."/&gt;</c> as a full documentation id, e.g.
/// <c>P:HaruyasumiRyokouki.Backend.Features.Days.EditDayCommand.BodyParameters.AutoTranslate</c>, and Swagger shows it as is.
/// Here every such reference is replaced with <c>&lt;c&gt;</c> containing the name without the namespace:
/// <c>EditDayCommand.BodyParameters.AutoTranslate</c>.
/// </remarks>
internal static class SwaggerXmlComments
{
	private static readonly ConcurrentDictionary<string, Type?> TypeCache = new();

	/// <summary>
	/// Loads the XML documentation file of <paramref name="assembly"/> with references converted to code names.
	/// </summary>
	public static XPathDocument Load(Assembly assembly)
	{
		string path = Path.Combine(AppContext.BaseDirectory, assembly.GetName().Name + ".xml");
		var document = XDocument.Load(path);

		foreach (var see in document.Descendants().Where(e => e.Name.LocalName is "see" or "seealso").ToList())
		{
			string? cref = see.Attribute("cref")?.Value;
			if (cref == null)
				continue;

			string text = string.IsNullOrWhiteSpace(see.Value) ? ToCodeName(cref) : see.Value;
			see.ReplaceWith(new XElement("c", text));
		}

		using var reader = document.CreateReader();
		return new XPathDocument(reader);
	}

	/// <summary>
	/// Converts a documentation id to the name used in code: types with their declaring types, without the namespace.
	/// </summary>
	/// <param name="cref">Documentation id, e.g. <c>M:Namespace.Type.Method(System.String)</c>.</param>
	/// <returns>E.g. <c>Type.Method</c>. When the type cannot be found, the last segment of the id.</returns>
	public static string ToCodeName(string cref)
	{
		if (cref.Length < 2 || cref[1] != ':')
			return LastSegment(cref);

		char kind = cref[0];
		string id = cref[2..];

		int parameters = id.IndexOf('(');
		if (parameters >= 0)
			id = id[..parameters];

		if (kind == 'T')
			return TypeName(id) ?? LastSegment(id);

		if (kind is 'P' or 'F' or 'M' or 'E')
		{
			int dot = id.LastIndexOf('.');
			if (dot < 0)
				return id;

			string typeName = TypeName(id[..dot]) ?? LastSegment(id[..dot]);
			string member = StripGenericArity(id[(dot + 1)..]);
			return member == "#ctor" ? typeName : $"{typeName}.{member}";
		}

		return LastSegment(id);
	}

	/// <summary>
	/// Finds the type by its documentation name, trying each split into a namespace and nested types,
	/// and returns the type name with its declaring types, or <see langword="null"/> when it is not found.
	/// </summary>
	private static string? TypeName(string documentationName)
	{
		var type = TypeCache.GetOrAdd(documentationName, FindType);
		return type == null ? null : CodeName(type);
	}

	private static Type? FindType(string documentationName)
	{
		string[] segments = documentationName.Split('.');
		var assemblies = AppDomain.CurrentDomain.GetAssemblies();

		// The longest namespace first: "A.B.Outer.Inner" is tried as "A.B.Outer+Inner" before "A.B+Outer+Inner".
		for (int namespaceLength = segments.Length - 1; namespaceLength >= 0; namespaceLength--)
		{
			string typeName = string.Join('+', segments[namespaceLength..]);
			string fullName = namespaceLength == 0
				? typeName
				: string.Join('.', segments[..namespaceLength]) + "." + typeName;

			foreach (var assembly in assemblies)
			{
				var type = assembly.GetType(fullName, throwOnError: false);
				if (type != null)
					return type;
			}
		}
		return null;
	}

	private static string CodeName(Type type)
	{
		string name = StripGenericArity(type.Name);
		var genericArguments = type.GetGenericArguments();
		int ownArgumentsStart = type.DeclaringType?.GetGenericArguments().Length ?? 0;
		if (genericArguments.Length > ownArgumentsStart)
			name += "<" + string.Join(", ", genericArguments[ownArgumentsStart..].Select(a => a.Name)) + ">";

		return type.DeclaringType == null ? name : $"{CodeName(type.DeclaringType)}.{name}";
	}

	private static string StripGenericArity(string name)
	{
		int tick = name.IndexOf('`');
		return tick < 0 ? name : name[..tick];
	}

	private static string LastSegment(string id)
	{
		return StripGenericArity(id[(id.LastIndexOf('.') + 1)..]);
	}
}
