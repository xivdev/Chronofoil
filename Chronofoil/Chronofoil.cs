﻿using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.IoC;
using Chronofoil.Capture;
using Chronofoil.Utility;

namespace Chronofoil;

public class Chronofoil : IDalamudPlugin
{
    private const string CommandName = "/pcf";

    public static Configuration Configuration { get; private set; }
    
    private readonly CaptureSessionManager _captureSessionManager;

    public Chronofoil([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
    {
        DalamudApi.Initialize(pluginInterface);
        
        Configuration = DalamudApi.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(DalamudApi.PluginInterface);
        
        DalamudApi.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the Chronofoil UI.",
        });

        DalamudApi.PluginInterface.UiBuilder.Draw += DrawUI;
        DalamudApi.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        
        var isOodleInitialized = Util.IsOodleInitialized();
        if (isOodleInitialized)
        {
            DalamudApi.PluginLog.Error("Oodle has already been initialized. The plugin will load, but cannot function until Oodle is reinitialized.");
        }

        _captureSessionManager = new CaptureSessionManager();
    }
    
    public void Dispose()
    {
        DalamudApi.CommandManager.RemoveHandler(CommandName);
        _captureSessionManager.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        
    }

    private void DrawUI()
    {
        
    }

    private void DrawConfigUI()
    {
        
    }
}