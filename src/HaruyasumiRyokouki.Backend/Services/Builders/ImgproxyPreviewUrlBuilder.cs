using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace HaruyasumiRyokouki.Backend.Services.Builders;

public class ImgproxyPreviewUrlBuilder : AbstractUrlBuilder
{
	public override HashSet<string> RequiredTokens =>
	[
		"signature",
		"fileFormat",
		"fileName",
		"xAxis",
		"yAxis"
	];

	private const string InsecureSignature = "insecure";
	private readonly bool _insecure;
	private readonly byte[] _key;
	private readonly byte[] _salt;

	public ImgproxyPreviewUrlBuilder(string template, bool insecure, string? key = null, string? salt = null) : base(template)
	{
		_insecure = insecure;
		if (!_insecure)
		{
			_key = Convert.FromHexString(key ?? "");
			_salt = Convert.FromHexString(salt ?? "");
		}
	}

	public string BuildRaw(string fileName)
	{
		string fileFormat = Path.GetExtension(fileName).TrimStart('.').ToLower();
		string signature;

		var templateForRawResult = Regex.Replace(
			_template,
			@"/[^/]*(\{xAxis\}|\{yAxis\})[^/]*/",
			"/raw:1/"
		);

		if (_insecure)
		{
			signature = InsecureSignature;
		}
		else
		{
			string payloadTemplate = templateForRawResult.Split("{signature}").Last();

			string payloadString = payloadTemplate
				.Replace("{fileFormat}", fileFormat)
				.Replace("{fileName}", Uri.EscapeDataString(fileName));

			signature = GenerateSignature(payloadString);
		}

		return templateForRawResult
			.Replace("{signature}", signature)
			.Replace("{fileFormat}", fileFormat)
			.Replace("{fileName}", Uri.EscapeDataString(fileName));
	}

	public string Build(string fileName, int xAxis, int yAxis)
	{
		string fileFormat = Path.GetExtension(fileName).TrimStart('.').ToLower();
		string signature;

		if (_insecure)
		{
			signature = InsecureSignature;
		}
		else
		{
			string payloadTemplate = _template.Split("{signature}").Last();

			string payloadString = payloadTemplate
				.Replace("{fileFormat}", fileFormat)
				.Replace("{fileName}", Uri.EscapeDataString(fileName))
				.Replace("{xAxis}", xAxis.ToString())
				.Replace("{yAxis}", yAxis.ToString());

			signature = GenerateSignature(payloadString);
		}

		return _template
			.Replace("{signature}", signature)
			.Replace("{fileFormat}", fileFormat)
			.Replace("{fileName}", Uri.EscapeDataString(fileName))
			.Replace("{xAxis}", xAxis.ToString())
			.Replace("{yAxis}", yAxis.ToString());
	}

	private string GenerateSignature(string payloadString)
	{
		using var hmac = new HMACSHA256(_key);

		var signaturePath = payloadString.StartsWith('/')
		   ? payloadString
		   : "/" + payloadString;

		var data = _salt
			.Concat(Encoding.UTF8.GetBytes(signaturePath))
			.ToArray();

		var hash = hmac.ComputeHash(data);

		return Convert
			.ToBase64String(hash)
			.TrimEnd('=')
			.Replace('+', '-')
			.Replace('/', '_');
	}
}
