using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour {

	public SaveManagerScript SaveManager;
	public List<GameObject> paintings;
	public GameController.Flags flags;
	public GameObject uiContinueButton;
	public GameObject uiNewGameButton;
	public SceneTransitionScript SceneManager;
	public Toggle toggle;
	public Toggle cleanFonts;
	public Toggle muteMusic;
	public AudioSource musicPlayer;

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

		if (!flags.hasBeenLoaded) {
			flags.accessibilityMode = true;
			
			SaveManager.SaveFlags(flags);
		}
		
		toggle.isOn = flags.accessibilityMode;
		
		cleanFonts.isOn = flags.cleanFonts;
		
		muteMusic.isOn = flags.muteMusic;
		
		musicPlayer.mute = flags.muteMusic;

		
		uiContinueButton.SetActive(flags.hasBeenLoaded);
	}

	public void ContinueGame() {
		SceneManager.ChangeSceneVoid("main");
	}

	public void NewGame() {
		
		var accessibilityState = flags.accessibilityMode;
		var cleanFontsState = flags.cleanFonts;
		var muteMusicState = flags.muteMusic;
		
		SaveManager.ResetAllSaves();
		
		
		flags = SaveManager.LoadFlags();
		
		var newFlags = flags;

		newFlags.accessibilityMode = accessibilityState;
		
		newFlags.cleanFonts = cleanFontsState;
		
		newFlags.muteMusic = muteMusicState;
		
		flags = newFlags;
		
		SaveManager.SaveFlags(flags);
		
		SceneManager.ChangeSceneVoid("main");
	}


	public void SetAccessibilityToggle() {

		var newFlags = flags;
		newFlags.accessibilityMode = toggle.isOn;
		flags = newFlags;
		SaveManager.SaveFlags(flags);
	}

	
	public void SetCleanFontsToggle() {

		var newFlags = flags;
		newFlags.cleanFonts = cleanFonts.isOn;
		
		flags = newFlags;
		SaveManager.SaveFlags(flags);
	}

	
	public void MuteMusicToggle() {

		var newFlags = flags;
		newFlags.muteMusic = muteMusic.isOn;

		musicPlayer.mute = newFlags.muteMusic;
		
		flags = newFlags;
		SaveManager.SaveFlags(flags);
	}

}
