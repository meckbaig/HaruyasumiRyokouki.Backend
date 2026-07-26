namespace HaruyasumiRyokouki.Backend.Common.Options;

sealed class AiApiOptions
{
	public const string ConfigurationSectionName = "AiApi";

	public required string ApiKey { get; set; }
	public required string Model { get; set; }
	public float Temperature { get; set; }
	public required string ApiUrl { get; set; }
}
