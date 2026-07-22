using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Common.OptionalType.Supporting.Swagger;

/// <summary>
/// Converter for <see cref="Optional{T}"/> into JSON.
/// </summary>
/// <typeparam name="T">Generic parameter inside <see cref="Optional{T}"/>.</typeparam>
internal class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
	private readonly JsonConverter<T>? _valueConverter;
	private readonly Type _valueType;

	public OptionalJsonConverter(JsonSerializerOptions options)
	{
		_valueConverter = (JsonConverter<T>?)options.GetConverter(typeof(T));
		_valueType = typeof(T);
	}

	public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		T? value = _valueConverter != null
			? _valueConverter.Read(ref reader, _valueType, options)
			: JsonSerializer.Deserialize<T>(ref reader, options);

		return new Optional<T>(value);
	}

	public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
	{
		if (value.HasValue)
		{
			if (_valueConverter != null)
			{
				_valueConverter.Write(writer, value.Value!, options);
			}
			else
			{
				JsonSerializer.Serialize(writer, value.Value, options);
			}
		}
	}
}
