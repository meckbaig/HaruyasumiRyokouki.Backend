namespace HaruyasumiRyokouki.Backend.Common.Abstractions;

public interface IHeaderAwareBehavior
{
	static abstract string HeaderName { get; }
	static abstract string Description { get; }
	static abstract string Example { get; }
	static abstract bool Required { get; }
}
