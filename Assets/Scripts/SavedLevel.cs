using System;
using System.Collections.Generic;

using UnityEngine;

[Serializable]
public class SavedLevel : ICloneable {
	public List<float[]> savedleveldata;
	public List<float[]> bgdata;
	public float endpoint;
	public List<float[]> lyricspos;
	public List<string> lyricstxt;

	public int savednotespacing;
	public float tempo;
	public int timesig;
	public float[] note_color_start;
	public float[] note_color_end;

	public Color NoteColorStart {
		get { return new Color(note_color_start[0], note_color_start[1], note_color_start[2]); }
	}

	public Color NoteColorEnd {
		get { return new Color(note_color_end[0], note_color_end[1], note_color_end[2]); }
	}

	public object Clone() {
		return (SavedLevel)MemberwiseClone();
	}
}