namespace HaruyasumiRyokouki.Backend.Common.Helpers;

/// <summary>
/// Builds patterns for SQL <c>LIKE</c> / <c>ILIKE</c> from user input.
/// </summary>
/// <remarks>
/// <c>%</c> and <c>_</c> are wildcards, so they are escaped for the input to be matched literally.
/// Npgsql generates <c>ESCAPE ''</c> (escaping disabled) unless an escape character is passed explicitly,
/// so patterns from this class must be used as <c>EF.Functions.ILike(value, pattern, LikePattern.EscapeCharacter)</c>.
/// </remarks>
internal static class LikePattern
{
	/// <summary>
	/// Escape character used in patterns built by this class.
	/// </summary>
	public const string EscapeCharacter = @"\";

	/// <summary>
	/// Escapes wildcards in <paramref name="text"/>, so the pattern matches the text exactly
	/// (case-insensitively when used with <c>ILIKE</c>).
	/// </summary>
	public static string Exact(string text)
	{
		return text
			.Replace(@"\", @"\\")
			.Replace("%", @"\%")
			.Replace("_", @"\_");
	}

	/// <summary>
	/// Returns a pattern that matches any value containing <paramref name="text"/> literally.
	/// </summary>
	public static string Contains(string text)
	{
		return $"%{Exact(text)}%";
	}
}
