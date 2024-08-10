using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.MyPlugin;

/// <summary>
/// Represents the MyPlugin plugin for Jellyfin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">The application paths.</param>
    /// <param name="xmlSerializer">The XML serializer.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the name of the plugin.
    /// </summary>
    public override string Name => "MyPlugin";

    /// <summary>
    /// Gets the unique identifier for the plugin.
    /// </summary>
    public override Guid Id => Guid.Parse("b5a2d6b3-c5f4-4f88-aed4-5f3e9877d0a6");

    /// <summary>
    /// Gets the description of the plugin.
    /// </summary>
    public override string Description => "A sample plugin for Jellyfin.";

    /// <summary>
    /// Gets the plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <summary>
    /// Gets the plugin configuration.
    /// </summary>
    public PluginConfiguration PluginConfiguration => Configuration;
}
