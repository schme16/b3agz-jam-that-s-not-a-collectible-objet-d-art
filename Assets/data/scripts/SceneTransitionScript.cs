using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionScript : MonoBehaviour {

	public Image fader;
	public float fadeTime;
	private CancellationTokenSource cancellationSource = new CancellationTokenSource();

	void Awake() {
		DontDestroyOnLoad(gameObject);
	}

	public async UniTask FadeScreenIn(CancellationToken cancellationToken = default) {

		
		fader.raycastTarget = true;

		//If the alpha is greater than 0 aka invisible
		while (fader.color.a > 0 && !cancellationToken.IsCancellationRequested) {

			//Reduce the alpha a bit
			fader.color = new Color(fader.color.r, fader.color.g, fader.color.b, Mathf.Clamp(fader.color.a - (Time.deltaTime / fadeTime), 0, 255));

			//Yeild to other processes
			await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
		}
		fader.raycastTarget = false;
	}

	public async UniTask FadeScreenOut(CancellationToken cancellationToken = default) {




		fader.raycastTarget = true;

		//If the alpha is less than 1 aka full visible
		while (fader.color.a < 1 && !cancellationToken.IsCancellationRequested) {

			//Increase the alpha a bit
			fader.color = new Color(fader.color.r, fader.color.g, fader.color.b, Mathf.Clamp(fader.color.a + (Time.deltaTime / fadeTime), 0, 1));

			//Yeild to other processes
			await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
		}

	}

	public async void ChangeSceneVoid(string scene) {
		await ChangeScene(scene, false);
	}

	public async UniTask ChangeScene(string scene, bool skipFade) {
		cancellationSource = new CancellationTokenSource();
		fader.color = new Color(fader.color.r, fader.color.g, fader.color.b, 0);

		//Fade the screen out
		if (!skipFade) {
			await FadeScreenOut(cancellationSource.Token);
		}

		//Wait a beat
		await UniTask.Delay(100, cancellationToken: cancellationSource.Token);

		//Load the new scene
		SceneManager.LoadScene(scene);
		fader.color = Color.black;

		//Wait few beats
		await UniTask.Delay(300, cancellationToken: cancellationSource.Token);

		//Fade the scene back in
		if (!skipFade) {
			await FadeScreenIn(cancellationSource.Token);
		}
	}


}
