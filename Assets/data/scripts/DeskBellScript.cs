using System.Threading;
using UnityEngine;

public class DeskBellScript : MonoBehaviour {

	public Transform button;
	public AudioSource audioSource;
	private GameController gc;
	public Vector3 startingButonPos;
	public Vector3 endingButonPos;
	private CancellationTokenSource cancellationSource;
	void Start() {

		//Get the game controller
		gc = FindFirstObjectByType<GameController>();
		startingButonPos = button.position;
		endingButonPos = startingButonPos - new Vector3(0, 0.0177f, 0);

	}

	public async void RingBell() {
		audioSource.PlayOneShot(audioSource.clip);
		if (cancellationSource is not null) {
			cancellationSource.Cancel();
		}

		cancellationSource = new CancellationTokenSource();
		var startPos = button.position;
		await gc.Translate(button, endingButonPos, 20, EasingFunction.Ease.EaseOutQuad, cancellationSource.Token);
		await gc.Translate(button, startingButonPos, 20, EasingFunction.Ease.EaseOutQuad, cancellationSource.Token);
		cancellationSource = null;

	}

}
