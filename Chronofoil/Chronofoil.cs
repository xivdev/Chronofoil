using Dalamud.Game.Command;
using Chronofoil.UI;
using Dalamud.Plugin.Services;

namespace Chronofoil;

public class Chronofoil
{
    private const string CommandName = "/chronofoil";
    
    private readonly ICommandManager _commandManager;
    private readonly ChronofoilUI _ui;
    
    public Chronofoil(
        ICommandManager commandManager,
        ChronofoilUI ui)
    {
        _commandManager = commandManager;
        _ui = ui;
        
        _commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Chronofoil general command.",
        });
    }
    
    public void Dispose()
    {
        _commandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        switch (args)
        {
            case "monitor":
                _ui.ShowMonitorWindow();    
                break;
            case "config" or "settings":
                _ui.ShowSettingsWindow();
                break;
            default:
                break;
        }
    }
}