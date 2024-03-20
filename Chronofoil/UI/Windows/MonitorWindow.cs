using System.Collections.Generic;
using Chronofoil.Monitor;
using Chronofoil.Monitor.Model;
using Chronofoil.UI.Components;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Chronofoil.UI.Windows;

public class MonitorWindow : Window
{
	private readonly MonitorSessionManager _manager;
	private MonitorTab _currentTab;
	private readonly Dictionary<string, MonitorTab> _tabs = new();
	
	public MonitorWindow(
		MonitorSessionManager monitorSessionManager,
		string name = "Chronofoil Monitor Window",
		ImGuiWindowFlags flags = ImGuiWindowFlags.MenuBar,
		bool forceMainWindow = false) : base(name, flags, forceMainWindow)
	{
		_manager = monitorSessionManager;
		NewTab();
	}

	public override void Draw()
	{
		DrawMenuBar();

		DrawPane1();
		DrawPane2();
	}

	private void DrawPane1()
	{
		using var _ = ImRaii.Child("Pane1", new System.Numerics.Vector2(0, 0), true);
		
		using (var _1 = ImRaii.Child("Pane1SubPane1", new System.Numerics.Vector2(0, 0), true))
		{
			DrawMonitorTabs();	
		}

		using (var _2 = ImRaii.Child("Pane1SubPane2", new System.Numerics.Vector2(0, 0), true))
		{
			ImGui.Text("Pane 1 Sub Pane 2");
			ImGui.Text("Pane 1 Sub Pane 2");
			ImGui.Text("Pane 1 Sub Pane 2");
			ImGui.Text("Pane 1 Sub Pane 2");
			ImGui.Text("Pane 1 Sub Pane 2");
			ImGui.Text("Pane 1 Sub Pane 2");
			ImGui.Text("Pane 1 Sub Pane 2");
		}
	}

	private void DrawPane2()
	{
		using var _ = ImRaii.Child("Pane2", new System.Numerics.Vector2(0, 0), true);
		using var _1 = ImRaii.Child("Pane2SubPane1", new System.Numerics.Vector2(0, 0), true);
		ImGui.Text("Pane 2 Sub Pane 1");
		ImGui.Text("Pane 2 Sub Pane 1");
		ImGui.Text("Pane 2 Sub Pane 1");
		ImGui.Text("Pane 2 Sub Pane 1");
		ImGui.Text("Pane 2 Sub Pane 1");
		ImGui.Text("Pane 2 Sub Pane 1");
	}

	private void DrawMonitorTabs()
	{
		using var _ = ImRaii.TabBar("MonitorTabBar", ImGuiTabBarFlags.None);
		foreach (var tab in _tabs)
		{
			if (ImGui.BeginTabItem(tab.Key))
			{
				tab.Value.Draw();
				ImGui.EndTabItem();
			}
		}
	}

	private void DrawMenuBar()
	{
		if (ImGui.BeginMenuBar())
		{
			if (ImGui.BeginMenu("File"))
			{
				if (ImGui.MenuItem("Open"))
					pass();
				if (ImGui.MenuItem("Save"))
					pass();
				ImGui.Separator();
				if (ImGui.MenuItem("Import FFXIV Replay"))
					pass();
				if (ImGui.MenuItem("Import ACT Log File"))
					pass();
				ImGui.Separator();
				if (ImGui.MenuItem("Clear Tab"))
					pass();
				if (ImGui.MenuItem("New Tab"))
					NewTab();
				ImGui.EndMenu();
			}
			if (ImGui.BeginMenu("Capture"))
			{
				if (ImGui.MenuItem("Start"))
					_currentTab.Session.Start();
				if (ImGui.MenuItem("Stop"))
					_currentTab.Session.Stop();
				ImGui.EndMenu();
			}
			if (ImGui.BeginMenu("Database"))
			{
				if (ImGui.MenuItem("Reload DB"))
					pass();
				if (ImGui.MenuItem("Redownload Definitions"))
					pass();
				if (ImGui.MenuItem("Select Version"))
					pass();
				if (ImGui.MenuItem("Select Branch/Commit"))
					pass();
				if (ImGui.MenuItem("Enable File Watcher for Definitions"))
					pass();
				if (ImGui.MenuItem("Open Definitions Folder"))
					pass();
				
				ImGui.EndMenu();
			}
			if (ImGui.BeginMenu("Filters"))
			{
				if (ImGui.MenuItem("Set Filter"))
					pass();
				if (ImGui.MenuItem("Reset"))
					pass();
				if (ImGui.MenuItem("Show Help"))
					pass();
				ImGui.EndMenu();
			}
			if (ImGui.BeginMenu("Scripting"))
			{
				if (ImGui.MenuItem("Run on capture"))
					pass();
				if (ImGui.MenuItem("Run on new packets"))
					pass();
				if (ImGui.MenuItem("Run multiple captures"))
					pass();
				if (ImGui.MenuItem("Open Output Window"))
					pass();
				ImGui.Separator();
				if (ImGui.MenuItem("Select Scripts"))
					pass();
				if (ImGui.MenuItem("Reload Scripts"))
					pass();
				if (ImGui.MenuItem("Reset data storage"))
					pass();
				if (ImGui.MenuItem("Show map of parsed structs"))
					pass();
				ImGui.EndMenu();
			}
			if (ImGui.BeginMenu("Diff"))
			{
				if (ImGui.MenuItem("Diff based on packet length"))
					pass();
				if (ImGui.MenuItem("Diff based on packet data"))
					pass();
				ImGui.EndMenu();
			}
			if (ImGui.BeginMenu("EXD"))
			{
				if (ImGui.MenuItem("Enable"))
					pass();
				if (ImGui.MenuItem("Reload"))
					pass();
				ImGui.EndMenu();
			}
			if (ImGui.BeginMenu("Options"))
			{
				if (ImGui.MenuItem("Hide known Actor ID fields in hex editor"))
					pass();
				if (ImGui.MenuItem("Stick packet view to bottom"))
					pass();
				if (ImGui.MenuItem("About"))
					pass();
				ImGui.EndMenu();
			}
			ImGui.EndMenuBar();
		}
	}

	private void NewTab()
	{
		_currentTab = new MonitorTab(_manager.NewSession());
		_tabs.Add(_currentTab.Name, _currentTab);
	}

	private void pass()
	{
		
	}
}