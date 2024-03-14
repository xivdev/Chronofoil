using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Chronofoil.Capture.Context;

public class ContextContainer
{
	public Guid CaptureGuid { get; set; }
	public ulong CaptureTime { get; set; }
	public Image<Rgba32> Image { get; private set; }
	public List<NamedRect> CensorRects { get; }

	public ContextContainer()
	{
		Image = null;
		CensorRects = new List<NamedRect>();
	}
	
	public void LoadImageData(Span<byte> srcSpan, int width, int height, int rowPitch)
	{
		// DalamudApi.PluginLog.Debug($"[LoadImageData] width {width} height {height} rowPitch {rowPitch}");

		var config = SixLabors.ImageSharp.Configuration.Default.Clone();
		
		config.PreferContiguousImageBuffers = true;
		Image = new Image<Rgba32>(config, width, height);
		Image.DangerousTryGetSinglePixelMemory(out var mem);
		var tgtSpan = MemoryMarshal.Cast<Rgba32, byte>(mem.Span);

		for (int y = 0; y < height; y++)
		{
			var padding = y * (rowPitch - width * 4);
			var srcIdx = (y * width * 4) + padding;
			var tgtIdx = (y * width * 4);
			srcSpan.Slice(srcIdx, width * 4).CopyTo(tgtSpan.Slice(tgtIdx, width * 4));
		}
	}
}