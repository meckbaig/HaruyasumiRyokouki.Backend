namespace HaruyasumiRyokouki.Backend.Common.Abstractions;

public interface ILocalizableRequest
{
	string? AcceptLanguage { get; set; }
}
