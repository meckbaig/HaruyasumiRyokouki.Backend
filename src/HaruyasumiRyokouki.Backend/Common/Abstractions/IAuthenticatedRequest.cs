namespace HaruyasumiRyokouki.Backend.Common.Abstractions;

public interface IAuthenticatedRequest
{
	bool IsAuthenticated { get; set; }
}
