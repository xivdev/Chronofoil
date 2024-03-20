using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Chronofoil.Monitor.Model;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace Chronofoil.UI.Components;

public class MonitorTab
{
	public MonitorSession Session { get; }
	
	// Convenience accessors
	public string Name => Session.Name;
	public bool IsActive => Session.IsActive;
	public List<MonitorPacket> Packets => Session.Packets;
	
	// ImGui state
	private List<MonitorPacket> _selectedPackets = new();
	private ImGuiListClipperPtr _clipperPtr;
	
	public MonitorTab(MonitorSession session)
	{
		Session = session;

		var native = Marshal.AllocHGlobal(Unsafe.SizeOf<ImGuiListClipper>());
		_clipperPtr = new ImGuiListClipperPtr(native);
	}

	public void Dispose()
	{
		unsafe
		{
			Marshal.FreeHGlobal((IntPtr)_clipperPtr.NativePtr);	
		}
	}

	public void Draw()
	{
		var tableFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY;// | ImGuiTableFlags.SizingFixedFit;
		
		// using var _ = ImRaii.Table("MonitorTable", 11, tableFlags);
		if (ImGui.BeginTable("MonitorTable", 10, tableFlags))
		{
			ImGui.TableSetupColumn("Direction", ImGuiTableColumnFlags.WidthFixed);
			ImGui.TableSetupColumn("Connection", ImGuiTableColumnFlags.WidthFixed);
			ImGui.TableSetupColumn("Segment Type", ImGuiTableColumnFlags.WidthFixed);
			ImGui.TableSetupColumn("Ipc Type", ImGuiTableColumnFlags.WidthFixed);
			// ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
			ImGui.TableSetupColumn("Server ID", ImGuiTableColumnFlags.WidthFixed);
			ImGui.TableSetupColumn("Comment", ImGuiTableColumnFlags.WidthFixed);
			ImGui.TableSetupColumn("Note", ImGuiTableColumnFlags.WidthFixed);
			ImGui.TableSetupColumn("Size", ImGuiTableColumnFlags.WidthFixed);
			ImGui.TableSetupColumn("Set", ImGuiTableColumnFlags.WidthFixed);
			ImGui.TableSetupColumn("Timestamp", ImGuiTableColumnFlags.WidthFixed);
			ImGui.TableSetupScrollFreeze(0, 1);
			
			ImGui.TableHeadersRow();
			
			_clipperPtr.Begin(Packets.Count);
			while (_clipperPtr.Step())
			{
				for (int i = _clipperPtr.DisplayStart; i < _clipperPtr.DisplayEnd; i++)
				{
					var packet = Packets[i];
					ImGui.TableNextRow();
					ImGui.TableNextColumn();
					ImGui.TextUnformatted(packet.Direction.ToString());
					ImGui.TableNextColumn();
					ImGui.TextUnformatted(packet.Protocol.ToString());
					ImGui.TableNextColumn();
					ImGui.TextUnformatted(packet.PacketHeader.Type.ToString());
					ImGui.TableNextColumn();
					ImGui.TextUnformatted(packet.IpcHeader?.Type.ToString("X4"));
					ImGui.TableNextColumn();
					ImGui.TextUnformatted(packet.IpcHeader?.ServerId.ToString());
					ImGui.TableNextColumn();
					ImGui.TextUnformatted("comment");
					ImGui.TableNextColumn();
					ImGui.TextUnformatted("note");
					ImGui.TableNextColumn();
					ImGui.TextUnformatted(packet.Data.Length.ToString());
					ImGui.TableNextColumn();
					// ImGui.TextUnformatted(packet.Set.ToString());
					ImGui.TextUnformatted("set");
					ImGui.TableNextColumn();
					ImGui.TextUnformatted(packet.Timestamp.ToString());
				}
			}
			
			ImGui.EndTable();
		}
	}
}