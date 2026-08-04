using HaruyasumiRyokouki.Backend.Models.InternalDtos.Enums;

namespace HaruyasumiRyokouki.Backend.Common.Options;

sealed class MediaFormatOptions
{
	public const string ConfigurationSectionName = "MediaFormat";

	public required FfmpegImagePreset TargetImagePreset { get; set; }
	public required FfmpegVideoPreset TargetVideoPreset { get; set; }
}
