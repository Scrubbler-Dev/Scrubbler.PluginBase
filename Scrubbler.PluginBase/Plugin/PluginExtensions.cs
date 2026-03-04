using Scrubbler.PluginBase.Plugin.Account;

namespace Scrubbler.PluginBase.Plugin;

public static class PluginExtensions
{
    public static string ResolvePluginType(this IPlugin plugin)
    {
        if (plugin is IAccountPlugin)
            return "Account Plugin";
        if (plugin is IScrobblePlugin || plugin is IAutoScrobblePlugin)
            return "Scrobble Plugin";

        return "Plugin";
    }
}
