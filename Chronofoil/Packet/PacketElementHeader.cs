namespace Chronofoil.Packet;

public struct PacketElementHeader
{
	public uint Size;
	public uint SrcEntity;
	public uint DstEntity;
	public PacketType Type;
	public ushort Padding01;
}