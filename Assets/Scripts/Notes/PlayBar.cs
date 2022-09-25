using System.Linq;
using UnityEngine;

public class PlayBar : MonoBehaviour
{
    void Update() {
		if (!BuildManager.Instance.IsPlaying) {
			return;
		}

		var trombone = BuildManager.Instance.Trombone;
		var playingFieldX = NoteManager.Instance.transform.position.x;

		var playedNote = NoteManager.Instance.NoteObjs.Find(noteObj => {
			var start = playingFieldX + noteObj.StartNode.transform.position.x;
			var end = playingFieldX + noteObj.EndNode.transform.position.x;

			return start < transform.position.x &&
				end > transform.position.x;
		});

		if (playedNote == null) {
			trombone.volume = 0;
			return;
		}

		var t = (transform.position.x - playingFieldX - playedNote.StartNode.transform.position.x) / (playedNote.EndNode.transform.position.x - playedNote.StartNode.transform.position.x);
		var currentY = playedNote.GetGlissandoPosition(t);
		var semitoneDistanceFromC = NoteManager.GetPitch(currentY);
		var pitch = NoteManager.GetPitchRelativeTo440(semitoneDistanceFromC);

		trombone.pitch = pitch;
		trombone.volume = 1;
	}
}
