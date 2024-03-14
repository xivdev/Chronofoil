using System;
using Chronofoil.Capture.IO;
using Chronofoil.Packet;

namespace Chronofoil.Capture;

public class CaptureSessionManager : IDisposable
{
	private readonly PersistentCaptureData _persistentCaptureData;
	
	private readonly CaptureHookManager _hookManager;
	// private readonly ContextManager _contextManager;

	private CaptureSession _session;

	public CaptureSessionManager()
	{
		_persistentCaptureData = new PersistentCaptureData();

		_hookManager = new CaptureHookManager();
		// _contextManager = new ContextManager();

		// TODO: fix networkinitialized hook, it starts on zone init. must be lobby init
		// _hookManager.NetworkInitialized += Restart;
		Begin();
	}

	public void Dispose()
	{
		End();
		_hookManager.Dispose();
		// _contextManager.Dispose();
	}

	private void Restart()
	{
		End();
		Begin();
	}

	public void Begin()
	{
		DalamudApi.PluginLog.Debug("[CaptureSessionManager] Begin!");
		var guid = Guid.NewGuid();
		_session = new CaptureSession(_persistentCaptureData, guid);
		// _contextManager.Reset(guid);
		_hookManager.NetworkEvent += OnNetworkEvent;
		_hookManager.Enable();
		DalamudApi.PluginInterface.UiBuilder.AddNotification($"Capture session started: {guid}!");
	}

	public void End()
	{
		DalamudApi.PluginLog.Debug("[CaptureSessionManager] End!");
		_hookManager.NetworkEvent -= OnNetworkEvent;
		_session?.FinalizeSession();
	}
	
	private void OnNetworkEvent(PacketProto proto, Direction direction, ReadOnlySpan<byte> data)
	{
		_session.WritePacket(proto, direction, data);
	}
}