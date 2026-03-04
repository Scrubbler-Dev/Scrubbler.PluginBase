using System.Reflection;
using Scrubbler.PluginBase.Services;

namespace Scrubbler.PluginBase.Plugin;

public abstract class PluginBase : IPlugin
{
	#region Properties

	/// <summary>
	/// Name of the plugin.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Unique id of the plugin.
	/// </summary>
	public string Id { get; }

	/// <summary>
	/// Description of what the plugin does.
	/// </summary>
	public string Description { get; }

	/// <summary>
	/// Optional uri to the plugin icon.
	/// </summary>
	public Uri? IconUri { get; }

	/// <summary>
	/// The supported platforms of the plugin.
	/// </summary>
	public PlatformSupport SupportedPlatforms { get; }

	/// <summary>
	/// Version of the plugin.
	/// </summary>
	public Version Version { get; }

	protected readonly ILogService _logService;

	#endregion Properties

	#region Construction

	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="logFactory">Factory for creating a log module for this plugin.</param>
	/// <exception cref="InvalidOperationException">If no <see cref="PluginMetadataAttribute"/>
	/// is defined on the inherited class.</exception>
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

	/// <summary>
	/// Gets the viewmodel of this plugin.
	/// </summary>
	/// <returns>ViewModel.</returns>
	public abstract IPluginViewModel GetViewModel();

	private static Version GetFileVersionOrDefault(Assembly asm)
	{
		var s = asm.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
		return Version.TryParse(s, out var v) ? v : new Version(0, 0, 0);
	}
}
