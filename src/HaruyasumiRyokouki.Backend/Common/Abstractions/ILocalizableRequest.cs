using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HaruyasumiRyokouki.Backend.Common.Abstractions;

public interface ILocalizableRequest
{
	[SwaggerIgnore]
	[JsonIgnore]
	string? AcceptLanguage { get; set; }
}
