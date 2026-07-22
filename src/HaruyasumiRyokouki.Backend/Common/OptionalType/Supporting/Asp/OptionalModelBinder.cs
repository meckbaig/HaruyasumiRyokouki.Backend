using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;

namespace HaruyasumiRyokouki.Backend.Common.OptionalType.Supporting.Asp;

/// <summary>
/// Binder for correct decryption of <see cref="Optional{T}"/> during data parsing in ASP.
/// </summary>
internal class OptionalModelBinder : IModelBinder
{
	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		var modelType = bindingContext.ModelType;
		if (!modelType.IsGenericType || modelType.GetGenericTypeDefinition() != typeof(Optional<>))
		{
			return Task.CompletedTask;
		}

		var innerType = modelType.GetGenericArguments()[0];
		var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

		if (valueProviderResult == ValueProviderResult.None)
		{
			// The parameter was not passed
			var optionalInstance = Activator.CreateInstance(modelType); // Optional<T> without value
			bindingContext.Result = ModelBindingResult.Success(optionalInstance);
			return Task.CompletedTask;
		}

		bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

		try
		{
			var converter = TypeDescriptor.GetConverter(innerType);
			var value = converter.ConvertFromInvariantString(valueProviderResult.FirstValue);
			var optionalCtor = modelType.GetConstructor([innerType]);
			var optionalInstance = optionalCtor?.Invoke([value]);
			bindingContext.Result = ModelBindingResult.Success(optionalInstance);
		}
		catch (Exception ex)
		{
			throw new ValidationException([new(bindingContext.ModelName, ex.Message)]);
		}

		return Task.CompletedTask;
	}
}
