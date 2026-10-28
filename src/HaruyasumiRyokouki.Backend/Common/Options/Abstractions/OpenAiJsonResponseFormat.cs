namespace HaruyasumiRyokouki.Backend.Common.Options.Abstractions;

/// <summary>
/// How an <see cref="TranslationOptionsType.OpenAi"/> provider is asked for a JSON answer.
/// </summary>
public enum OpenAiJsonResponseFormat
{
	/// <summary>
	/// <c>json_object</c>: any valid JSON, the structure is described in the prompt. Supported by OpenAI and DeepSeek.
	/// </summary>
	JsonObject,

	/// <summary>
	/// <c>json_schema</c>: JSON constrained by a schema generated from the response type. Supported by OpenAI and LM Studio.
	/// </summary>
	JsonSchema,

	/// <summary>
	/// No response format is sent, the structure is described only in the prompt. For APIs that support neither of the above.
	/// </summary>
	Text
}
