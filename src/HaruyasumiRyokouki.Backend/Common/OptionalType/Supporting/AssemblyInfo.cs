using System.Reflection;

namespace HaruyasumiRyokouki.Backend.Common.OptionalType.Supporting;

/// <summary>
/// Name of the application assembly, used to find its types.
/// </summary>
public static class AssemblyInfo
{
	/// <summary>
	/// Name of the entry assembly.
	/// </summary>
	public static string AssemblyName => ExecutingOrEntryAssembly.GetName().Name ?? string.Empty;
	private static Assembly ExecutingOrEntryAssembly => Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
}
