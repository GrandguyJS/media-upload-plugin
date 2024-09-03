using System;
using System.Collections.Generic;
using System.Globalization;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.RemoteUploadBeta;

/// <summary>
/// Represents the RemoteUpload plugin for Jellyfin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
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

        ConfigurationChanged += OnConfigurationChanged;
    }

    /// <summary>
    /// Gets the name of the plugin.
    /// </summary>
    public override string Name => "RemoteUploadBeta";

    /// <summary>
    /// Gets the unique identifier for the plugin.
    /// </summary>
    public override Guid Id => Guid.Parse("b5a2d6b3-c5f4-4f88-aed4-5f3e9877d0a5");

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

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = this.Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Pages.config.html", GetType().Namespace)
            },
            
            new PluginPageInfo
            {
                Name = @"RemoteFileUpload - Beta",
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Pages.menu.html", GetType().Namespace),
                EnableInMainMenu = true
            }
        };
    }

    private void OnConfigurationChanged(object? sender, BasePluginConfiguration e)
    {
    }
}
