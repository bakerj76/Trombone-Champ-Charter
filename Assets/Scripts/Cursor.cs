using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
		var pos = BuildManager.Instance.MousePosition;

		if (DataPanel.Instance.SnapToRhythm) {
			// Get global position.
			pos.x = NoteManager.Instance.GetNearestBeat(BuildManager.Instance.TrackMousePosition.x);

			// Get local position.
			pos.x += NoteManager.Instance.transform.position.x;
		}

		if (float.IsNaN(pos.x)) {
			pos.x = 0;
		}

		if (DataPanel.Instance.SnapToChroma) {
			pos.y = NoteManager.GetNearestChromaticPitch(pos.y);
		}

		transform.position = pos;
	}
}
