using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Chronofoil.Packet;
using Chronofoil.Utility;

namespace Chronofoil.Monitor.Model;

// public record MonitorPacket(PacketProto Protocol, Direction Direction, byte[] Data);
public record MonitorPacket
{
	// public Guid Id { get; }
	
	public required PacketProto Protocol;
	public required Direction Direction;
	public required PackedPacketHeader FrameHeader;
	public required int FrameIndex;
	public required byte[] Data;
	
	public required ulong Timestamp;

	private PacketElementHeader? _packetHeader;
	private PacketIpcHeader? _ipcHeader;
	
	[SetsRequiredMembers]
	public MonitorPacket(PacketProto protocol, Direction direction, PackedPacketHeader frameHeader, int frameIndex, byte[] data)
	{
		// Id = Guid.NewGuid();
		Protocol = protocol;
		Direction = direction;
		FrameHeader = frameHeader;
		FrameIndex = frameIndex;
		Data = data;
		Timestamp = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds();
	}
	
	public PacketElementHeader PacketHeader
	{
		get
		{
			var pktHdrSize = Unsafe.SizeOf<PacketElementHeader>();
			var pktHdrSlice = Data[..pktHdrSize];
			return _packetHeader ??= Util.Cast<byte, PacketElementHeader>(pktHdrSlice);
		}
	}

	public PacketIpcHeader? IpcHeader {
		get
		{
			if (PacketHeader.Type != PacketType.Ipc) return null;
			var pktHdrSize = Unsafe.SizeOf<PacketElementHeader>();
			var ipcHdrSize = Unsafe.SizeOf<PacketIpcHeader>();
			var ipcHdrSlice = Data.AsSpan().Slice(pktHdrSize, ipcHdrSize);
			return _ipcHeader ??= Util.Cast<byte, PacketIpcHeader>(ipcHdrSlice);
		}
	}

	public override string ToString() => $"[{Protocol}{Direction}] {PacketHeader.Type} {PacketHeader.Size} bytes";
}