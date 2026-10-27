using Asp.Versioning.ApiExplorer;
using HaruyasumiRyokouki.Backend.Common.Handlers.Exceptions;
using HaruyasumiRyokouki.Backend.Common.HealthChecks;
using HaruyasumiRyokouki.Backend.Common.Helpers;
using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Text;
using System.Text.Json;

namespace HaruyasumiRyokouki.Backend.Extensions;

internal static class ApplicationBuilderExtensions
{
	/// <summary>
	/// Adds a login requirement before accessing Swagger.
	/// </summary>
	/// <param name="builder"></param>
	/// <returns></returns>
	public static IApplicationBuilder RequireSwaggerAuthorization(this IApplicationBuilder builder)
	{
		var options = builder.ApplicationServices
			.GetRequiredService<IOptions<SwaggerAuthOptions>>()
			.Value;
		var loginAttemptLimiter = builder.ApplicationServices.GetRequiredService<ILoginAttemptLimiter>();
		return options.RequireLogin
			? builder
			.UseWhen(context => context.Request.Path.StartsWithSegments("/swagger"), appBuilder =>
			{
				appBuilder.Use(async (context, next) =>
				{
					string clientKey = "swagger:" + context.Connection.RemoteIpAddress;
					if (loginAttemptLimiter.IsBlocked(clientKey))
					{
						context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
						return;
					}

					bool hasHeader = context.Request.Headers.ContainsKey("Authorization");
					if (BasicCredentials.TryParse(context.Request.Headers.Authorization, out var username, out var password)
						&& BasicCredentials.AreEqual(username, password, options.Username, options.Password))
					{
						loginAttemptLimiter.Reset(clientKey);
						await next.Invoke();
						return;
					}

					// A request without the header is the browser asking for credentials, not a failed attempt.
					if (hasHeader)
						loginAttemptLimiter.RegisterFailure(clientKey);

					context.Response.Headers["WWW-Authenticate"] = "Basic";
					context.Response.StatusCode = 401;
				});
			})
			: builder;
	}

	/// <summary>
	/// Enables processing of X-Forwarded-For / X-Forwarded-Proto headers from known proxies,
	/// so the client IP and scheme are correct when the service is behind a reverse proxy.
	/// Must run before any middleware that uses the client IP or scheme.
	/// </summary>
	/// <param name="builder">The application configuration builder</param>
	/// <returns>The configured application configuration builder</returns>
	public static IApplicationBuilder UseForwardedHeadersFromKnownProxies(this IApplicationBuilder builder)
	{
		var appOptions = builder.ApplicationServices.GetRequiredService<IOptions<ApplicationOptions>>().Value;
		var forwardedHeadersOptions = new ForwardedHeadersOptions
		{
			ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
		};
		appOptions.KnownProxies
			.Select(IPAddress.Parse)
			.ToList()
			.ForEach(forwardedHeadersOptions.KnownProxies.Add);

		return builder.UseForwardedHeaders(forwardedHeadersOptions);
	}

	/// <summary>
	/// Adds and configures Swagger documentation in the application
	/// </summary>
	/// <param name="builder">The application configuration builder</param>
	/// <returns>The configured application configuration builder</returns>
	public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder builder)
	{
		// Configure Swagger URLs for correct operation behind a reverse proxy,
		// using information from the HTTP request (scheme, host, base path)
		builder.UseSwagger(options =>
		{
			options.PreSerializeFilters.Add((swagger, httpReq) =>
			{
				swagger.Servers = new List<OpenApiServer>
				{
					new() { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}{httpReq.PathBase.Value?.TrimEnd('/')}" }
				};
			});
		});

		// Configure the Swagger UI interface, adding endpoints for each API version.
		// Use the IApiVersionDescriptionProvider to get information about all API versions
		// and register them in SwaggerUI with the corresponding URLs and names.
		builder.UseSwaggerUI(options =>
		{
			var provider = builder.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
			foreach (var desc in provider.ApiVersionDescriptions)
			{
				options.SwaggerEndpoint($"{desc.GroupName}/swagger.json", $"{desc.GroupName}");
			}

			options.RoutePrefix = "swagger";
		});
		return builder;
	}

	internal static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
	{
		var exceptionMiddleware = ActivatorUtilities.CreateInstance<CustomExceptionHandler>(app.ApplicationServices);
		return app.Use(async (context, next) =>
		{
			try
			{
				await next(context);
			}
			catch (Exception ex)
			{
				await exceptionMiddleware.TryHandleAsync(context, ex, new CancellationToken());
			}
		});
	}

	internal static IEndpointRouteBuilder MapHealthCheckEndpoint(this IEndpointRouteBuilder endpoints, string path = "/health")
	{
		endpoints.MapHealthChecks(path, new HealthCheckOptions
		{
			ResponseWriter = async (context, report) =>
			{
				var status = HealthCheckTags.GetOverallStatus(report);
				context.Response.StatusCode = status == HealthStatus.Unhealthy
					? StatusCodes.Status503ServiceUnavailable
					: StatusCodes.Status200OK;
				context.Response.ContentType = "application/json";

				var response = new HealthCheckResponse
				{
					Status = status.ToString(),
					Duration = report.TotalDuration.TotalMilliseconds,
					Checks = report.Entries.Select(entry => new HealthCheckComponent
					{
						Entry = entry.Key,
						Status = entry.Value.Status.ToString(),
						Description = entry.Value.Description,
						ErrorMessage = entry.Value.Exception?.Message
					}).ToList()
				};

				var json = JsonSerializer.Serialize(response, JsonResponseExtensions.SerializerOptions);
				await context.Response.WriteAsync(json);
			}
		});

		return endpoints;
	}

	internal static WebApplication MapOpenTelemetryMetricsEndpoint(this WebApplication app, string path = "/metrics")
	{
		app.MapPrometheusScrapingEndpoint(path);

		return app;
	}
}
