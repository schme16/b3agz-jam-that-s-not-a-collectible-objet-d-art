using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {

	public SaveManagerScript SaveManager;
	public List<GameObject> paintings;
	public GameController.Flags flags;
	public GameObject uiContinueButton;
	public GameObject uiNewGameButton;
	public SceneTransitionScript SceneManager;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start() {

		SceneManager = FindFirstObjectByType<SceneTransitionScript>();
		foreach (var painting in paintings) {
			painting.SetActive(false);
		}


		var painting1 = Random.Range(0, paintings.Count);
		paintings[painting1].SetActive(true);
		paintings.Remove(paintings[painting1]);

		var painting2 = Random.Range(0, paintings.Count);
		paintings[painting2].SetActive(true);
		paintings.Remove(paintings[painting2]);

		var painting3 = Random.Range(0, paintings.Count);
		paintings[painting3].SetActive(true);
		paintings.Remove(paintings[painting3]);



		flags = SaveManager.LoadFlags();

		uiContinueButton.SetActive(flags.hasBeenLoaded);

	}

	public void ContinueGame() {
		SceneManager.ChangeSceneVoid("main");
	}

	public void NewGame() {
		SaveManager.ResetAllSaves();
		SceneManager.ChangeSceneVoid("main");
	}

}
