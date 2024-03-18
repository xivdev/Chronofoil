using System;
using System.IO;
using Chronofoil.Capture.IO;
using Chronofoil.Packet;
using Dalamud.Plugin.Services;

namespace Chronofoil.Capture;

public class CaptureSession
{
	private readonly IPluginLog _log;
	
	private readonly Guid _captureGuid;
	private readonly ulong _captureTime;
	private readonly FileInfo _captureFile;
	private readonly BufferedStream _captureStream;

	private bool _disposed;
	
	public CaptureSession(IPluginLog log, Configuration config, PersistentCaptureData data, Guid guid)
	{
		_log = log;
		
		_captureGuid = guid;
		_captureTime = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		var captureDirectory = new DirectoryInfo(config.StorageDirectory);
		captureDirectory = captureDirectory.CreateSubdirectory(_captureGuid.ToString());

		_captureFile = new FileInfo(Path.Combine(captureDirectory.FullName, "capture.dat"));
		_log.Debug($"[CaptureSession] Capture file path is {_captureFile.FullName}");
		_captureStream = new BufferedStream(_captureFile.OpenWrite(), 10240);
		data.WriteTo(_captureStream);
		WriteTo(_captureStream);
	}

	private void WriteTo(Stream stream)
	{
		stream.Write(BitConverter.GetBytes(28)); // 4
		stream.Write(_captureGuid.ToByteArray()); // 16
		stream.Write(BitConverter.GetBytes(_captureTime)); // 8
	}

	public void WritePacket(PacketProto proto, Direction direction, ReadOnlySpan<byte> data)
	{
		_captureStream.WriteByte((byte) proto);
		_captureStream.WriteByte((byte) direction);
		_captureStream.Write(data);
	}

	public void FinalizeSession()
	{
		if (_disposed) return;
		_captureStream.Flush();
		_captureStream.Dispose();
		_disposed = true;
	}
}