using Chronofoil.UI.Windows;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;

namespace Chronofoil.UI;

public class ChronofoilUI
{
	private readonly IPluginLog _log;
	private readonly UiBuilder _uiBuilder;
	
	private readonly WindowSystem _windowSystem;

	private readonly MainWindow _mainWindow;
	private readonly MonitorWindow _monitorWindow;
	private readonly SettingsWindow _settingsWindow;

	public ChronofoilUI(
		IPluginLog log,
		MainWindow mainWindow,
		MonitorWindow monitorWindow,
		SettingsWindow settingsWindow,
		UiBuilder uiBuilder
	)
	{
		_log = log;
		_uiBuilder = uiBuilder;

		_mainWindow = mainWindow;
		_monitorWindow = monitorWindow;
		_settingsWindow = settingsWindow;
		
		_windowSystem = new WindowSystem("Chronofoil");
		_windowSystem.AddWindow(mainWindow);
		_windowSystem.AddWindow(monitorWindow);
		_windowSystem.AddWindow(settingsWindow);

		_uiBuilder.Draw += _windowSystem.Draw;
		_uiBuilder.OpenMainUi += ShowMonitorWindow;
		_uiBuilder.OpenConfigUi += ShowSettingsWindow;
	}
	
	public void ShowMainWindow() => _mainWindow.IsOpen = true;
	public void ToggleMainWindow() => _mainWindow.IsOpen = !_mainWindow.IsOpen;
	public void ShowMonitorWindow() => _monitorWindow.IsOpen = true;
	public void ToggleMonitorWindow() => _monitorWindow.IsOpen = !_monitorWindow.IsOpen;
	public void ShowSettingsWindow() => _settingsWindow.IsOpen = true;
	public void ToggleSettingsWindow() => _settingsWindow.IsOpen = !_settingsWindow.IsOpen;
}