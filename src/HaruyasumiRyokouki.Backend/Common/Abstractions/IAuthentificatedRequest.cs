namespace HaruyasumiRyokouki.Backend.Common.Abstractions;

public interface IAuthentificatedRequest
{
	bool IsAuthenticated { get; set; }
}
