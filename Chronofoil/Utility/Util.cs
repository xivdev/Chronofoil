using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Chronofoil.Utility;

public static class Util
{
	// public static unsafe bool IsOodleInitialized()
	// {
	// 	const string oodleAllocFuncsInitSig = "80 3D ?? ?? ?? ?? ?? 75 1A 48 8D 15";
	// 	var oodleAllocFuncsInitPtr = (byte*) DalamudApi.SigScanner.GetStaticAddressFromSig(oodleAllocFuncsInitSig);
	// 	return oodleAllocFuncsInitPtr == (void*)0 || *oodleAllocFuncsInitPtr == 1;
	// }

	public static unsafe T Read<T>(nint ptr) where T : struct
	{
		var size = Unsafe.SizeOf<T>();
		var span = new Span<byte>((void*)ptr, size);
		return MemoryMarshal.Cast<byte, T>(span)[0];
	}
	
	public static U Cast<T, U>(Span<T> input) where T : struct where U : struct
	{
		return MemoryMarshal.Cast<T, U>(input)[0];
	}

	public static void LogBytes(ReadOnlySpan<byte> data, int offset, int length)
	{
		DalamudApi.PluginLog.Debug($"{ByteString(data, offset, length)}");
	}

	public static string ByteString(ReadOnlySpan<byte> data, int offset, int length)
	{
		var sb = new StringBuilder();
		for (int i = offset; i < length; i++)
		{
			sb.Append($"{data[i]:X2}");
		}
		return sb.ToString();
	}
	
	public static unsafe string ByteString(byte* data, int offset, int length)
	{
		var sb = new StringBuilder();
		for (int i = offset; i < length; i++)
		{
			sb.Append($"{data[i]:X2}");
		}
		return sb.ToString();
	}
	
	public static string GetHumanByteString(ulong bytes)
	{
		return bytes switch
		{
			>= (1024 * 1024 * 1024) => $"{bytes / 1024 / 1024 / 1024}gb",
			>= (1024 * 1024) => $"{bytes / 1024 / 1024}mb",
			>= (1024) => $"{bytes / 1024}kb",
			_ => $"{bytes} bytes",
		};
	}
}