using TMPro;
using UnityEngine;

public class JournalEntryScript : MonoBehaviour {


	public string type;
	public string npcsName;
	public string artistsName;
	public int salePrice;
	public int realAValue;
	public int profit;
	public bool isFake;
	public TextMeshPro uiTypeText;
	public TextMeshPro uiDescriptionText;
	public string textValue;
	public string lastTextValue = "";
	public string lastType = "-";
	public GameController gc;


	void Start() {

		//Get the game controller
		gc = FindFirstObjectByType<GameController>();

		if (gc.flags.accessibilityMode) {
			uiTypeText.font = gc.accessibilityFont;
			uiDescriptionText.font = gc.accessibilityFont;
		}
		textValue = $"{artistsName}{(isFake ? " (FAKE) " : " ")}from seller {npcsName} - ${salePrice} (${profit} profit)";
	}

	void Update() {
		if (artistsName.Length > 0) {
			textValue = $"{artistsName}{(isFake ? " (FAKE) " : "")}from seller {npcsName} - ${salePrice} (${profit} profit)";
		}
		else {
			textValue = "";
		}

		if (textValue != lastTextValue || type != lastType) {
			uiTypeText.SetText(type);
			uiDescriptionText.SetText(textValue);
			lastTextValue = textValue;
			lastType = type;
		}
	}
}
