using System.Reflection;

namespace HaruyasumiRyokouki.Backend.Common.OptionalType.Supporting;

public static class AssemblyInfo
{
	public static string AssemblyName => ExecutingOrEntryAssembly.GetName().Name ?? string.Empty;
	private static Assembly ExecutingOrEntryAssembly => Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
}
