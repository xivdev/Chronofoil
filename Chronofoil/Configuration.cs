using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.IO;

namespace Chronofoil;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public string StorageDirectory { get; set; } = Path.Combine(Path.GetTempPath(), "chronofoil");

    [NonSerialized] private DalamudPluginInterface _pluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
    }

    public void Save()
    {
        _pluginInterface.SavePluginConfig(this);
    }
}