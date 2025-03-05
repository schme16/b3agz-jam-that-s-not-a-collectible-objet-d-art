using System;
using UnityEngine;

public class TalkTrigger : MonoBehaviour {


	private GameController gc;

	private void Start() {
		//Get the game controller
		gc = FindFirstObjectByType<GameController>();
	}

	private void OnTriggerEnter(Collider other) {
		gc.inTalkTrigger = true;
	}

	private void OnTriggerExit(Collider other) {
		gc.inTalkTrigger = false;
	}
}
