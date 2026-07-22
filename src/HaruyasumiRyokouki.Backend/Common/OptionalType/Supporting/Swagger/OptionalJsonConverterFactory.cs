using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Common.OptionalType.Supporting.Swagger;

/// <summary>
/// Factory for creating <see cref="OptionalJsonConverter{T}"/> with the formation of the required generic parameter T.
/// </summary>
internal class OptionalJsonConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert.IsGenericType &&
			   typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);
	}

	public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
	{
		Type valueType = type.GetGenericArguments()[0];
		JsonConverter converter = (JsonConverter)Activator.CreateInstance(
			typeof(OptionalJsonConverter<>).MakeGenericType(valueType),
			BindingFlags.Instance | BindingFlags.Public,
			binder: null,
			args: [options],
			culture: null)!;

		return converter;
	}
}
