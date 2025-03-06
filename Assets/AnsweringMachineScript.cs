using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class AnsweringMachineScript : MonoBehaviour {

	public MeshRenderer button;
	public TextMeshPro numberMessages;
	public List<GameController.AnsweringMachineMessage> pendingMessages;
	public List<GameController.AnsweringMachineMessage> allMessages;
	public AudioSource audioSource;

	public int lastNumMessages;
	public float blinkTimer;
	public float blinkDuration;
	public InteractableScript interactionScript;
	public bool direction;
	public Color startColour = new Color(255, 0, 0, 0);
	public Color endColour = Color.red;
	public Color colour;
	private GameController gc;



	void Start() {

		//Get the game controller
		gc = FindFirstObjectByType<GameController>();


		pendingMessages = new List<GameController.AnsweringMachineMessage>();
		allMessages = new List<GameController.AnsweringMachineMessage>();

		pendingMessages.Add(new GameController.AnsweringMachineMessage {
			audio = gc.va.vaTest,
		});

	}


	// Update is called once per frame
	void Update() {

		interactionScript.enabled = lastNumMessages > 0 && !audioSource.isPlaying;

		if (lastNumMessages != pendingMessages.Count) {
			lastNumMessages = pendingMessages.Count;
			numberMessages.SetText($"{pendingMessages.Count}");
		}

		if (lastNumMessages > 0 && !audioSource.isPlaying) {

			if (!direction) {
				blinkTimer += Time.deltaTime;
			}
			else {
				blinkTimer -= Time.deltaTime;
			}

			if (blinkTimer < 0) {
				direction = false;
				blinkTimer = 0;
				colour = startColour;
			}

			if (blinkTimer > blinkDuration) {
				direction = true;
				blinkTimer = blinkDuration;
				colour = endColour;
			}


			if (audioSource.isPlaying) {
				colour = endColour;
			}
			

		}
		else if (lastNumMessages == 0) {
			colour = startColour;
			blinkTimer = 0;
			direction = false;
		}
		
		button.material.SetColor("_BaseColor", colour);
	}

	public async void PlayAllMessages() {

		await PlayMessage(new GameController.AnsweringMachineMessage {
			audio = gc.sfxAnsweringMachineBip,
		});

		foreach (var message in pendingMessages) {

			await PlayMessage(message);


			if (pendingMessages.Count > 0) {
				await PlayMessage(new GameController.AnsweringMachineMessage {
					audio = gc.sfxAnsweringMachineBip,
				});
			}

		}
		pendingMessages.Clear();


		await PlayMessage(new GameController.AnsweringMachineMessage {
			audio = gc.sfxAnsweringMachineBeeep,
		});

	}

	private async UniTask PlayMessage(GameController.AnsweringMachineMessage message) {
		audioSource.clip = message.audio;

		audioSource.Play();

		while (audioSource.isPlaying) {
			await UniTask.Yield(PlayerLoopTiming.Update);
		}

	}
}
