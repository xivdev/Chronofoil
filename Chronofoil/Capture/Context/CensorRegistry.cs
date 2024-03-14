using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;

namespace Chronofoil.Capture.Context;

/**
 * This class is used to register all the UI elements that need to be censored.
 * Censor is not currently in use. Context images will be saved with UI elements visible.
 */
public static class CensorRegistry
{
	public static readonly ReadOnlyCollection<Censorable> Censorables;

	static CensorRegistry()
	{
		var censorables = new List<Censorable>
		{
			new("ChatLogPanel_1", Censorable.CensorType.Full),
			new("ChatLogPanel_2", Censorable.CensorType.Full),
			new("ChatLogPanel_3", Censorable.CensorType.Full),
			new("ChatLogPanel_4", Censorable.CensorType.Full),
			new("LetterList", Censorable.CensorType.Full),
			new("LetterViewer", Censorable.CensorType.Full),
			new("_CharaSelectListMenu", Censorable.CensorType.Partial, new Vector2(0, 120), new Vector2(500, 50)),
			new("Character", Censorable.CensorType.Partial, new Vector2(500, 5), new Vector2(230, 30)),
		};
		Censorables = new ReadOnlyCollection<Censorable>(censorables);
	}
}

public class Censorable
{
	public enum CensorType
	{
		Full,
		Partial,
	}
	
	public string AddonString { get; init; }
	public CensorType Type { get; init; }
	
	public Vector2 Offset { get; init; }
	public Vector2 OffsetDimensions { get; init; }

	public Censorable(string addon, CensorType type, Vector2 offset = default, Vector2 offsetDim = default)
	{
		if (type != CensorType.Full && (offset == default || offsetDim == default))
			throw new ArgumentException($"If no offset or dimensions are specified, type must be Full.");
		if (type == CensorType.Full && (offset != default || offsetDim != default))
			throw new ArgumentException($"If offset or dimensions are specified, type must not be Full.");

		AddonString = addon;
		Type = type;
		Offset = offset;
		OffsetDimensions = offsetDim;
	}
}