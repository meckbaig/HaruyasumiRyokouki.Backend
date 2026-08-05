using Asp.Versioning;
using FluentValidation;
using HaruyasumiRyokouki.Backend.Common.Behaviours;
using HaruyasumiRyokouki.Backend.Common.Conventions;
using HaruyasumiRyokouki.Backend.Common.Handlers;
using HaruyasumiRyokouki.Backend.Common.OptionalType.Supporting.Asp;
using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Common.Options.Configurators.Swagger;
using HaruyasumiRyokouki.Backend.Common.Options.Loggers;
using HaruyasumiRyokouki.Backend.Common.Options.Validators;
using HaruyasumiRyokouki.Backend.Common.Options.Validators.Loggers;
using HaruyasumiRyokouki.Backend.DbContexts;
using HaruyasumiRyokouki.Backend.Services;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Globalization;
using System.Net;
using System.Reflection;
using WebDav;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;


namespace HaruyasumiRyokouki.Backend.Extensions;

internal static class ServiceCollectionExtensions
{
	internal static IServiceCollection AddAppOptions(this IServiceCollection services)
	{
		services
			.AddOptionsWithValidateOnStart<SwaggerAuthOptions>()
			.BindConfiguration(SwaggerAuthOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<MediaStorageOptions>()
			.BindConfiguration(MediaStorageOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<MediaPreviewOptions>()
			.BindConfiguration(MediaPreviewOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<MediaFormatOptions>()
			.BindConfiguration(MediaFormatOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<LocalStorageOptions>()
			.BindConfiguration(LocalStorageOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<WebDavOptions>()
			.BindConfiguration(WebDavOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<AiApiOptions>()
			.BindConfiguration(AiApiOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<ApplicationOptions>()
			.BindConfiguration(ApplicationOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<ConnectionStringsOptions>()
			.BindConfiguration(ConnectionStringsOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<SeqOptions>()
			.BindConfiguration(SeqOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<ConsoleLogOptions>()
			.BindConfiguration(ConsoleLogOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<DebugLogOptions>()
			.BindConfiguration(DebugLogOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<FileLogOptions>()
			.BindConfiguration(FileLogOptions.ConfigurationSectionName);

		return services;
	}

	internal static IServiceCollection AddAppOptionsValidators(this IServiceCollection services)
	{
		services.AddSingleton<IValidateOptions<SwaggerAuthOptions>, SwaggerAuthOptionsValidator>();
		services.AddSingleton<IValidateOptions<MediaStorageOptions>, MediaStorageOptionsValidator>();
		services.AddSingleton<IValidateOptions<MediaPreviewOptions>, MediaPreviewOptionsValidator>();
		services.AddSingleton<IValidateOptions<MediaFormatOptions>, MediaFormatOptionsValidator>();
		services.AddSingleton<IValidateOptions<LocalStorageOptions>, LocalStorageOptionsValidator>();
		services.AddSingleton<IValidateOptions<WebDavOptions>, WebDavOptionsValidator>();
		services.AddSingleton<IValidateOptions<AiApiOptions>, AiApiOptionsValidator>();
		services.AddSingleton<IValidateOptions<ApplicationOptions>, ApplicationOptionsValidator>();
		services.AddSingleton<IValidateOptions<ConnectionStringsOptions>, ConnectionStringsOptionsValidator>();
		services.AddSingleton<IValidateOptions<SeqOptions>, SeqOptionsValidator>();
		services.AddSingleton<IValidateOptions<ConsoleLogOptions>, ConsoleLogOptionsValidator>();
		services.AddSingleton<IValidateOptions<DebugLogOptions>, DebugLogOptionsValidator>();
		services.AddSingleton<IValidateOptions<FileLogOptions>, FileLogOptionsValidator>();

		return services;
	}

	internal static IServiceCollection AddDatabaseConnection(this IServiceCollection services)
	{
		var connectionOptions = services
			.BuildServiceProvider()
			.GetRequiredService<IOptions<ConnectionStringsOptions>>()
			.Value;
		var applicationOptions = services
			.BuildServiceProvider()
			.GetRequiredService<IOptions<ApplicationOptions>>()
			.Value;

		return services.AddDbContext<IAppDbContext, AppDbContext>
		(
			options => options.UseNpgsql
			(
				connectionOptions.Default,
				options => options.EnableRetryOnFailure(
					maxRetryCount: applicationOptions.CheckDbRetryCount,
					maxRetryDelay: TimeSpan.FromSeconds(applicationOptions.CheckDbRetryDelay),
					errorCodesToAdd: null
				)
			)
		);
	}

	internal static IServiceCollection AddControllersWithJsonNamingPolicy(this IServiceCollection services)
	{
		services.Configure<ApiBehaviorOptions>(options =>
		{
			options.SuppressModelStateInvalidFilter = true;
		});
		services
			.AddControllers(options =>
			{
				options.Conventions.Add(new CamelCaseControllerNameConvention());
				options.Conventions.Add(new CamelCaseQueryParameterConvention());
				options.ModelBinderProviders.Insert(0, new OptionalModelBinderProvider());
			})
			.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.PropertyNamingPolicy = JsonResponseExtensions.SerializerOptions.PropertyNamingPolicy;
				options.JsonSerializerOptions.DictionaryKeyPolicy = JsonResponseExtensions.SerializerOptions.DictionaryKeyPolicy;
				options.JsonSerializerOptions.PropertyNameCaseInsensitive = JsonResponseExtensions.SerializerOptions.PropertyNameCaseInsensitive;
			});
		services.Configure<JsonOptions>(options =>
		{
			options.SerializerOptions.PropertyNamingPolicy = JsonResponseExtensions.SerializerOptions.PropertyNamingPolicy;
			options.SerializerOptions.DictionaryKeyPolicy = JsonResponseExtensions.SerializerOptions.DictionaryKeyPolicy;
		});
		return services;
	}

	internal static IServiceCollection AddMediatRFromAssembly(this IServiceCollection services)
	{
		return services.AddMediatR(c =>
		{
			c.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
			c.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
			c.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
			c.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LocalizationBehavior<,>));
		});
	}

	internal static IServiceCollection AddAutoMapperFromAssembly(this IServiceCollection services)
	{
		return services.AddAutoMapper(Assembly.GetExecutingAssembly());
	}

	internal static IServiceCollection AddValidatorsFromAssembly(this IServiceCollection services)
	{
		ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en");
		services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
		return services;
	}

	internal static IServiceCollection AddSwaggerSupport(this IServiceCollection services)
	{
		services.ConfigureOptions<ConfigureSwaggerOptions>();
		services.AddEndpointsApiExplorer();
		services.AddApiVersioning(options =>
		{
			options.AssumeDefaultVersionWhenUnspecified = true;
			options.DefaultApiVersion = new ApiVersion(1);
			options.ReportApiVersions = true;
			options.ApiVersionReader = new UrlSegmentApiVersionReader();
		}).AddApiExplorer(options =>
			{
				options.GroupNameFormat = "'v'VVV";
				options.SubstituteApiVersionInUrl = true;
			}
		);
		services.AddSwaggerGen(options =>
		{
			options.IncludeXmlComments(Assembly.GetExecutingAssembly());
			options.EnableAnnotations();
			options.DocInclusionPredicate((docName, apiDesc) =>
			{
				var groupName = apiDesc.GroupName;
				return groupName == docName;
			});
		});

		return services;
	}

	internal static IServiceCollection AddFileStorageProvider(this IServiceCollection services)
	{
		var storageOptions = services
			.BuildServiceProvider()
			.GetRequiredService<IOptions<MediaStorageOptions>>()
			.Value;

		switch (storageOptions!.Provider)
		{
			case MediaStorageOptions.FileStorageProvider.Local:
				services.AddSingleton<IFileStorage, LocalFileStorageService>();
				break;

			case MediaStorageOptions.FileStorageProvider.WebDav:
				services.AddSingleton(sp =>
				{
					var options = sp.GetRequiredService<IOptions<WebDavOptions>>().Value;

					var clientParams = new WebDavClientParams
					{
						BaseAddress = new Uri(options.Endpoint),
						Credentials = new NetworkCredential(
							options.Username,
							options.Password)
					};

					return new WebDavClient(clientParams);
				});

				services.AddSingleton<IFileStorage, WebDavFileStorageService>();
				break;

			default:
				throw new NotSupportedException(
					$"Unsupported file storage provider: {storageOptions.Provider}");
		}

		return services;
	}

	internal static IServiceCollection AddMediaPreviewProvider(this IServiceCollection services)
	{
		var previewOptions = services
			.BuildServiceProvider()
			.GetRequiredService<IOptions<MediaPreviewOptions>>()
			.Value;

		switch (previewOptions.Provider)
		{
			case MediaPreviewOptions.MediaPreviewProvider.Nextcloud:
				services.AddSingleton<IMediaPreviewService, NextcloudMediaPreviewService>();
				break;
			case MediaPreviewOptions.MediaPreviewProvider.Imgproxy:
				services.AddSingleton<IMediaPreviewService, ImgproxyMediaPreviewService>();
				break;
			default:
				throw new NotSupportedException(
					$"Unsupported media preview provider: {previewOptions.Provider}");
		}

		return services;
	}

	internal static IServiceCollection AddAppHealthChecks(this IServiceCollection services)
	{
		var connectionOptions = services
			.BuildServiceProvider()
			.GetRequiredService<IOptions<ConnectionStringsOptions>>()
			.Value;

		services.AddHealthChecks()
			.AddNpgSql(connectionOptions.Default, name: "database");

		return services;
	}

	internal static IServiceCollection AddOpenTelemetryMetrics(this IServiceCollection services)
	{
		services.AddOpenTelemetry()
			.WithMetrics(metrics =>
			{
				metrics
					.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Assembly.GetExecutingAssembly().GetName().Name ?? "HaruyasumiRyokouki.Backend"))
					.AddAspNetCoreInstrumentation()
					.AddRuntimeInstrumentation()
					.AddPrometheusExporter(); // <-- creates /metrics
			});

		return services;
	}

	internal static IServiceCollection AddBasicAuthentication(this IServiceCollection services)
	{
		services
			.AddAuthentication("Basic")
			.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);
		return services;
	}

	internal static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
	{
		services.AddCors(options =>
		{
			options.AddPolicy("AllowAllOrigins",
				builder => builder
					.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader());
		});
		return services;
	}
}
