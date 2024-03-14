using System;
using Chronofoil.Capture.Context;
using Chronofoil.Capture.IO;
using Chronofoil.Packet;

namespace Chronofoil.Capture;

public class CaptureSessionManager : IDisposable
{
	private readonly PersistentCaptureData _persistentCaptureData;
	
	private readonly CaptureHookManager _hookManager;
	private readonly ContextManager _contextManager;

	private CaptureSession _session;

	private bool _initialized;

	public CaptureSessionManager()
	{
		_persistentCaptureData = new PersistentCaptureData();
		_contextManager = new ContextManager();
		_hookManager = new CaptureHookManager();
		_hookManager.Enable();
		_hookManager.NetworkInitialized += OnNetworkInitialized;
	}

	public void Dispose()
	{
		End();
		_hookManager.Dispose();
		_contextManager.Dispose();
	}

	private void Restart()
	{
		DalamudApi.PluginLog.Debug("[CaptureSessionManager] Restart!");

		End();
		Begin();
	}

	public void Begin()
	{
		DalamudApi.PluginLog.Debug("[CaptureSessionManager] Begin!");
		var guid = Guid.NewGuid();
		_session = new CaptureSession(_persistentCaptureData, guid);
		_contextManager.Reset(guid);
		_hookManager.NetworkEvent += OnNetworkEvent;
		DalamudApi.ClientState.Logout += End;
		DalamudApi.PluginInterface.UiBuilder.AddNotification($"Capture session started: {guid}!");
		_initialized = true;
	}

	public void End()
	{
		DalamudApi.PluginLog.Debug("[CaptureSessionManager] End!");
		_contextManager.Stop();
		_hookManager.NetworkEvent -= OnNetworkEvent;
		DalamudApi.ClientState.Logout -= End;
		_session?.FinalizeSession();
		_initialized = false;
	}
	
	private void OnNetworkInitialized()
	{
		DalamudApi.PluginLog.Debug("[CaptureSessionManager] OnNetworkInitialized!");

		if (_initialized)
			Restart();
		else
			Begin();
	}
	
	private void OnNetworkEvent(PacketProto proto, Direction direction, ReadOnlySpan<byte> data)
	{
		DalamudApi.PluginLog.Debug($"[CaptureSessionManager] OnNetworkEvent! {proto} {direction}");

		if (_initialized)
			_session.WritePacket(proto, direction, data);
	}
}