﻿using System;
using Chronofoil.Capture.Context;
using Chronofoil.Capture.IO;
using Chronofoil.Packet;
using Dalamud.Interface;
using Dalamud.Plugin.Services;

namespace Chronofoil.Capture;

public class CaptureSessionManager : IDisposable
{
	private readonly PersistentCaptureData _persistentCaptureData;

	private readonly IPluginLog _log;
	private readonly Configuration _config;
	private readonly IClientState _clientState;
	private readonly UiBuilder _uiBuilder;
	
	private readonly CaptureHookManager _hookManager;
	private readonly ContextManager _contextManager;

	private CaptureSession _session;

	private bool _isCapturing;

	public CaptureSessionManager(
		IPluginLog log,
		Configuration config,
		IClientState clientState,
		UiBuilder uiBuilder,
		ContextManager contextManager,
		CaptureHookManager hookManager)
	{
		_log = log;
		_config = config;
		_clientState = clientState;
		_uiBuilder = uiBuilder;
		
		_contextManager = contextManager;
		_hookManager = hookManager;
		_hookManager.Enable();
		
		_persistentCaptureData = new PersistentCaptureData();
		_hookManager.NetworkInitialized += OnNetworkInitialized;
	}

	public void Dispose()
	{
		End();
	}

	private void Restart()
	{
		_log.Debug("[CaptureSessionManager] Restart!");

		End();
		Begin();
	}

	public void Begin()
	{
		_log.Debug("[CaptureSessionManager] Begin!");
		var guid = Guid.NewGuid();
		_session = new CaptureSession(_log, _config, _persistentCaptureData, guid);
		_contextManager.Reset(guid);
		_hookManager.NetworkEvent += OnNetworkEvent;
		_clientState.Logout += End;
		_uiBuilder.AddNotification($"Capture session started: {guid}!");
		_isCapturing = true;
	}

	public void End()
	{
		_log.Debug("[CaptureSessionManager] End!");
		_contextManager.Stop();
		_hookManager.NetworkEvent -= OnNetworkEvent;
		_clientState.Logout -= End;
		_session?.FinalizeSession();
		_isCapturing = false;
	}
	
	private void OnNetworkInitialized()
	{
		_log.Debug("[CaptureSessionManager] OnNetworkInitialized!");

		if (_isCapturing)
			Restart();
		else
			Begin();
	}
	
	private void OnNetworkEvent(PacketProto proto, Direction direction, ReadOnlySpan<byte> data)
	{
		_log.Debug($"[CaptureSessionManager] OnNetworkEvent! {proto} {direction}");

		if (_isCapturing)
			_session.WritePacket(proto, direction, data);
	}
}