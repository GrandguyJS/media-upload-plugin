using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.RemoteUploadBeta;

/// <summary>
/// Represents the configuration settings for the RemoteUpload plugin.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        uploaddir = "/";
    }

    /// <summary>
    /// Executable for youtube-dl/youtube-dlp
    /// </summary>
    public string uploaddir { get; set; }
}
