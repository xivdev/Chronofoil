using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Chronofoil.Capture;
using Chronofoil.Capture.IO;
using Chronofoil.Monitor.Model;
using Chronofoil.Packet;
using Chronofoil.Utility;
using Dalamud.Plugin.Services;

namespace Chronofoil.Monitor;

public class MonitorSessionManager
{
	private readonly IPluginLog _log;
	private readonly CaptureHookManager _captureHookManager;
	
	private readonly Dictionary<string, MonitorSession> _sessions = new();
	private readonly List<MonitorPacket> _currentFrame = new();
	
	private int _sessionCounter = 0;
	
	public MonitorSessionManager(
		IPluginLog log,
		CaptureHookManager captureHookManager)
	{
		_log = log;
		_captureHookManager = captureHookManager;
		_captureHookManager.NetworkEvent += OnNetworkEvent;
	}
	
	private void OnNetworkEvent(PacketProto proto, Direction direction, ReadOnlySpan<byte> data)
	{
		// _log.Debug($"[MonitorSessionManager] packet: {proto} {direction}, {data.Length} bytes");
		PacketsFromFrame(proto, direction, data);

		foreach (var session in _sessions.Values.Where(session => session.IsActive))
			session.AddPacketRange(_currentFrame);
		_currentFrame.Clear();
	}
	
	private void PacketsFromFrame(PacketProto proto, Direction direction, ReadOnlySpan<byte> frame)
    {
	    var headerSize = Unsafe.SizeOf<PackedPacketHeader>();
	    var headerSpan = frame[..headerSize];
        var header = Util.Cast<byte, PackedPacketHeader>(headerSpan);
        var data = frame.Slice(headerSize, (int)header.TotalSize - headerSize);
        
        var offset = 0;
        for (int i = 0; i < header.Count; i++)
        {
	        var pktHdrSize = Unsafe.SizeOf<PacketElementHeader>();
            var pktHdrSlice = data.Slice(offset, pktHdrSize);
            var pktHdr = Util.Cast<byte, PacketElementHeader>(pktHdrSlice);

            // _log.Debug($"[MonitorSessionManager] packet: type {pktHdr.Type}, {pktHdr.Size} bytes, {proto} {direction}, {pktHdr.SrcEntity} -> {pktHdr.DstEntity}");
            
            var packet = data.Slice(offset, (int)pktHdr.Size);
            
            _currentFrame.Add(new MonitorPacket(proto, direction, header, i, packet.ToArray()));
            offset += (int)pktHdr.Size;
        }
    }

	public MonitorSession NewSession()
	{
		var sessionKey = "Session " + _sessionCounter++;
		var session = new MonitorSession(sessionKey);
		_sessions.Add(sessionKey, session);
		return session;
	}

	public MonitorSession? GetSession(string sessionKey)
	{
		return _sessions.GetValueOrDefault(sessionKey, null);
	}
	
	public IEnumerable<MonitorSession> GetSessions()
	{
		return _sessions.Values;
	}
}