using HaruyasumiRyokouki.Backend.Common.Exceptions;

namespace HaruyasumiRyokouki.Backend.UnitTests.Exceptions;

public class EntityNotFoundExceptionTests
{
	[Fact]
	public void ThrowIfAnyMissing_AllFound_DoesNotThrow()
	{
		EntityNotFoundException.ThrowIfAnyMissing([1, 2, 3], [3, 2, 1], "Media file");
	}

	[Fact]
	public void ThrowIfAnyMissing_SomeMissing_ListsOnlyMissingIds()
	{
		var ex = Assert.Throws<EntityNotFoundException>(
			() => EntityNotFoundException.ThrowIfAnyMissing([1, 2, 3, 4], [1, 3], "Media file"));

		Assert.Equal("Media file with Id 2, 4 not found.", ex.Message);
	}

	[Fact]
	public void ThrowIfAnyMissing_DuplicateRequestedIds_ListedOnce()
	{
		var ex = Assert.Throws<EntityNotFoundException>(
			() => EntityNotFoundException.ThrowIfAnyMissing([5, 5, 1], [1], "Tag"));

		Assert.Equal("Tag with Id 5 not found.", ex.Message);
	}

	[Fact]
	public void ThrowIfAnyMissing_DuplicateRequestedIdsAllFound_DoesNotThrow()
	{
		EntityNotFoundException.ThrowIfAnyMissing([1, 1], [1], "Tag");
	}
}
