using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
	private static BuildManager _instance;
	public static BuildManager Instance {
		get { return _instance ?? (_instance = FindObjectOfType<BuildManager>()); }
	}

	public List<NoteNode> MovingNodes;
	public List<NoteNode> SelectedNodes;
	public Note BuildGhostNote;
	public Cursor Cursor;
	public Transform BuildTrack;

	public AudioSource MusicTrack;
	public AudioSource Trombone;

	private int _uiLayer;

	public bool CreatingNode {
		get; private set;
	}

	public Vector3 MousePosition {
		get; private set;
	}

	public Vector3 TrackMousePosition {
		get { return MousePosition - BuildTrack.position; }
	}

	public bool IsPlaying {
		get; private set;
	}

	public bool IsMovingNode {
		get; private set;
	}

	private void Start() {
		_uiLayer = LayerMask.NameToLayer("UI");
		Cursor = FindObjectOfType<Cursor>();
		BuildGhostNote.gameObject.SetActive(false);
		Trombone.volume = 0;
	}

	private void Update() {
		var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		mousePos.z = 0;
		MousePosition = mousePos;

		CheckPlaying();
		CheckScroll();

		if (Input.GetKeyUp(KeyCode.Backspace)) {
			BuildTrack.position = new Vector3(0, 0);
		}

		if (Input.GetKeyUp(KeyCode.X) && !IsPlaying) {
			foreach (var node in SelectedNodes) {
				NoteManager.Instance.DeleteNote(node.ParentNote);
			}

			SelectedNodes.Clear();
		}

		// Make sure we're not editing the board when we're clicking data panel stuff.
		if (IsPointerOverUIElement()) {
			return;
		}

		CheckPreview();
		CheckCreating();
		CheckMoving();
	}

	public void SetAudioFile(AudioClip soundClip) {
		MusicTrack.clip = soundClip;
	}

	private void CheckCreating() {
		if (Input.GetMouseButtonDown(0) && !IsPlaying) {
			CreateNode();
		}
		else if (Input.GetMouseButton(0) && CreatingNode) {
			BuildGhostNote.SetNote(BuildGhostNote.StartNode.transform.position, Cursor.transform.position);
		}
		else if (Input.GetMouseButtonUp(0) && CreatingNode) {
			EndCreatingNode();
		}
	}

	private void CreateNode() {
		CreatingNode = true;

		BuildGhostNote.gameObject.SetActive(true);
		BuildGhostNote.StartNode.transform.position = Cursor.transform.position;
	}

	private void EndCreatingNode() {
		CreatingNode = false;

		var start = BuildGhostNote.StartNode.transform.position;
		var end = BuildGhostNote.EndNode.transform.position;

		// Get position relative to track
		start -= BuildTrack.position;
		end -= BuildTrack.position;

		if (Mathf.Abs(start.x - end.x) < float.Epsilon) {
			BuildGhostNote.gameObject.SetActive(false);
			return;
		}

		NoteManager.Instance.AddNote(
			start,
			end
		);

		BuildGhostNote.gameObject.SetActive(false);
	}

	private void CheckPlaying() {
		// Check playing input.
		if (Input.GetKeyUp(KeyCode.Space)) {
			TogglePlaying();
		}

		if (!IsPlaying) {
			return;
		}

		// Handle playing scrolling updates.
		var levelData = DataManager.Instance.LevelData;
		var scrollSpeed = DataManager.Instance.NoteSpacing * DataManager.Instance.BPM / 60f * DataPanel.Instance.ScrollSpeed;
		BuildTrack.position -= new Vector3(scrollSpeed * Time.deltaTime, 0f);

		// If the position is greater than 0, don't play the music track yet.
		if (BuildTrack.position.x > 0) {
			MusicTrack.Stop();
		} else if (-8 - BuildTrack.position.x >= 0  && !MusicTrack.isPlaying) {
			MusicTrack.Play();
		}
	}

	private void CheckScroll() {
		if (IsPlaying) {
			return;
		}

		var scroll = Input.mouseScrollDelta.y;
		BuildTrack.position += new Vector3(scroll * 2, 0);

		var levelData = DataManager.Instance.LevelData;
		var beatTime = 1 / (DataManager.Instance.BPM / 60f); // seconds per beat
		var lengthInBeats = (-8 - BuildTrack.position.x) / (DataManager.Instance.NoteSpacing * DataPanel.Instance.ScrollSpeed);

		var time = lengthInBeats * beatTime;

		if (time > 0) {
			MusicTrack.time = time;
		}
	}

	private void CheckPreview() {
		if (Input.GetMouseButtonDown(0) || IsMovingNode) {
			CheckPreviewNote();
		} else if (Input.GetMouseButtonUp(0)) {
			EndPreviewNote();
		}
		
		if (Input.GetMouseButton(0) || IsMovingNode) {
			CheckPreviewNotePitch();
		}
		
		
	}

	private void CheckPreviewNote() {
		if (DataPanel.Instance.PreviewNote) {
			Trombone.volume = 1;
		}
	}

	private void CheckPreviewNotePitch() {
		var semitoneDistanceFromC = NoteManager.GetPitch(Cursor.transform.position.y);
		var pitch = NoteManager.GetPitchRelativeTo440(semitoneDistanceFromC);

		Trombone.pitch = pitch;
	}

	private void EndPreviewNote() {
		Trombone.volume = 0;
	}
	
	private void TogglePlaying() {
		if (IsPlaying) {
			IsPlaying = false;
			DataPanel.Instance.gameObject.SetActive(true);

			MusicTrack.Stop();
			Trombone.volume = 0;
		} else {
			StartPlaying();
		}
	}

	private void StartPlaying() {
		if (CreatingNode) {
			return;
		}

		var levelData = DataManager.Instance.LevelData;

		IsPlaying = true;
		DataPanel.Instance.gameObject.SetActive(false);
	}

	private void CheckMoving() {
		if (Input.GetMouseButtonUp(1) && IsMovingNode) {
			IsMovingNode = false;
			MovingNodes.Clear();

			Trombone.volume = 0;
		}
		else if (Input.GetMouseButton(1) && MovingNodes.Count != 0) {
			IsMovingNode = true;
			foreach (var node in MovingNodes) {
				node.Move(Cursor.transform.position);
			}
		}
	}

	public void MoveNode(NoteNode node) {
		if (MovingNodes.Count > 0 && IsMovingNode) {
			MovingNodes.Clear();
		}

		MovingNodes.Add(node);
	}

	public void SelectNode(NoteNode node) {
		if (IsPlaying) {
			return;
		}

		if (SelectedNodes.Count > 0) {
			foreach (var selectedNode in SelectedNodes) {
				selectedNode.Highlighted = false;
			}
			SelectedNodes.Clear();
		}

		node.Highlighted = true;
		SelectedNodes.Add(node);
	}

	// Taken from https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/
	//Returns 'true' if we touched or hovering on Unity UI element.
	public bool IsPointerOverUIElement() {
		return IsPointerOverUIElement(GetEventSystemRaycastResults());
	}


	//Returns 'true' if we touched or hovering on Unity UI element.
	private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults) {
		for (int index = 0; index < eventSystemRaysastResults.Count; index++) {
			RaycastResult curRaysastResult = eventSystemRaysastResults[index];
			if (curRaysastResult.gameObject.layer == _uiLayer)
				return true;
		}
		return false;
	}


	//Gets all event system raycast results of current mouse or touch position.
	static List<RaycastResult> GetEventSystemRaycastResults() {
		PointerEventData eventData = new PointerEventData(EventSystem.current);
		eventData.position = Input.mousePosition;
		List<RaycastResult> raysastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, raysastResults);
		return raysastResults;
	}
}
