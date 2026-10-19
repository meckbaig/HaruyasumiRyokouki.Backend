namespace HaruyasumiRyokouki.Backend.Common.Exceptions;

/// <summary>
/// Exception that returns <see cref="StatusCodes.Status404NotFound"/>.
/// </summary>
internal class EntityNotFoundException : Exception
{
	public EntityNotFoundException(string message) : base(message)
	{
	}

	/// <summary>
	/// Throws <see cref="EntityNotFoundException"/> listing the requested ids that are absent from <paramref name="foundIds"/>.
	/// </summary>
	/// <param name="requestedIds">Ids from the request.</param>
	/// <param name="foundIds">Ids of the entities loaded from the database.</param>
	/// <param name="entityName">Entity name for the message, e.g. "Media file".</param>
	public static void ThrowIfAnyMissing(IEnumerable<int> requestedIds, IEnumerable<int> foundIds, string entityName)
	{
		var missingIds = requestedIds.Except(foundIds).ToList();
		if (missingIds.Count > 0)
			throw new EntityNotFoundException($"{entityName} with Id {string.Join(", ", missingIds)} not found.");
	}
}
