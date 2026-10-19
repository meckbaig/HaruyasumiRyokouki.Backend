using FluentValidation;
using MediatR;

namespace HaruyasumiRyokouki.Backend.Common.Behaviors;

internal class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
	private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

	public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
	{
		_logger = logger;
	}

	public async Task<TResponse> Handle
	(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		try
		{
			return await next(cancellationToken);
		}
		catch (ValidationException ex)
		{
			_logger.LogInformation("Validation error: {ErrorMessage}", ex.Message);
			throw;
		}
		catch (OperationCanceledException ex)
		{
			_logger.LogInformation("Operation was canceled: {ErrorMessage}", ex.Message);
			throw;
		}
		// Other exceptions are logged by CustomExceptionHandler.
	}
}
