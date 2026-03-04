namespace Scrubbler.PluginBase.Services;

public interface ILinkOpenerService
{
    Task OpenLink(string url);
}
