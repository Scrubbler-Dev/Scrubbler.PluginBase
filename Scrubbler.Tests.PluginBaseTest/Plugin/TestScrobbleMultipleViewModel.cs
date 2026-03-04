using System.Collections.ObjectModel;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.PluginBase;

namespace Scrubbler.Tests.PluginBaseTest.Plugin;

internal sealed class TestScrobbleMultipleViewModel
    : ScrobbleMultipleViewModelBase<IScrobbableObjectViewModel>
{
    public override Task<IEnumerable<ScrobbleData>> GetScrobblesAsync()
        => Task.FromResult<IEnumerable<ScrobbleData>>([]);

    public void SetScrobbles(IEnumerable<IScrobbableObjectViewModel> scrobbles)
    {
        Scrobbles = new ObservableCollection<IScrobbableObjectViewModel>(scrobbles);
    }
}
