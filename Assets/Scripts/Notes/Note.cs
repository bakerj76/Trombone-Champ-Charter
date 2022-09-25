using UnityEngine;

public class Note : MonoBehaviour
{
	public const int GlissandoPositions = 10;

	public NoteNode StartNode;
	public NoteNode EndNode;

	private LineRenderer _slur;


	void Awake() {
		_slur = GetComponent<LineRenderer>();
	}

	public void SetNote(Vector2 start, Vector2 end, Color? startColor = null, Color? endColor = null) {
		var realStartColor = startColor.HasValue ? startColor.Value : DataManager.Instance.LevelData.NoteColorStart;
		var realEndColor = endColor.HasValue ? endColor.Value : DataManager.Instance.LevelData.NoteColorEnd;

		StartNode.transform.position = start;
		StartNode.GetComponent<SpriteRenderer>().color = realStartColor;

		EndNode.transform.position = end;
		EndNode.GetComponent<SpriteRenderer>().color = realEndColor;

		EaseGlissando(start, end);

		_slur.startColor = realStartColor;
		_slur.endColor = realEndColor;
	}

	public void EaseGlissando(Vector2 start, Vector2 end) {
		// Get pos relative to the note's position
		start -= (Vector2)transform.position;
		end -= (Vector2)transform.position;

		if (Mathf.Abs(start.y - end.y) < float.Epsilon) {
			_slur.positionCount = 2;
			_slur.SetPosition(0, start);
			_slur.SetPosition(1, end);
			return;
		}

		_slur.positionCount = GlissandoPositions;

		for (var i = 0; i < GlissandoPositions; i++) {
			var t =  (float)i / ((float)GlissandoPositions - 1f);
			var xPos = start.x + (end.x - start.x) * t;
			var yPos = GetGlissandoPosition(t);

			_slur.SetPosition(i, new Vector3(xPos, yPos, 0f));
		}
	}

	public float GetGlissandoPosition(float t) {
		var start = StartNode.transform.position;
		var end = EndNode.transform.position;

		if (Mathf.Abs(start.y - end.y) < float.Epsilon) {
			return start.y;
		}

		t *= 2;
		var y = t * t;

		if (t >= 1f) {
			t -= 1f;
			y = -(t * (t - 2f) - 1f);
		}

		return y / 2f * (end.y - start.y) + start.y;
	}
}
