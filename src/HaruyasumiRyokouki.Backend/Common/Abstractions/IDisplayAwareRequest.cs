using HaruyasumiRyokouki.Backend.Models.InternalDtos;

namespace HaruyasumiRyokouki.Backend.Common.Abstractions;

public interface IDisplayAwareRequest
{
	ClientDisplay? ClientDisplay { get; set; }
}
