using System.Collections.Generic;

using UnityEngine;

public class NoteManager : MonoBehaviour {
	private static NoteManager _instance;
	public static NoteManager Instance {
		get { return _instance ?? (_instance = FindObjectOfType<NoteManager>()); }
	}

	public const float Padding = 0.5f;
	public const float NoteRange = 165 * 2;

	public const int ChromaticScaleTones = 12;

	public const float DefaultHorizontalSpacing = 10f;

	public const float Temperament = 1.059463f;

	public Note NotePrefab;
	public LineRenderer MeasurePrefab;

	public List<Note> NoteObjs;

	public Transform Measures;
	public Transform Notes;

	private int _measuresPerScreen;

	private void Update() {
		DrawMeasureBars();
	}

	public void Populate() {
		InitializeNotes();
		SetupMeasureBars();
	}

	public void ClearNotes() {
		foreach (var note in NoteObjs) {
			GameObject.Destroy(note.gameObject);
		}
		
		NoteObjs.Clear();
	}

	public void AddNote(Vector2 start, Vector2 end) {
		var levelData = DataManager.Instance.LevelData;

		var note = GameObject.Instantiate(NotePrefab, Notes, true);
		note.name = $"Note {NoteObjs.Count}";
		note.transform.position = Vector3.zero;

		// Sort start and end.
		if (start.x > end.x) {
			var temp = start;
			start = end;
			end = temp;
		}

		// Move relative to build track position.
		start += (Vector2)transform.position;
		end += (Vector2)transform.position;

		note.SetNote(start, end, levelData.NoteColorStart, levelData.NoteColorEnd);

		NoteObjs.Add(note);
	}

	public void DeleteNote(Note note) {
		GameObject.Destroy(note.gameObject);
		NoteObjs.Remove(note);
	}

	public void RedrawNoteLengths(float oldScrollSpeed, float oldNoteSpacing) {
		foreach (var noteObj in NoteObjs) {
			var start = TrackPositionToPositionData(noteObj.StartNode.transform.position, oldScrollSpeed, oldNoteSpacing);
			var end = TrackPositionToPositionData(noteObj.EndNode.transform.position, oldScrollSpeed, oldNoteSpacing);

			var newStart = PositionDataToTrackPosition(start);
			var newEnd = PositionDataToTrackPosition(end);

			// Move relative to build track position.
			newStart += (Vector2)transform.position;
			newEnd += (Vector2)transform.position;

			noteObj.SetNote(newStart, newEnd);
		}
	}

	private void InitializeNotes() {
		// Clear the notes first.
		ClearNotes();

		// Then load the notes from the file.
		var notes = DataManager.Instance.LevelData.savedleveldata;

		if (notes == null || notes.Count < 0) {
			return;
		}

		for (var i = 0; i < notes.Count; i++) {
			var note = notes[i];

			// Position data seems to be in [x, deltaX, y, deltaY, y]
			var start = PositionDataToTrackPosition(new Vector2(note[0], note[2]));
			var end = PositionDataToTrackPosition(new Vector2(note[0] + note[1], note[2] + note[3]));

			AddNote(start, end);
		}
	}

	private void CreateMeasureBars() {
		var levelData = DataManager.Instance.LevelData;

		for (var i = Measures.childCount; i < _measuresPerScreen; i++) {
			var measure = GameObject.Instantiate(MeasurePrefab);
			measure.transform.SetParent(Measures);
		}
	}

	public void SetupMeasureBars() {
		var levelData = DataManager.Instance.LevelData;
		var spacing = DataPanel.Instance.ScrollSpeed * DataManager.Instance.NoteSpacing * DataManager.Instance.TimeSignature;

		var screenHeight = Camera.main.orthographicSize * 2f;
		var screenWidth = screenHeight * Camera.main.aspect;

		_measuresPerScreen = (int)Mathf.Ceil(screenWidth / spacing) + 1;
		CreateMeasureBars();
	}

	public void DrawMeasureBars() {
		var spacing = DataPanel.Instance.ScrollSpeed * DataManager.Instance.NoteSpacing * DataManager.Instance.TimeSignature;

		// Don't draw anything if they're behind the starting position
		if (transform.position.x > 0) {
			return;
		}

		// However many measures far is this track is.
		var measuresTrackPosition = (int)(-transform.position.x / spacing);

		for (var i = 0; i < _measuresPerScreen; i++) {
			var measure = Measures.GetChild(i)?.GetComponent<LineRenderer>();

			if (measure == null) {
				return;
			}

			// Get track position.
			var xPos = measuresTrackPosition * spacing + spacing * i;

			if (xPos < 0) {
				continue;
			}

			measure.SetPosition(0, new Vector3(xPos, -5f));
			measure.SetPosition(1, new Vector3(xPos, 5f));
		}
	}

	/// <summary>
	/// Transforms position data from save files to the track position in this editor.
	/// </summary>
	/// <param name="pos">The position in data</param>
	/// <returns>The track position</returns>
	public Vector2 PositionDataToTrackPosition(Vector3 pos) {
		// Divide by the note range (165, -165) so the note's pos is in (-1, 1), then multiply by the screen height - padding
		var verticalSpacing = (Camera.main.orthographicSize * 2f - Padding * 2f) / NoteRange;

		// Note length (savednotesspacing) * scroll speed
		var noteLength = DataManager.Instance.NoteSpacing * DataPanel.Instance.ScrollSpeed;

		var newPos = Vector3.Scale(pos, new Vector2(noteLength, verticalSpacing));

		// Return the pos relative to the build track.
		return newPos;
	}

	/// <summary>
	/// Transforms track position in this editor to position data for the save file.
	/// </summary>
	/// <param name="pos">Track position</param>
	/// <param name="scrollSpeed">Scroll speed from file data</param>
	/// <param name="noteSpacing">Note spacing from file data</param>
	/// <returns>Position in data</returns>
	public Vector3 TrackPositionToPositionData(Vector3 pos, float scrollSpeed, float noteSpacing) {
		// Get pos relative to the build track
		pos -= transform.position;

		// Divide by the screen height - padding so the note's pos is in (-1, 1), then multiply it by the note range
		var verticalSpacing = NoteRange / (Camera.main.orthographicSize * 2 - Padding * 2f);

		// Note length (savednotesspacing) * scroll speed
		var noteLength = 1f / (noteSpacing * scrollSpeed);

		return pos * new Vector2(noteLength, verticalSpacing);
	}

	public float GetNearestBeat(float pos) {
		// Put this in local position
		var spacing = DataPanel.Instance.ScrollSpeed * DataManager.Instance.NoteSpacing / (DataPanel.Instance.Subdivision / 4);
		return Mathf.Round(pos / spacing) * spacing;
	}

	/// <summary>
	/// Gets the pitch from the y-position in the editor.
	/// </summary>
	/// <param name="yPos">Y-position</param>
	/// <returns>Pitch</returns>
	public static float GetPitch(float yPos) {
		var screenHeight = (Camera.main.orthographicSize * 2f - Padding * 2f) / 2f;

		// Put it in (-1, 1) then get it in (-12, 12)
		return yPos / screenHeight * (float)ChromaticScaleTones;
	}

	/// <summary>
	/// Gets the pitch from the y-position in the editor, but rounds it to the nearest chromatic note on track.
	/// </summary>
	/// <param name="pos">Y-position</param>
	/// <returns>Chromatic pitch track position</returns>
	public static float GetNearestChromaticPitch(float pos) {
		var screenHeight = (Camera.main.orthographicSize * 2f - Padding * 2f) / 2f; 

		// Put it in (-1, 1) then get it in (-12, 12)
		var note = Mathf.Round(pos / screenHeight * (float)ChromaticScaleTones);
		note = Mathf.Clamp(note, -13f, 13f);

		// Put it back into screen coords
		return note / (float)ChromaticScaleTones * screenHeight;
	}

	/// <summary>
	/// Gets the pitch relative to it's distance from the C5 note.
	/// </summary>
	/// <param name="semitoneDistanceFromC">Semitone distance from C5</param>
	/// <returns>Pitch ratio of this note's frequency / 440 Hz</returns>
	public static float GetPitchRelativeTo440(float semitoneDistanceFromC) {
		var pitch = 440f * Mathf.Pow(NoteManager.Temperament, (52 + semitoneDistanceFromC) - 49);
		return pitch / 440f;
	}
}
