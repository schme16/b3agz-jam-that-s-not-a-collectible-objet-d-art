using TMPro;
using UnityEngine;

public class SyncFontChoice : MonoBehaviour {


	private GameController gc;


	void Start() {

		//Get the game controller
		gc = FindFirstObjectByType<GameController>();

		var text = gameObject.GetComponentsInChildren<TextMeshProUGUI>();

		if (gc.flags.cleanFonts) {
			foreach (var textMeshProUGUI in text) {
				textMeshProUGUI.gameObject.AddComponent<FontBackup>();

				textMeshProUGUI.GetComponent<FontBackup>().originalFont = textMeshProUGUI.font;

				textMeshProUGUI.font = gc.accessibilityFont;
			}
		}

	}

	// Update is called once per frame
	void Update() {

	}
}
