using Chronofoil.Capture;
using Chronofoil.Capture.Context;
using Chronofoil.Capture.IO;
using Chronofoil.Lobby;
using Chronofoil.Utility;
using Dalamud.Game;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Chronofoil;

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
				.AddSingleton<MultiSigScanner>()
				.AddSingleton<Configuration>()
				.AddSingleton<CaptureHookManager>()
				.AddSingleton<CaptureSessionManager>()
				.AddSingleton<ContextManager>()
				.AddSingleton<LobbyEncryptionProvider>()
				.AddSingleton<Chronofoil>();
		});
		
		_host = builder.Build();
		_host.Start();
		Logging.Initialize(_host.Services.GetService<IPluginLog>());
		_host.Services.GetService<Chronofoil>();
	}
	
	public void Dispose()
	{
		_host?.StopAsync().GetAwaiter().GetResult();
		_host?.Dispose();
	}
}