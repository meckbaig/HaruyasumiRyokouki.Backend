using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace HaruyasumiRyokouki.Backend.Common.OptionalType.Supporting.Asp;

/// <summary>
/// Provider for <see cref="OptionalModelBinder"/>.
/// </summary>
internal class OptionalModelBinderProvider : IModelBinderProvider
{
	public IModelBinder? GetBinder(ModelBinderProviderContext context)
	{
		if (context.Metadata.ModelType.IsGenericType &&
			context.Metadata.ModelType.GetGenericTypeDefinition() == typeof(Optional<>))
		{
			return new BinderTypeModelBinder(typeof(OptionalModelBinder));
		}
		return null;
	}
}
