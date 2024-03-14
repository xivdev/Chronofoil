using SharpDX.Direct3D11;

namespace Chronofoil.Capture.Context;

public static class GraphicsHelper
{
	public static Texture2D CreateStagingTexture(SharpDX.DXGI.Device device, int width, int height)
	{
		var nDevice = new Device(device.NativePointer);
		return CreateStagingTexture(nDevice, width, height);
	}
	
	public static Texture2D CreateStagingTexture(Device device, int width, int height)
	{
		// For handling of staging resource see
		// http://msdn.microsoft.com/en-US/Library/Windows/Desktop/FF476259(v=vs.85).aspx
		var textureDescription = new Texture2DDescription
		{
			Width = width,
			Height = height,
			MipLevels = 1,
			ArraySize = 1,
			Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
			Usage = ResourceUsage.Staging,
			SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
			BindFlags = BindFlags.None,
			CpuAccessFlags = CpuAccessFlags.Read,
			OptionFlags = ResourceOptionFlags.None,
		};
		return new Texture2D(device, textureDescription);
	}
}