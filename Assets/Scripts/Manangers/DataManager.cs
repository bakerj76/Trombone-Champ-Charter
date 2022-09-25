using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using UnityEngine;

public class DataManager : MonoBehaviour
{
	private static DataManager _instance;
	public static DataManager Instance {
		get { return _instance ?? (_instance = FindObjectOfType<DataManager>()); }
	}

	private SavedLevel _levelData;
	public SavedLevel LevelData {
		get { return _levelData; } set { SetLevelData(value); }
	}

	public delegate void OnLevelLoadedDelegate();
	public OnLevelLoadedDelegate OnLevelLoaded;

	public float BPM {
		get { return _levelData.tempo; }
		set { _levelData.tempo = value; }
	}

	public int NoteSpacing {
		get { return _levelData.savednotespacing; }
		set { _levelData.savednotespacing = value; }
	}

	public int TimeSignature {
		get { return _levelData.timesig; }
		set { _levelData.timesig = value; }
	}

	public float EndPoint {
		get { return _levelData.endpoint; }
		set { _levelData.endpoint = value; }
	}

	void Awake() {
		// Create some default data
		LevelData = new SavedLevel {
			bgdata = new List<float[]>(),
			endpoint = 20f,
			lyricspos = new List<float[]>(),
			lyricstxt = new List<string>(),
			
			savednotespacing = 5,
			tempo = 120,
			timesig = 4,
			note_color_start = new float[3] { 1f, 0f, 0f },
			note_color_end = new float[3] { 0f, 0f, 1f },
		};
	}

	void Start() {
        if (Instance == null) {
			Debug.LogError("No DataManager in scene");
		}

		OnLevelLoaded?.Invoke();
	}

	public void OpenTMBFile(string path) {
		var bf = new BinaryFormatter();
		SavedLevel levelData;

		using(var fs = File.Open(path, FileMode.Open)) {
			levelData = (SavedLevel)bf.Deserialize(fs);

			// Make this a reasonable number for the editor
			levelData.savednotespacing = 5;
		}

		LevelData = levelData;
	}

	public void SaveTMBFile(string path) {
		var bf = new BinaryFormatter();
		var newSavedLevel = (SavedLevel)LevelData.Clone();
		var newSavedLevelData = new List<float[]>();

		newSavedLevel.bgdata = new List<float[]>();
		newSavedLevel.lyricspos = new List<float[]>();
		newSavedLevel.lyricstxt = new List<string>();

		foreach (var note in NoteManager.Instance.NoteObjs) {
			var startDataPos = NoteManager.Instance.TrackPositionToPositionData(note.StartNode.transform.position, DataPanel.Instance.ScrollSpeed, NoteSpacing);
			var endDataPos = NoteManager.Instance.TrackPositionToPositionData(note.EndNode.transform.position, DataPanel.Instance.ScrollSpeed, NoteSpacing);
			var deltaPos = endDataPos - startDataPos;
			var data = new[] { startDataPos.x, deltaPos.x, startDataPos.y, deltaPos.y, startDataPos.y };

			// print($"{data[0]}, {data[1]}, {data[2]}, {data[3]}, {data[4]}");

			newSavedLevelData.Add(new[] { startDataPos.x, deltaPos.x, startDataPos.y, deltaPos.y, startDataPos.y });
		}

		newSavedLevel.savedleveldata = newSavedLevelData;
		newSavedLevel.savednotespacing = 140;

		using(var fs = File.Create(path)) {
			bf.Serialize(fs, newSavedLevel);
		}
	}

	private void SetLevelData(SavedLevel value) {
		_levelData = value;
		OnLevelLoaded?.Invoke();
	}
}
