using HaruyasumiRyokouki.Backend.Common;
using HaruyasumiRyokouki.Backend.Common.Behaviours;
using HaruyasumiRyokouki.Backend.Extensions;
using MediatR;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = builder.CreateBootstrapLogger();

try
{
	builder.Services.AddAppOptionsValidators();
	builder.Services.AddAppOptions();
	Log.Logger = builder.CreateCompleteLogger();
	builder.Logging.ClearProviders().AddSerilog(Log.Logger);
	builder.Services.AddDatabaseConnection();
	builder.Services.AddFileStorageClient();
	builder.Services.AddControllersWithJsonNamingPolicy();
	builder.Services.AddHttpContextAccessor();	
	builder.Services.AddMediatRFromAssembly();
	builder.Services.AddAutoMapperFromAssembly();
	builder.Services.AddValidatorsFromAssembly();
	builder.Services.AddSwaggerSupport();
	builder.Services.AddAppHealthChecks();
	builder.Services.AddOpenTelemetryMetrics();
	builder.Services.AddBasicAuthentication();
	builder.Services.AddAuthorization();
	builder.Services.AddCorsConfiguration();

	var app = builder.Build();

	app.RequireSwaggerAuthorization();
	app.UseSwaggerDocumentation();
	app.UseCustomExceptionHandler();
	app.UseCors("AllowAllOrigins");
	app.MapControllers();
	app.MapHealthCheckEndpoint();
	app.MapOpenTelemetryMetricsEndpoint();
	app.UseAuthentication();
	app.UseAuthorization();

	app.Run();

	return ExitCode.Normal;
}
catch (OptionsValidationException ex)
{
	Log.Error(ex.Message);

	return ExitCode.NoConfiguration;
}
catch (Exception ex)
{
	Log.Fatal(ex, "Fatal termination of application.");

	return ExitCode.Fatal;
}
finally
{
	Log.CloseAndFlush();
}

/// <summary>
/// <see langword="partial"/> initialization of the <see cref="Program"/> class for use in tests.
/// </summary>
public partial class Program { }
