using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Dalamud.Hooking;
using Chronofoil.Lobby;
using Chronofoil.Packet;
using Chronofoil.Utility;

namespace Chronofoil.Capture.IO;

public unsafe class CaptureHookManager : IDisposable
{
	private const string LobbyKeySignature = "C7 46 ?? ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 3B EB";
	private const string NetworkInitSignature = "E8 ?? ?? ?? ?? 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8D 8C 24 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 8F";
	private const string GenericRxSignature = "E8 ?? ?? ?? ?? 4C 8B 43 10 41 8B 40 18";
	private const string GenericTxSignature = "E8 ?? ?? ?? ?? 8B 53 2C 48 8D 8B";
	private const string LobbyTxSignature = "40 53 48 83 EC 20 44 8B 41 28";

	public delegate void NetworkInitializedDelegate();
	public event NetworkInitializedDelegate NetworkInitialized;

	public delegate void NetworkEventDelegate(PacketProto proto, Direction direction, ReadOnlySpan<byte> data);
	public event NetworkEventDelegate NetworkEvent;
	
	private delegate void NetworkInit(nint a1, nint a2, int a3, uint a4);
	private readonly Hook<NetworkInit> _networkInitHook;
	
	private delegate nuint RxPrototype(byte* data, byte* a2, nuint a3, nuint a4, nuint a5);
	private delegate nuint TxPrototype(byte* data, byte* a2, nuint a3, nuint a4, nuint a5, nuint a6);
	private delegate void LobbyTxPrototype(nuint data);

	private readonly Hook<RxPrototype> _chatRxHook;
	private readonly Hook<RxPrototype> _lobbyRxHook;
	private readonly Hook<RxPrototype> _zoneRxHook;
	private readonly Hook<TxPrototype> _chatTxHook;
	private readonly Hook<LobbyTxPrototype> _lobbyTxHook;
	private readonly Hook<TxPrototype> _zoneTxHook;

	private readonly LobbyEncryptionProvider _encryptionProvider;
	private readonly SimpleBuffer _buffer;

	public CaptureHookManager()
	{
		var multiScanner = new MultiSigScanner(Process.GetCurrentProcess().MainModule, true);
		_buffer = new SimpleBuffer(1024 * 1024);
		
		var lobbyKeyPtr = DalamudApi.SigScanner.ScanText(LobbyKeySignature);
		var lobbyKey = (ushort) *(int*)(lobbyKeyPtr + 3); // skip instructions and register offset
		_encryptionProvider = new LobbyEncryptionProvider(lobbyKey);
		DalamudApi.PluginLog.Debug($"[CaptureHooks] Lobby key is {lobbyKey}.");
		
		var rxPtrs = multiScanner.ScanText(GenericRxSignature, 3);
		_chatRxHook = DalamudApi.Hooks.HookFromAddress<RxPrototype>(rxPtrs[0], ChatRxDetour);
		_lobbyRxHook = DalamudApi.Hooks.HookFromAddress<RxPrototype>(rxPtrs[1], LobbyRxDetour);
		_zoneRxHook = DalamudApi.Hooks.HookFromAddress<RxPrototype>(rxPtrs[2], ZoneRxDetour);
		
		var txPtrs = multiScanner.ScanText(GenericTxSignature, 2);
		_chatTxHook = DalamudApi.Hooks.HookFromAddress<TxPrototype>(txPtrs[0], ChatTxDetour);
		_zoneTxHook = DalamudApi.Hooks.HookFromAddress<TxPrototype>(txPtrs[1], ZoneTxDetour);
		
		var lobbyTxPtr = DalamudApi.SigScanner.ScanText(LobbyTxSignature);
		_lobbyTxHook = DalamudApi.Hooks.HookFromAddress<LobbyTxPrototype>(lobbyTxPtr, LobbyTxDetour);
		
		var networkInitPtr = DalamudApi.SigScanner.ScanText(NetworkInitSignature);
		_networkInitHook = DalamudApi.Hooks.HookFromAddress<NetworkInit>(networkInitPtr, NetworkInitDetour);
		_networkInitHook.Enable();
	}

	public void Enable()
	{
		_chatRxHook?.Enable();
		_zoneRxHook?.Enable();
		_lobbyRxHook?.Enable();
		_chatTxHook?.Enable();
		_zoneTxHook?.Enable();
		_lobbyTxHook?.Enable();
	}
	
	public void Disable()
	{
		_chatRxHook?.Disable();
		_zoneRxHook?.Disable();
		_lobbyRxHook?.Disable();
		_chatTxHook?.Disable();
		_zoneTxHook?.Disable();
		_lobbyTxHook?.Disable();
	}
	
	public void Dispose()
	{
		Disable();
		_networkInitHook?.Dispose();
		_chatRxHook?.Dispose();
		_lobbyRxHook?.Dispose();
		_zoneRxHook?.Dispose();
		_chatTxHook?.Dispose();
		_lobbyTxHook?.Dispose();
		_zoneTxHook?.Dispose();
	}
	
	private void NetworkInitDetour(nint a1, nint a2, int a3, uint a4)
	{
		NetworkInitialized?.Invoke();
		DalamudApi.PluginLog.Debug("Network reinitialized.");
		_networkInitHook.Original(a1, a2, a3, a4);
	}
	
    private nuint ChatRxDetour(byte* data, byte* a2, nuint a3, nuint a4, nuint a5)
    {
	    // DalamudApi.PluginLog.Debug($"ChatRxDetour: {(long)data:X} {(long)a2:X} {a3:X} {a4:X} {a5:X}");
	    var ret = _chatRxHook.Original(data, a2, a3, a4, a5);
	    
	    var packetOffset = *(uint*)(data + 28);
	    if (packetOffset != 0) return ret;
	    
        PacketsFromFrame(PacketProto.Chat, Direction.Rx, (byte*) *(nint*)(data + 16));

        return ret;
    }
    
    private nuint LobbyRxDetour(byte* data, byte* a2, nuint a3, nuint a4, nuint a5)
    {
	    // DalamudApi.PluginLog.Debug($"LobbyRxDetour: {(long)data:X} {(long)a2:X} {a3:X} {a4:X} {a5:X}");

	    var packetOffset = *(uint*)(data + 28);
	    if (packetOffset != 0) return _lobbyRxHook.Original(data, a2, a3, a4, a5);
	    
        PacketsFromFrame(PacketProto.Lobby, Direction.Rx, (byte*) *(nint*)(data + 16));

        return _lobbyRxHook.Original(data, a2, a3, a4, a5);
    }
    
    private nuint ZoneRxDetour(byte* data, byte* a2, nuint a3, nuint a4, nuint a5)
    {
	    // DalamudApi.PluginLog.Debug($"ZoneRxDetour: {(long)data:X} {(long)a2:X} {a3:X} {a4:X} {a5:X}");
	    var ret = _zoneRxHook.Original(data, a2, a3, a4, a5);

	    var packetOffset = *(uint*)(data + 28);
	    if (packetOffset != 0) return ret;
	    
        PacketsFromFrame(PacketProto.Zone, Direction.Rx, (byte*) *(nint*)(data + 16));

        return ret;
    }
    
    private nuint ChatTxDetour(byte* data, byte* a2, nuint a3, nuint a4, nuint a5, nuint a6)
    {
	    // DalamudApi.PluginLog.Debug($"ChatTxDetour: {(long)data:X} {(long)a2:X} {a3:X} {a4:X} {a5:X} {a6:X}");
	    var ptr = (nuint*)data;
        ptr += 2;
        PacketsFromFrame(PacketProto.Chat, Direction.Tx, (byte*) *ptr);

        return _chatTxHook.Original(data, a2, a3, a4, a5, a6);
    }
    
    private void LobbyTxDetour(nuint data)
    {
	    // DalamudApi.PluginLog.Debug($"LobbyTxDetour: {data:X}");
        _lobbyTxHook.Original(data);
        
        var ptr = data + 32;
        ptr = *(nuint*)ptr;
        PacketsFromFrame(PacketProto.Lobby, Direction.Tx, (byte*) ptr);
    }
    
    private nuint ZoneTxDetour(byte* data, byte* a2, nuint a3, nuint a4, nuint a5, nuint a6)
    {
	    // DalamudApi.PluginLog.Debug($"ZoneTxDetour: {(long)data:X} {(long)a2:X} {a3:X} {a4:X} {a5:X} {a6:X}");
	    var ptr = (nuint*)data;
        ptr += 2;
        PacketsFromFrame(PacketProto.Zone, Direction.Tx, (byte*) *ptr);
        
        return _zoneTxHook.Original(data, a2, a3, a4, a5, a6);
    }

    private void PacketsFromFrame(PacketProto proto, Direction direction, byte* framePtr)
    {
        try
        {
            PacketsFromFrame2(proto, direction, framePtr);
        }
        catch (Exception e)
        {
            DalamudApi.PluginLog.Error(e, "[PacketsFromFrame] Error!!!!!!!!!!!!!!!!!!");
        }
    }
    
    private void PacketsFromFrame2(PacketProto proto, Direction direction, byte* framePtr)
    {
	    // DalamudApi.PluginLog.Debug($"PacketsFromFrame: {(long)framePtr:X} {proto} {direction}");
        if ((nuint)framePtr == 0)
        {
            DalamudApi.PluginLog.Error("null ptr");
            return;
        }
        _buffer.Clear();
        
        var headerSize = Unsafe.SizeOf<PackedPacketHeader>();
        var headerSpan = new Span<byte>(framePtr, headerSize);
        _buffer.Write(headerSpan);
        
        var header = Util.Cast<byte, PackedPacketHeader>(headerSpan);
        // DalamudApi.PluginLog.Debug($"PacketsFromFrame: writing {header.Count} packets");
        var span = new Span<byte>(framePtr, (int)header.TotalSize);
        
        // DalamudApi.PluginLog.Debug($"[{(nuint)framePtr:X}] [{proto}{direction}] proto {header.Protocol} unk {header.Unknown}, {header.Count} pkts size {header.TotalSize} usize {header.DecompressedLength}");
        
        var data = span.Slice(headerSize, (int)header.TotalSize - headerSize);
        
        // Compression
        if (header.Compression != CompressionType.None)
        {
            DalamudApi.PluginInterface.UiBuilder.AddNotification($"[{proto}{direction}] A frame was compressed.", "Chronofoil Error");
            // DalamudApi.PluginLog.Debug($"frame compressed: {header.Compression} payload is {header.TotalSize - 40} bytes, decomp'd is {header.DecompressedLength}");
            return;
        }

        var offset = 0;
        for (int i = 0; i < header.Count; i++)
        {
	        var pktHdrSize = Unsafe.SizeOf<PacketElementHeader>();
            var pktHdrSlice = data.Slice(offset, pktHdrSize);
            _buffer.Write(pktHdrSlice);
            var pktHdr = Util.Cast<byte, PacketElementHeader>(pktHdrSlice);

            // DalamudApi.PluginLog.Debug($"packet: type {pktHdr.Type}, {pktHdr.Size} bytes, {pktHdr.SrcEntity} -> {pktHdr.DstEntity}");
            
            var pktData = data.Slice(offset + pktHdrSize, (int)pktHdr.Size - pktHdrSize);

            var canInitEncryption = proto == PacketProto.Lobby && pktHdr.Type is PacketType.EncryptionInit;
            var needsDecryption = proto == PacketProto.Lobby && pktHdr.Type is PacketType.Ipc or PacketType.Unknown_A;
            
            if (canInitEncryption)
                _encryptionProvider.Initialize(pktData);
            
            if (_encryptionProvider.Initialized && needsDecryption)
            {
                var decoded = _encryptionProvider.DecryptPacket(pktData);
                pktData = new Span<byte>(decoded);
            }
            
            _buffer.Write(pktData);
            
            // DalamudApi.PluginLog.Debug($"packet: type {pktHdr.Type}, {pktHdr.Size} bytes, {pktHdr.SrcEntity} -> {pktHdr.DstEntity}");
            offset += (int)pktHdr.Size;
        }
        
        // DalamudApi.PluginLog.Debug($"[{proto}{direction}] invoking network event header size {header.TotalSize} usize {header.DecompressedLength} buffer size {_buffer.GetBuffer().Length}");
        NetworkEvent?.Invoke(proto, direction, _buffer.GetBuffer());
    }
}