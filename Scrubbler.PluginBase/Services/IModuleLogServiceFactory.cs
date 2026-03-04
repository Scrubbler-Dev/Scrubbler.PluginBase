namespace Scrubbler.PluginBase.Services;

public interface IModuleLogServiceFactory
{
    ILogService Create(string moduleName);
}
