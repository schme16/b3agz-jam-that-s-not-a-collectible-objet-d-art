using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PaperScript : MonoBehaviour {

	public BillMaker script;

	public TextMeshProUGUI text;
	
	void Start () {
	}
	
	void Update () {
		if (script != null) {
			text.SetText(script.prenoun + " the " + script.fluff + " of " + script.noun + " " + script.verb);
		}	
	}
}
