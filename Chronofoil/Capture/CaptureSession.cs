using System;
using System.IO;
using Dalamud.Logging;
using Chronofoil.Capture.IO;
using Chronofoil.Packet;

namespace Chronofoil.Capture;

public class CaptureSession
{
	private Guid _captureGuid;
	private ulong _captureTime;
	private FileInfo _captureFile;
	private BufferedStream _captureStream;

	public CaptureSession(PersistentCaptureData data, Guid guid)
	{
		_captureGuid = guid;
		_captureTime = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		var captureDirectory = new DirectoryInfo(Chronofoil.Configuration.StorageDirectory);
		captureDirectory = captureDirectory.CreateSubdirectory(_captureGuid.ToString());

		_captureFile = new FileInfo(Path.Combine(captureDirectory.FullName, "capture.dat"));
		DalamudApi.PluginLog.Debug($"[CaptureSession] Capture file path is {_captureFile.FullName}");
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
		_captureStream.Flush();
		_captureStream.Dispose();
	}
}