using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.IoC;
using Chronofoil.Capture;
using Chronofoil.Utility;
using Dalamud.Interface;
using Dalamud.Plugin.Services;

namespace Chronofoil;

public class Chronofoil
{
    private const string CommandName = "/chronofoil";
    
    private readonly ICommandManager _commandManager;
    private readonly UiBuilder _uiBuilder;
    private readonly CaptureSessionManager _captureSessionManager;
    
    public Chronofoil(
        ICommandManager commandManager,
        UiBuilder uiBuilder, 
        CaptureSessionManager captureSessionManager)
    {
        _commandManager = commandManager;
        _uiBuilder = uiBuilder;
        _captureSessionManager = captureSessionManager;
        
        _commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the Chronofoil UI.",
        });

        _uiBuilder.Draw += DrawUI;
        _uiBuilder.OpenConfigUi += DrawConfigUI;
    }
    
    public void Dispose()
    {
        _commandManager.RemoveHandler(CommandName);
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