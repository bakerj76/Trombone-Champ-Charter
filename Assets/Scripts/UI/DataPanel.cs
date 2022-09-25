using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

using TMPro;
using SFB;

public class DataPanel : MonoBehaviour
{
	public const int DefaultNoteLength = 5;

	private static DataPanel _instance;
	public static DataPanel Instance {
		get { return _instance ?? (_instance = FindObjectOfType<DataPanel>()); }
	}

	public TMP_InputField BPMInput;
	public TMP_InputField EndPointInput;
	public TMP_InputField TimeSignatureInput;
	public TMP_InputField NoteSpacingInput;

	public ColorPicker StartNotePicker;
	public ColorPicker EndNotePicker;

	public Toggle SnapToChromaToggle;
	public Toggle SnapToRhythmToggle;
	public Toggle PreviewNoteToggle;

	public Slider ScrollSpeedSlider;

	public TMP_Dropdown SubdivisionPicker;

	private float _scrollSpeed;
	public float ScrollSpeed {
		get { return _scrollSpeed; }
	}

	public bool SnapToChroma {
		get { return SnapToChromaToggle.isOn; }
	}

	public bool SnapToRhythm {
		get { return SnapToRhythmToggle.isOn; }
	}

	public bool PreviewNote {
		get { return PreviewNoteToggle.isOn; }
	}

	/// <summary>
	/// Gets 1/subdivision.
	/// </summary>
	public float Subdivision {
		get {
			var option = SubdivisionPicker.options[SubdivisionPicker.value];
			return float.Parse(
				option.text.Substring(2)
			);
		}
}


	private void Start() {
		DataManager.Instance.OnLevelLoaded += Populate;
		_scrollSpeed = ScrollSpeedSlider.value;

		var levelData = DataManager.Instance.LevelData;

		// Set level data on UI element changes
		BPMInput.onValueChanged.AddListener(v => {
			float value;
			if (float.TryParse(v, out value)) {
				DataManager.Instance.BPM = value;
				return;
			}
		});
		EndPointInput.onValueChanged.AddListener(v => {
			float value;
			if (float.TryParse(v, out value)) {
				DataManager.Instance.EndPoint = value;
				return;
			}
		});
		TimeSignatureInput.onValueChanged.AddListener(v => {
			if (levelData == null) {
				return;
			}

			levelData.timesig = int.Parse(v);
			NoteManager.Instance.SetupMeasureBars();
		});
		NoteSpacingInput.onValueChanged.AddListener(v => {
			if (levelData == null) {
				return;
			}

			levelData.savednotespacing = int.Parse(v);
			NoteManager.Instance.SetupMeasureBars();
		});

		StartNotePicker.OnValueChanged += v => {
			if (levelData == null) {
				return;
			}

			levelData.note_color_start = new float[3] { v.r, v.g, v.b };
		};
		EndNotePicker.OnValueChanged += v => {
			if (levelData == null) {
				return;
			}

			levelData.note_color_end = new float[3] { v.r, v.g, v.b };
		};

		ScrollSpeedSlider.onValueChanged.AddListener(v => {
			var lastScrollSpeed = _scrollSpeed;
			_scrollSpeed = v;
			NoteManager.Instance.SetupMeasureBars();
			NoteManager.Instance.RedrawNoteLengths(lastScrollSpeed, DataManager.Instance.NoteSpacing);
		});
	}

	public void OpenTMBFilePicker() {
		var files = StandaloneFileBrowser.OpenFilePanel("Open File", "", "tmb", false);
		if (files.Length == 0) {
			return;
		}

		DataManager.Instance.OpenTMBFile(files[0]);
	}

	public void OpenMusicFilePicker() {
		var files = StandaloneFileBrowser.OpenFilePanel("Open File", "", new []{ new ExtensionFilter("audio", "ogg", "wav") }, false);
		if (files.Length == 0) {
			return;
		}

		var audioFile = files[0];
		StartCoroutine(LoadMusicFile(audioFile));
	}

	private IEnumerator LoadMusicFile(string audioFile) {
		var audioType = audioFile.EndsWith(".wav") ? AudioType.WAV : AudioType.OGGVORBIS;

		using (var wr = UnityWebRequestMultimedia.GetAudioClip($"file://localhost/{audioFile}", audioType)) {
			yield return wr.SendWebRequest();

			if (wr.result != UnityWebRequest.Result.Success) {
				Debug.Log(wr.error);
				yield return null;
			}

			var clip = DownloadHandlerAudioClip.GetContent(wr);
			BuildManager.Instance.SetAudioFile(clip);
		}
	}

	public void SaveTMBFile() {
		var savePath = StandaloneFileBrowser.SaveFilePanel("Save File", "", "new_song", "tmb");

		if (savePath == "") {
			return;
		}

		DataManager.Instance.SaveTMBFile(savePath);
	}


	public void Populate() {
		var levelData = DataManager.Instance.LevelData;

		BPMInput.text = levelData.tempo.ToString();
		EndPointInput.text = levelData.endpoint.ToString();
		TimeSignatureInput.text = levelData.timesig.ToString();
		NoteSpacingInput.text = (levelData.savednotespacing > 0 ? levelData.savednotespacing : DefaultNoteLength).ToString();

		StartNotePicker.SetColor(levelData.note_color_start[0], levelData.note_color_start[1], levelData.note_color_start[2]);
		EndNotePicker.SetColor(levelData.note_color_end[0], levelData.note_color_end[1], levelData.note_color_end[2]);

		FindObjectOfType<NoteManager>().Populate();
	}
}
