namespace Chronofoil.Packet;

public struct PackedPacketHeader
{
	public unsafe fixed byte Prefix[16];
	public ulong TimeValue;
	public uint TotalSize;
	public PacketProto Protocol;
	public ushort Count;
	public byte Version;
	public CompressionType Compression;
	public ushort Unknown;
	public uint DecompressedLength;
}