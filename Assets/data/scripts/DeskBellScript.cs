using UnityEngine;

public class DeskBellScript : MonoBehaviour {

	public Transform button;
	public AudioSource audioSource;
	private GameController gc;

	void Start() {

		//Get the game controller
		gc = FindFirstObjectByType<GameController>();

	}

	// Update is called once per frame
	void Update() {

	}

	public async void RingBell() {
		audioSource.PlayOneShot(audioSource.clip);
		var startPos = button.position;
		await gc.Translate(button, startPos - new Vector3(0, 0.0177f, 0), 10, EasingFunction.Ease.EaseOutQuad);
		await gc.Translate(button, startPos, 10, EasingFunction.Ease.EaseOutQuad);
	}



	private void OnTriggerEnter(Collider other) {
		gc.inBellTrigger = true;
	}

	private void OnTriggerExit(Collider other) {
		gc.inBellTrigger = false;
	}
}
