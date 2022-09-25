using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteNode : MonoBehaviour
{
	public GameObject Highlight;
	public Note ParentNote;

	private bool _highlighted;
	public bool Highlighted {
		get { return _highlighted; }
		set { SetHighlighted(value); }
	}

	private void Start() {
		if (transform.childCount == 0) {
			Debug.LogError("No highlight on note node found");
			return;
		}

		ParentNote = transform.parent.GetComponent<Note>();

		Highlight = transform.GetChild(0).gameObject;
	}

	private void Update() {
		var mousePosition = BuildManager.Instance.MousePosition;

		if (Input.GetMouseButtonDown(1) && (mousePosition - transform.position).sqrMagnitude < 0.16) {
			BuildManager.Instance.SelectNode(this);
			BuildManager.Instance.MoveNode(this);
		}
	}

	public void Move(Vector2 pos) {
		if (ParentNote.StartNode == this) {
			ParentNote.SetNote(pos, ParentNote.EndNode.transform.position);
		} else {
			ParentNote.SetNote(ParentNote.StartNode.transform.position, pos);
		}
	}

	private void SetHighlighted(bool value) {
		_highlighted = value;
		Highlight.SetActive(value);
	}
}
