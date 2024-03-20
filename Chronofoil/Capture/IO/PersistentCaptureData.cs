using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Chronofoil.Capture.IO;

public class PersistentCaptureData
{
	private const int CaptureVersion = 0;
	private const ushort HeaderSize = 240;
	
	public ulong Dx9GameRev { get; init; }
	public ulong Dx11GameRev { get; init; }
	
	public byte[] Dx9Hash { get; init; }
	public byte[] Dx11Hash { get; init; }
	
	public string FfxivGameVer { get; init; }
	public string Ex1GameVer { get; init; }
	public string Ex2GameVer { get; init; }
	public string Ex3GameVer { get; init; }
	public string Ex4GameVer { get; init; }
	
	public string PluginVersion { get; init; }

	public PersistentCaptureData()
	{
		var path = Process.GetCurrentProcess().MainModule.FileName;
		var dx11Path = path.Replace("ffxiv.exe", "ffxiv_dx11.exe");
		var dx9Path = path.Replace("ffxiv_dx11.exe", "ffxiv.exe");

		var dx9Data = File.Exists(dx9Path) ? File.ReadAllBytes(dx9Path) : Array.Empty<byte>();
		var dx11Data = File.ReadAllBytes(dx11Path);

		var parent = Directory.GetParent(path).FullName;
		var sqpack = Path.Combine(parent, "sqpack");
		
		var ffxivVerFile = Path.Combine(parent, "ffxivgame.ver");
		var ex1VerFile = Path.Combine(sqpack, "ex1", "ex1.ver");
		var ex2VerFile = Path.Combine(sqpack, "ex2", "ex2.ver");
		var ex3VerFile = Path.Combine(sqpack, "ex3", "ex3.ver");
		var ex4VerFile = Path.Combine(sqpack, "ex4", "ex4.ver");
		
		Dx9GameRev = File.Exists(dx9Path) ? GetBuild(dx9Data) : ulong.MaxValue;
		Dx11GameRev = GetBuild(dx11Data);
		Dx9Hash = GetHash(dx9Data);
		Dx11Hash = GetHash(dx11Data);
		FfxivGameVer = GetVer(ffxivVerFile);
		Ex1GameVer = GetVer(ex1VerFile);
		Ex2GameVer = GetVer(ex2VerFile);
		Ex3GameVer = GetVer(ex3VerFile);
		Ex4GameVer = GetVer(ex4VerFile);
		PluginVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
	}
	
	private static ulong GetBuild(byte[] data)
	{
		var bytes = "/*****ff14******rev"u8.ToArray();
		var stringBytes = new List<byte>();
		for (int i = 0; i < data.Length - bytes.Length; i++) {
			if (data.AsSpan().Slice(i, bytes.Length).SequenceEqual(bytes))
			{
				i += bytes.Length;
				for (int j = 0; data[i + j] != '_'; j++) {
					stringBytes.Add(data[i + j]);
				}
				break;
			}
		}
		return ulong.Parse(Encoding.ASCII.GetString(stringBytes.ToArray()));
	}

	private static byte[] GetHash(byte[] data)
	{
		return SHA1.HashData(data);
	}

	private static string GetVer(string path)
	{
		return File.Exists(path) ? File.ReadAllText(path) : "0000.00.00.0000.0000";
	}
	
	public void WriteTo(Stream stream)
	{
		stream.Write(BitConverter.GetBytes(256)); // 4
		stream.Write(BitConverter.GetBytes(CaptureVersion)); // 4
		stream.Write(BitConverter.GetBytes(Dx9GameRev)); // 8
		stream.Write(BitConverter.GetBytes(Dx11GameRev)); // 8
		stream.Write(Dx9Hash); // 20
		stream.Write(Dx11Hash); // 20

		stream.WritePadded(FfxivGameVer, 32); // 32
		stream.WritePadded(Ex1GameVer, 32); // 32
		stream.WritePadded(Ex2GameVer, 32); // 32
		stream.WritePadded(Ex3GameVer, 32); // 32
		stream.WritePadded(Ex4GameVer, 32); // 32

		stream.WritePadded(PluginVersion, 16); // 16

		var pad = 256 - HeaderSize;
		for (int i = 0; i < pad; i++)
			stream.WriteByte(0);
	}
}