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


	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start() {
			textValue = $"{artistsName}{(isFake ? " (FAKE) " : " ")}from seller {npcsName} - ${salePrice} (${profit} profit)";
	}

	// Update is called once per frame
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
