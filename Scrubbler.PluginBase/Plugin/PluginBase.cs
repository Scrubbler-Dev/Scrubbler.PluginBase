using System.Reflection;
using Scrubbler.PluginBase.Services;

namespace Scrubbler.PluginBase.Plugin;

public abstract class PluginBase : IPlugin
{
	#region Properties

	public string Name { get; }

	public string Id { get; }

	public string Description { get; }

	public Uri? IconUri { get; }

	public PlatformSupport SupportedPlatforms { get; }

	public Version Version { get; }

	protected readonly ILogService _logService;

	#endregion Properties

	#region Construction

	protected PluginBase(IModuleLogServiceFactory logFactory)
	{
		var type = GetType();
		var asm = type.Assembly;

		var attribute = type.GetCustomAttribute<PluginMetadataAttribute>()
			?? throw new InvalidOperationException($"{type.Name} must have [PluginMetadata] attribute.");

		Name = attribute.Name;
		Id = type.FullName!.ToLowerInvariant();
		Description = attribute.Description;

		IconUri = new Uri(Path.Combine(
			Path.GetDirectoryName(asm.Location)!,
			"icon.png"));

		SupportedPlatforms = attribute.SupportedPlatforms;
		_logService = logFactory.Create(Name);

		Version = GetFileVersionOrDefault(asm);
	}

	#endregion Construction

	public abstract IPluginViewModel GetViewModel();

	private static Version GetFileVersionOrDefault(Assembly asm)
	{
		var s = asm.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
		return Version.TryParse(s, out var v) ? v : new Version(0, 0, 0);
	}
}
