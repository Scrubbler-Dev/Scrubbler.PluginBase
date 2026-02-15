namespace Scrubbler.PluginBase.Discord
{
  public interface IDiscordRichPresence
  {
    /// <summary>
    /// Publishes presence state to the host.
    /// </summary>
    void Publish(NowPlayingPresence presence);

    /// <summary>
    /// Clears the current presence.
    /// </summary>
    void Clear();
  }
}
