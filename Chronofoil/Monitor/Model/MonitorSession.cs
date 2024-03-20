using System.Collections.Generic;

namespace Chronofoil.Monitor.Model;

public class MonitorSession
{
	public string Key { get; private set; } = "New Session";
	public string Name { get; set; } = "New Session";
	public List<MonitorPacket> Packets { get; } = new();
	public bool IsActive { get; private set; } = false;

	public MonitorSession(string name)
	{
		Key = name;
		Name = name;
	}
	
	public void AddPacket(MonitorPacket packet) => Packets.Add(packet);
	public void AddPacketRange(List<MonitorPacket> packets) => Packets.AddRange(packets);
	public void ClearPackets() => Packets.Clear();
	// public void 
	
	public void Start() => IsActive = true;
	public void Stop() => IsActive = false;
}