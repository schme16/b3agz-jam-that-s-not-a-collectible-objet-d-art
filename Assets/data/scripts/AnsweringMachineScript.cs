using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class AnsweringMachineScript : MonoBehaviour {

	public MeshRenderer button;
	public TextMeshPro numberMessages;
	
	public List<int> pendingMessages;
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

		
		//Add the intro 0
		allMessages.Add(new GameController.AnsweringMachineMessage {
			audio = gc.va.vaIntro,
			OnPlay = new UnityEvent(),
		});
		allMessages[^1].OnPlay.AddListener(() => {
			gc.SpawnNewNPC();
			
			var flags = gc.flags;
			flags.hasPlayed_vaIntro = true;
			gc.flags = flags;
			gc.SaveManager.SaveFlags(gc.flags);
			
		});
		
		
		//Add the vaCustomerNotServed 1
		allMessages.Add(new GameController.AnsweringMachineMessage {
			audio = gc.va.vaCustomerNotServed,
			OnPlay = new UnityEvent(),
		});
		
		
		//Add the vaGreatDealOnPurchase 2
		allMessages.Add(new GameController.AnsweringMachineMessage {
			audio = gc.va.vaGreatDealOnPurchase,
			OnPlay = new UnityEvent(),
		});
		
		
		//Add the vaGreatDealOnSale 3
		allMessages.Add(new GameController.AnsweringMachineMessage {
			audio = gc.va.vaGreatDealOnSale,
			OnPlay = new UnityEvent(),
		});
		
		
		//Add the vaPaidTooMuchOverValue 4
		allMessages.Add(new GameController.AnsweringMachineMessage {
			audio = gc.va.vaPaidTooMuchOverValue,
			OnPlay = new UnityEvent(),
		});
		
		
		//Add the va5thCustomerWalkedOut 5
		allMessages.Add(new GameController.AnsweringMachineMessage {
			audio = gc.va.va5thCustomerWalkedOut,
			OnPlay = new UnityEvent(),
		});
		
		
		//Add the vaLJHookerLateRent 6
		allMessages.Add(new GameController.AnsweringMachineMessage {
			audio = gc.va.vaLJHookerLateRent,
			OnPlay = new UnityEvent(),
		});
		
		
		//Add the vaAmazonScam 7
		allMessages.Add(new GameController.AnsweringMachineMessage {
			audio = gc.va.vaAmazonScam,
			OnPlay = new UnityEvent(),
		});
		
		
		//Add the vaTooManyFakes 8
		allMessages.Add(new GameController.AnsweringMachineMessage {
			audio = gc.va.vaTooManyFakes,
			OnPlay = new UnityEvent(),
		});
		
		
		//Add the vaLJHookerLateRentResponse 9
		allMessages.Add(new GameController.AnsweringMachineMessage {
			audio = gc.va.vaLJHookerLateRentResponse,
			OnPlay = new UnityEvent(),
		});
		
		
		
		
		
		
		
		
		
		
		
		

		//pendingMessages = new List<GameController.AnsweringMachineMessage>();
		pendingMessages = gc.SaveManager.LoadMessages();

		
		//Has the intro been queued before?
		if (!gc.flags.hasQueued_vaIntro) {
			
			//No? Queue it
			pendingMessages.Add(0);
			
			gc.SaveManager.SaveMessages(pendingMessages);

			var flags = gc.flags;
			flags.hasQueued_vaIntro = true;
			gc.flags = flags;
			gc.SaveManager.SaveFlags(gc.flags);
		}
		

		gc.SaveManager.SaveMessages(pendingMessages);

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




		}
		else if (lastNumMessages == 0) {
			colour = startColour;
			blinkTimer = 0;
			direction = false;
		}
		
		if (audioSource.isPlaying) {
			colour = endColour;
		}

		button.material.SetColor("_BaseColor", colour);
	}

	public async void PlayAllMessages() {

		await PlayMessage(new GameController.AnsweringMachineMessage {
			audio = gc.sfxAnsweringMachineBip,
		});

		foreach (var message in pendingMessages) {

			await PlayMessage(allMessages[message]);


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

		if (gc.currentNPC is null || gc.currentNPC == null) {
			
			await UniTask.Delay(Random.Range(1000, 10000));
			gc.SpawnNewNPC();
		}
	}

	private async UniTask PlayMessage(GameController.AnsweringMachineMessage message) {
		audioSource.clip = message.audio;

		audioSource.Play();

		while (audioSource.isPlaying) {
			await UniTask.Yield(PlayerLoopTiming.Update);
		}

        if (message.OnPlay is not null) {
			message.OnPlay.Invoke();
		}
		
		gc.SaveManager.SaveMessages(pendingMessages);
		
	}
	
	
	
}
