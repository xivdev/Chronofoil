using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Hooking;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Component.GUI;
using SharpDX.Direct3D11;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Device = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using SwapChain = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.SwapChain;

namespace Chronofoil.Capture.Context;

/**
 * Note: Context will either be entirely removed or disabled before crowdsourcing is implemented.
 * Context images are too large to be sent to a server for crowdsourcing, no matter what the size
 * or screenshot frequency is. Also, the images are completely useless for automated pipelines.
 * They are useful for local context, though, so they may remain in the codebase.
 */
public unsafe class ContextManager : IDisposable
{
	private const string PresentSig = "E8 ?? ?? ?? ?? C6 47 79 00";
	private const int Interval = 5000;

	private delegate void PresentPrototype(nint address);
	private readonly Hook<PresentPrototype> _presentHook;
	
	private ulong _lastCtx;
	private readonly ContextContainer _contextContainer;
	private string _contextDir;
	private readonly CancellationTokenSource _tokenSource;

	private readonly Font _font = SystemFonts.CreateFont("Consolas", 12f, FontStyle.Regular);

	public ContextManager()
	{
		_contextContainer = new ContextContainer();
		_tokenSource = new CancellationTokenSource();
		
		var presentPtr = DalamudApi.SigScanner.ScanText(PresentSig);
		_presentHook = DalamudApi.Hooks.HookFromAddress<PresentPrototype>(presentPtr, PresentDetour);
		
		_lastCtx = 0;
	}

	public void Reset(Guid captureGuid)
	{
		lock (_contextContainer)
			_contextContainer.CaptureGuid = captureGuid;
		_contextDir = Path.Combine(Chronofoil.Configuration.StorageDirectory, captureGuid.ToString(), "ctx");
		Directory.CreateDirectory(_contextDir);
		_presentHook?.Enable();
	}

	public void Stop()
	{
		_presentHook?.Disable();
		_contextDir = null;
		lock (_contextContainer)
			_contextContainer.CaptureGuid = Guid.Empty;
	}

	public void Dispose()
	{
		_presentHook?.Dispose();
		_tokenSource?.Cancel();
	}
	
	private void PresentDetour(nint ptr)
	{
		var ms = (ulong)Environment.TickCount64;
		if (ms - _lastCtx <= Interval)
		{
			_presentHook.Original(ptr);
			return;
		}
		
		var gameDevice = Device.Instance();
		if (gameDevice == null) return;
		var gameSwapChain = gameDevice->SwapChain;
		if (gameSwapChain == null) return;
		
		using var device = new SharpDX.Direct3D11.Device((nint)gameDevice->D3D11Forwarder);
		using var deviceContext = device.ImmediateContext;
		using var swapChain = new SharpDX.DXGI.SwapChain((nint)gameSwapChain->DXGISwapChain);

		_lastCtx = ms;
		var captureMs = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds();

		using var backbuffer = swapChain.GetBackBuffer<Texture2D>(0);
		var width = backbuffer.Description.Width;
		var height = backbuffer.Description.Height;
		using var stagingTexture = GraphicsHelper.CreateStagingTexture(device, width, height);

		using var rt = new RenderTargetView(device, backbuffer);
		using var res = rt.Resource;
		deviceContext.CopyResource(res, stagingTexture);
		var dataBox = deviceContext.MapSubresource(stagingTexture, 0, MapMode.Read, MapFlags.None);

		var rowPitch = dataBox.RowPitch;
		var slicePitch = dataBox.SlicePitch;
		
		try
		{
			lock (_contextContainer)
			{
				var imageData = new Span<byte>((void*)dataBox.DataPointer, slicePitch);
				_contextContainer.LoadImageData(imageData, width, height, rowPitch);
				_contextContainer.CaptureTime = captureMs;
				_contextContainer.CensorRects.Clear();
				
				foreach (var censorable in CensorRegistry.Censorables)
				{
					var addon = (AtkUnitBase*)DalamudApi.GameGui.GetAddonByName(censorable.AddonString);
					if (addon != null)
					{
						var offsetX = censorable.Type == Censorable.CensorType.Full ? 0 : (int) censorable.Offset.X;
						var offsetY = censorable.Type == Censorable.CensorType.Full ? 0 : (int) censorable.Offset.Y;
						var dimX = censorable.Type == Censorable.CensorType.Full ? (int)addon->GetScaledWidth(true) : (int) censorable.OffsetDimensions.X;
						var dimY = censorable.Type == Censorable.CensorType.Full ? (int)addon->GetScaledHeight(true) : (int) censorable.OffsetDimensions.Y;

						offsetX = (int) Math.Floor(offsetX * addon->Scale);
						offsetY = (int) Math.Floor(offsetY * addon->Scale);
						dimX = (int) Math.Floor(dimX * addon->Scale);
						dimY = (int) Math.Floor(dimY * addon->Scale);

						var rect = new NamedRect(
							censorable.AddonString,
							addon->X + offsetX,
							addon->Y + offsetY, 
							dimX,
							dimY);
						_contextContainer.CensorRects.Add(rect);
					}
				}
			}
			
			Task.Run(RenderContext, _tokenSource.Token);
		}
		catch (Exception e)
		{
			DalamudApi.PluginLog.Error(e, "oh no");
		}
		finally
		{
			deviceContext?.UnmapSubresource(stagingTexture, 0);
			_presentHook?.Original(ptr);
		}
	}

	private void RenderContext()
	{
		try
		{
			lock (_contextContainer)
			{
				var captureTime = _contextContainer.CaptureTime;

				_contextContainer.Image.ProcessPixelRows(accessor =>
				{
					for (int y = 0; y < accessor.Height; y++)
					{
						var pixelRow = accessor.GetRowSpan(y);
						for (int x = 0; x < pixelRow.Length; x++)
							pixelRow[x].A = 255;
					}
				});
				
				// _contextContainer.Image.Mutate(ctx => {
				// 	foreach (var element in _contextContainer.CensorRects)
				// 	{
				// 		ctx.Fill(Color.Black, element.ToRectangleF());
				// 		ctx.DrawText(element.Name, _font, Color.White, new PointF(element.ToRectangleF().X, element.ToRectangleF().Y));
				// 	}
				// });
				
				_contextContainer.Image.SaveAsJpeg(Path.Combine(_contextDir, $"ctx-{captureTime}.jpeg"));
			}
		}
		catch (Exception e)
		{
			DalamudApi.PluginLog.Error(e, "oh no 2");
		}
	}
}

