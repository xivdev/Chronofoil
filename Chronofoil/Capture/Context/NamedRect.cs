using SixLabors.ImageSharp;

namespace Chronofoil.Capture.Context;

public readonly struct NamedRect
{
	public readonly string Name;
	private readonly int _width;
	private readonly int _height;
	private readonly int _x;
	private readonly int _y;

	public NamedRect(string name, int x, int y, int width, int height)
	{
		Name = name;
		_x = x;
		_y = y;
		_width = width;
		_height = height;
	}

	public RectangleF ToRectangleF()
	{
		return new RectangleF
		{
			X = _x,
			Y = _y,
			Width = _width,
			Height = _height,
		};
	}
}