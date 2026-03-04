using Windows.System;

namespace Scrubbler.PluginBase.Services;

public class LinkOpenerService : ILinkOpenerService
{
    public async Task OpenLink(string url)
    {
        await Launcher.LaunchUriAsync(new Uri(url));
    }
}
