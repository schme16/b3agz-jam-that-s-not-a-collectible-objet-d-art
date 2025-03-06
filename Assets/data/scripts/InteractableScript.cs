using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class InteractableScript : MonoBehaviour {

	public string interactionText;
	public UnityEvent OnInteract;
	private GameController gc;



	private void Start() {

		//Get the game controller
		gc = FindFirstObjectByType<GameController>();


	}

	public void Interact() {

		//Was an interaction function set?
		if (OnInteract is not null) {

			//Run the func
			OnInteract.Invoke();
		}
	}
}
