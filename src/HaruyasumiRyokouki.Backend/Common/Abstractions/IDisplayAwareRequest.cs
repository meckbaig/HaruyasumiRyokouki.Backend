using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Common.Abstractions;

public interface IDisplayAwareRequest
{
	[SwaggerIgnore]
	[JsonIgnore]
	ClientDisplay? ClientDisplay { get; set; }
}
