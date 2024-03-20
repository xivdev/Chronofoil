using Chronofoil.Capture;
using Chronofoil.Capture.Context;
using Chronofoil.Capture.IO;
using Chronofoil.Lobby;
using Chronofoil.Monitor;
using Chronofoil.UI;
using Chronofoil.UI.Windows;
using Chronofoil.Utility;
using Dalamud.Game;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Chronofoil;

/// <summary>
/// Bootstrap class for the Chronofoil plugin.
/// </summary>
public class Plugin : IDalamudPlugin
{
	private readonly IHost _host;
	
	public Plugin(DalamudPluginInterface pi)
	{
		var builder = new HostBuilder();
		builder
			.UseContentRoot(pi.ConfigDirectory.FullName)
			.ConfigureServices((hostContext, services) =>
		{
			var configuration = pi.GetPluginConfig() as Configuration ?? new Configuration();
			configuration.Initialize(pi);
			
			services
				.AddExistingService(pi.UiBuilder)
				.AddExistingService(configuration)
				.AddDalamudService<IClientState>(pi)
				.AddDalamudService<ICommandManager>(pi)
				.AddDalamudService<IGameGui>(pi)
				.AddDalamudService<ISigScanner>(pi)
				.AddDalamudService<IGameInteropProvider>(pi)
				.AddDalamudService<IPluginLog>(pi)
				.AddDalamudService<INotificationManager>(pi)
				.AddSingleton<MultiSigScanner>()
				.AddSingleton<Configuration>()
				.AddSingleton<CaptureHookManager>()
				.AddSingleton<CaptureSessionManager>()
				.AddSingleton<ContextManager>()
				.AddSingleton<LobbyEncryptionProvider>()
				.AddSingleton<MainWindow>()
				.AddSingleton<MonitorWindow>()
				.AddSingleton<SettingsWindow>()
				.AddSingleton<ChronofoilUI>()
				.AddSingleton<MonitorSessionManager>()
				.AddSingleton<Chronofoil>();
		});
		
		_host = builder.Build();
		_host.Start();
		Logging.Initialize(_host.Services.GetService<IPluginLog>());
		_host.Services.GetService<Chronofoil>();
		_host.Services.GetService<CaptureSessionManager>();
	}
	
	public void Dispose()
	{
		_host?.StopAsync().GetAwaiter().GetResult();
		_host?.Dispose();
	}
}