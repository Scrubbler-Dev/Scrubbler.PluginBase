// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Scrubbler.PluginBase.Controls;

public sealed partial class ScrobbleTimeControl : UserControl
{
    private readonly DispatcherTimer _refreshTimer = new() { Interval = TimeSpan.FromSeconds(1) };

    public ScrobbleTimeControl()
    {
        InitializeComponent();
        _refreshTimer.Tick += (_, _) => RefreshTime();
        Loaded += (_, _) =>
        {
            RefreshTime();
            _refreshTimer.Start();
        };
        Unloaded += (_, _) => _refreshTimer.Stop();
        DataContextChanged += (_, _) =>
        {
            if (IsLoaded)
                RefreshTime();
        };
    }

    private void RefreshTime()
    {
        if (DataContext is Plugin.ScrobbleTimeViewModel vm)
            vm.RefreshCurrentTime();
    }
}
