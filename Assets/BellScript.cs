using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class BellScript : MonoBehaviour {

	public AudioSource bellAudioSource;
	public AudioClip[] sfxDoorBells;
	private bool debounce;
	private GameController gc;

	void Start() {
		gc = FindFirstObjectByType<GameController>();
	}

	// Update is called once per frame
	void Update() {

	}

	private async Task OnCollisionEnter(Collision other) {
		if (!debounce) {

			bellAudioSource.PlayOneShot(sfxDoorBells[Random.Range(0, sfxDoorBells.Length)]);
			bellAudioSource.pitch = Random.Range(0.9f, 1.1f);
			debounce = true;
			
			await UniTask.Delay(1000);
			debounce = false;
		}
	}
}
