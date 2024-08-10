﻿using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.RemoteUpload;

/// <summary>
/// Represents the configuration settings for the RemoteUpload plugin.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    // Define properties for plugin configuration if needed

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        // Initialize properties with default values if necessary
        IgnoreString = string.Empty;
    }

    /// <summary>
    /// Gets or sets a value of the patterns we want to ignore.
    /// </summary>
    public string IgnoreString { get; set; }
}
