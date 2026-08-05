using HaruyasumiRyokouki.Backend.Common.Options;
using HaruyasumiRyokouki.Backend.Models.InternalDtos;
using HaruyasumiRyokouki.Backend.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Text;
using WebDav;

namespace HaruyasumiRyokouki.Backend.Services;

internal class WebDavFileStorageService : IFileStorage
{
	private readonly WebDavClient _client;
	private readonly ILogger<WebDavFileStorageService> _logger;
	private readonly WebDavOptions _options;

	public WebDavFileStorageService(WebDavClient client, ILogger<WebDavFileStorageService> logger, IOptions<WebDavOptions> options)
	{
		_client = client;
		_logger = logger;
		_options = options.Value;
	}

	public Task<MediaInput> GetMediaInputAsync(string filePath, CancellationToken cancellationToken = default)
	{
		byte[] textAsBytes = Encoding.UTF8.GetBytes($"{_options.Username}:{_options.Password}");
		string base64String = Convert.ToBase64String(textAsBytes);
		return Task.FromResult(new MediaInput
		{
			Input = new Uri(new Uri(_options.Endpoint), filePath).ToString(),
			Headers =
			{
				["Authorization"] = $"Basic {base64String}"
			}
		});
	}

	public async Task<Stream> OpenReadAsync(string fileName, CancellationToken cancellationToken = default)
	{
		var temp = Path.GetTempFileName();

		var response = await _client.GetRawFile(fileName);

		if (!response.IsSuccessful)
		{
			_logger.LogError("Failed to download {FileName}", fileName);
			throw new Exception($"Download failed: {response.StatusCode}");
		}

		return new WebDavStream(response);

		await using (var local = File.Create(temp))
		{
			await response.Stream.CopyToAsync(local, cancellationToken);
		}

		return new FileStream(
			temp,
			FileMode.Open,
			FileAccess.Read,
			FileShare.Read,
			81920,
			FileOptions.DeleteOnClose);
	}

	public async Task SaveFileAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
	{
		await _client.PutFile(fileName, content);
	}

	public async Task<IReadOnlyCollection<StorageFile>> GetFilesAsync(CancellationToken cancellationToken)
	{
		var result = await _client.Propfind("");

		return result.Resources
			.Where(r => !r.IsCollection && r.LastModifiedDate != null)
			.Select(r => new StorageFile
			{
				FileName = Path.GetFileName(r.Uri),
				Created = DateTime.SpecifyKind(r.LastModifiedDate!.Value, DateTimeKind.Unspecified)
			})
			.ToList();
	}

	public async Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
	{
		if (await ExistsAsync(fileName, cancellationToken))
		{
			var result = await _client.Delete(fileName);

			if (!result.IsSuccessful)
			{
				_logger.LogError("Failed to delete {FileName}", fileName);
				throw new Exception($"Delete failed: {result.StatusCode}");
			}
		}

		_logger.LogInformation("Deleted {FileName}", fileName);
	}

	public async Task<bool> ExistsAsync(string fileName, CancellationToken cancellationToken = default)
	{
		var result = await _client.Propfind(fileName);

		return result.IsSuccessful;
	}

	private sealed class WebDavStream : Stream
	{
		private readonly WebDavStreamResponse _response;

		public WebDavStream(WebDavStreamResponse response)
		{
			_response = response;
		}

		public override bool CanRead => _response.Stream.CanRead;

		public override bool CanSeek => _response.Stream.CanSeek;

		public override bool CanWrite => _response.Stream.CanWrite;

		public override long Length => _response.Stream.Length;
		public override long Position { get => _response.Stream.Position; set => _response.Stream.Position = value; }

		public override void Flush()
		{
			_response.Stream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _response.Stream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _response.Stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_response.Stream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_response.Stream.Write(buffer, offset, count);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_response.Dispose();

			base.Dispose(disposing);
		}
	}
}
