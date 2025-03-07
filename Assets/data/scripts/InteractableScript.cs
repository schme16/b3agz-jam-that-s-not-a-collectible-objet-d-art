using System;
using UnityEngine;
using UnityEngine.Events;

public class InteractableScript : MonoBehaviour {

	public string interactionText;
	public UnityEvent OnInteract;
	public bool inView;
	public Outline outline;
	private GameController gc;



	private void Start() {

		//Get the game controller
		gc = FindFirstObjectByType<GameController>();


		gameObject.AddComponent<Outline>();
		outline = GetComponent<Outline>();
		outline.OutlineColor = new Color(255, 147, 0, 255);
		outline.OutlineWidth = 3;

	}

	private void Update() {
		if (outline is not null) {
			outline.enabled = gc is not null && gc.flags.accessibilityMode && inView;
		}
	}

	public void Interact() {

		//Was an interaction function set?
		if (OnInteract is not null) {

			//Run the func
			OnInteract.Invoke();
		}
	}

	private void OnDisable() {
		inView = false;
		if (outline is not null) {
			outline.enabled = false;
		}
	}
}
