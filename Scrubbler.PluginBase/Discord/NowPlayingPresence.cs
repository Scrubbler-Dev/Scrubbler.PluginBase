namespace Scrubbler.PluginBase.Discord
{
  public sealed record NowPlayingPresence(
      string Details,
      string? State = null,
      string? LargeImageKey = null,
      string? LargeImageText = null,
      string? SmallImageKey = null,
      string? SmallImageText = null,
      DateTimeOffset? StartTimestamp = null);
}
