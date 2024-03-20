namespace Chronofoil.Packet;

// Segment type 3, IPC
public struct PacketIpcHeader
{
	public ushort Unknown;
	public ushort Type; // Opcode
	public ushort Padding01;
	public ushort ServerId;
	public ushort Timestamp;
	public ushort Padding02;
};