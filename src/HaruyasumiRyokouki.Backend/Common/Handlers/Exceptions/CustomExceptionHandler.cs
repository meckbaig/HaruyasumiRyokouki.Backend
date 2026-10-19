using FluentValidation;
using FluentValidation.Results;
using HaruyasumiRyokouki.Backend.Common.Exceptions;
using HaruyasumiRyokouki.Backend.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HaruyasumiRyokouki.Backend.Common.Handlers.Exceptions;

internal class CustomExceptionHandler : IExceptionHandler
{
	private readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;
	private readonly ILogger<CustomExceptionHandler> _logger;

	public CustomExceptionHandler(ILogger<CustomExceptionHandler> logger)
	{
		_logger = logger;
		_exceptionHandlers = new()
		{
			{ typeof(ValidationException), HandleValidationException },
			{ typeof(EntityNotFoundException), HandleNotFoundException },
			{ typeof(ForbiddenAccessException), HandleForbiddenAccessException },
			{ typeof(UnauthorizedException), HandleUnauthorizedException },
			{ typeof(ConflictException), HandleConflictException },
			{ typeof(ExternalServiceException), HandleExternalServiceException },
		};
	}

	public async ValueTask<bool> TryHandleAsync
	(
		HttpContext httpContext,
		Exception ex,
		CancellationToken cancellationToken
	)
	{
		var handler = _exceptionHandlers
			.FirstOrDefault(h => h.Key.IsInstanceOfType(ex))
			.Value;

		if (handler != null)
		{
			await handler.Invoke(httpContext, ex);
			return true;
		}

		if (ex is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
			_logger.LogInformation("Request {Method} {Path} was aborted by the client", httpContext.Request.Method, httpContext.Request.Path);
		else
			_logger.LogError(ex, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

		await HandleUnhandledException(httpContext, ex);
		return false;
	}

	private static async Task HandleUnhandledException(HttpContext context, Exception ex)
	{
		context.Response.StatusCode = StatusCodes.Status500InternalServerError;

		// The inner exception usually holds the meaningful message (e.g. the database error
		// behind DbUpdateException), and it is shown on the client.
		string detail = ex.InnerException?.Message ?? ex.Message;
		await context.Response.WriteAsJsonAsync(GenerateInternalProblemDetails(detail));
	}

	private static async Task HandleValidationException(HttpContext context, Exception ex)
	{
		ValidationException validationException = (ex as ValidationException)!;
		context.Response.StatusCode = StatusCodes.Status400BadRequest;
		if (validationException.Errors.Count() > 0)
		{
			await context.Response.WriteAsJsonAsync
			(
				GenerateValidationProblemDetails(validationException.Errors)
			);
		}
		else
		{
			await context.Response.WriteAsJsonAsync(new ProblemDetails
			{
				Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-400-bad-request",
				Status = context.Response.StatusCode,
				Title = "Validation error",
				Detail = validationException.Message
			});
		}

	}

	private static async Task HandleNotFoundException(HttpContext context, Exception ex)
	{
		context.Response.StatusCode = StatusCodes.Status404NotFound;

		await context.Response.WriteAsJsonAsync(new ProblemDetails
		{
			Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-404-not-found",
			Status = context.Response.StatusCode,
			Title = "Resource not found",
			Detail = ex.Message
		});
	}

	private static async Task HandleForbiddenAccessException(HttpContext context, Exception ex)
	{
		context.Response.StatusCode = StatusCodes.Status403Forbidden;

		await context.Response.WriteAsJsonAsync(new ProblemDetails
		{
			Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-403-forbidden",
			Status = context.Response.StatusCode,
			Title = "Access denied",
			Detail = ex.Message
		});
	}

	private static async Task HandleUnauthorizedException(HttpContext context, Exception ex)
	{
		context.Response.StatusCode = StatusCodes.Status401Unauthorized;
		await context.Response.WriteAsJsonAsync(new ProblemDetails
		{
			Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-401-unauthorized",
			Status = context.Response.StatusCode,
			Title = "Unauthorized",
			Detail = ex.Message
		});
	}

	private static async Task HandleConflictException(HttpContext context, Exception ex)
	{
		context.Response.StatusCode = StatusCodes.Status409Conflict;
		await context.Response.WriteAsJsonAsync(new ProblemDetails
		{
			Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-409-conflict",
			Status = context.Response.StatusCode,
			Title = "Conflict",
			Detail = ex.Message
		});
	}

	private async Task HandleExternalServiceException(HttpContext context, Exception ex)
	{
		_logger.LogWarning("External service failed for {Method} {Path}: {Message}", context.Request.Method, context.Request.Path, ex.Message);

		context.Response.StatusCode = StatusCodes.Status502BadGateway;
		await context.Response.WriteAsJsonAsync(new ProblemDetails
		{
			Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-502-bad-gateway",
			Status = context.Response.StatusCode,
			Title = "External service error",
			Detail = ex.Message
		});
	}

	#region public static members

	/// <summary>
	/// Creates an instance of <see cref="ValidationProblemDetails"/> with default parameters for a validation error.
	/// </summary>
	/// <param name="failures">A list of validation errors.</param>
	/// <returns>An instance of <see cref="ValidationProblemDetails"/>.</returns>
	public static ValidationProblemDetails GenerateValidationProblemDetails(IEnumerable<ValidationFailure> failures)
	{
		var errors = failures
			.GroupBy(e => e.PropertyName)
			.ToDictionary(
				g => string.Join('.', g.Key.Split('.').Select(JsonResponseExtensions.SerializerOptions.PropertyNamingPolicy!.ConvertName)),
				g => g.Select(e => e.ErrorMessage).ToArray());

		return new ValidationProblemDetails
		{
			Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-400-bad-request",
			Status = StatusCodes.Status400BadRequest,
			Title = "Validation error",
			Errors = errors
		};
	}

	public static ProblemDetails GenerateInternalProblemDetails(string detail)
	{
		return new ProblemDetails
		{
			Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-500-internal-server-error",
			Status = StatusCodes.Status500InternalServerError,
			Title = "Unhandled exception occurred",
			Detail = detail
		};
	}

	#endregion
}
