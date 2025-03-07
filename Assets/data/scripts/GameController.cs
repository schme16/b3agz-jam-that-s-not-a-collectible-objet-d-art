using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using data.scripts;
using Kamgam.UGUIBlurredBackground;
using StarterAssets;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Yarn.Unity;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour {

	public Transform playerCamRoot;
	public GameObject paintingAPrefab;
	public GameObject paintingBPrefab;
	public List<LedgerScript> ledgers;
	public AnsweringMachineScript answeringMachine;
	public SaveManagerScript SaveManager;

	public DeskBellScript deskBell;
	public CinemachineRotationComposer rotationComposer;
	public FirstPersonController firstPersonController;
	public NPCArtSellerScript npcInConversation;
	public StarterAssetsInputs playerInputs;
	public CinemachineVirtualCameraBase cam;
	public GameObject npcPrefab;
	[Range(0, 10f)]
	public float hangedPaintingHitDistance;
	public bool inModelView;
	public bool inlastModelView;
	public bool inTalkTrigger;
	public bool talking;
	public bool lastTalking;
	public Color highlightColor;
	public TMP_FontAsset accessibilityFont;





	[Header("Interactable stuff")]
	public InteractableScript interactScript;
	private InteractableScript lastInteractScript;
	private bool lastInteractScriptEnabled;
	public RectTransform uiPressEToPressBell;
	public Transform currentHitObject;
	public bool accessibilityEnabled;




	private Transform currentModel;
	public AudioClip sfxRegisterChime;

	public GameObject blurCanvas;
	public BlurredBackgroundImage blurredBG;
	public Transform modelViewerHolder;
	public Transform ledgerModelView;
	public Transform modelViewerSpawnPointPortrait;
	public Transform modelViewerSpawnPointSquare;
	public Transform modelViewerPainting;


	public Transform waypointCounter;
	public Transform waypointInsideDoor;
	public Transform waypointOutsideDoor;
	public Transform waypointLeave;

	public TextMeshProUGUI uiRegisterText;
	public GameObject uiBackToDialogue;
	public GameObject uiExitModelViewer;
	public RectTransform uiPressEToTalk;
	public GameObject uiReticule;
	public GameObject currentNPC;

	public DoorScript door;

	public GameObject[] ArtPrefabs;
	public AudioSource audioSource;
	public DialogueRunner dialogue;
	public VariableStorageBehaviour yarnStorage;
	public static Names names;
	public ArtObjectScript[] portraitHangPoints;
	public ArtObjectScript[] squareHangPoints;
	public List<Art> collectedPortraitArt;
	public List<Art> collectedSquareArt;
	public List<Sale> purchases;
	public Flags flags;
	public List<JournalEntryScript> journalEntries;
	public Transform counterPaintingHolder;
	public AudioClip sfxAnsweringMachineBip;
	public AudioClip sfxAnsweringMachineBeeep;
	public VoiceActingScript va;
	[FormerlySerializedAs("lockInptus")]
	public bool lockInputs;


	public Texture[] hair;
	public Texture[] eyebrows;
	public Texture[] eyes;
	public Texture[] noses;
	public Texture[] mouths;
	public GameObject[] hats;


	public struct Names {
		public string[] first;
		public string[] middle;
		public string[] last;
	}

	[Serializable]
	public struct Flags {

		//Bool to check if it's been loaded
		public bool hasBeenLoaded;

		public int numberOfFakes;

		public int numberOfStormOuts;

		public int numberOfPurchases;

		public bool customerHasWalkedOut;

		public bool hadGoodPurchase;

		public bool hadBadPurchase;

		public bool hadGoodSale;

		public bool accessibilityMode;

		public bool cleanFonts;

		//This shows the message was queued
		public bool hasQueued_vaIntro;
		public bool hasQueued_vaCustomerNotServed;
		public bool hasQueued_vaGreatDealOnPurchase;
		public bool hasQueued_vaGreatDealOnSale;
		public bool hasQueued_vaPaidTooMuchOverValue;
		public bool hasQueued_va5thCustomerWalkedOut;
		public bool hasQueued_vaTooManyFakes;
		public bool hasQueued_vaLJHookerLateRent;
		public bool hasQueued_vaAmazonScam;

		//This shows the message was played
		public bool hasPlayed_vaIntro;
		public bool hasPlayed_vaCustomerNotServed;
		public bool hasPlayed_vaGreatDealOnPurchase;
		public bool hasPlayed_vaGreatDealOnSale;
		public bool hasPlayed_vaPaidTooMuchOverValue;
		public bool hasPlayed_va5thCustomerWalkedOut;
		public bool hasPlayed_vaTooManyFakes;
		public bool hasPlayed_vaLJHookerLateRent;
		public bool hasPlayed_vaAmazonScam;
	}

	[Serializable]
	public struct Sale {
		public string type;
		public string npcsName;
		public string artistsName;
		public int salePrice;
		public int actualValue;
		public int profit;
		public bool isFake;
	}

	[Serializable]
	public struct Art {
		public string signatureName;
		public string artistsRealName;
		public string impersonatedArtistsName;
		public bool isSquare;
		public bool isFake;
		public bool isGoodFake;
		public int hangedPosition;
		public int actualValue;
		public int signatureLocation;
		public int signatureFont;
		public int frameOption;
		public int frameMaterial;
		public int whichArtwork;
	}

	public struct Npc {
		public string name;
		public int askingPrice;
		public int willAcceptPrice;
		public bool thinksItsFake;
		public bool willStormOut;
		public Art artPiece;
	}

	[Serializable]
	public struct AnsweringMachineMessage {
		public AudioClip audio;
		public UnityEvent OnPlay;
	}


	async void Start() {
		
		uiRegisterText.SetText("$0.00");

		collectedPortraitArt = new List<Art>();
		collectedSquareArt = new List<Art>();

		LoadCollectedArtwork();

		purchases = SaveManager.LoadPurchases();
		foreach (var ledger in ledgers) {
			ledger.Render();
		}


		flags = SaveManager.LoadFlags();

    	await UniTask.Delay(300);
		if (flags.cleanFonts) {
			var text = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			
			foreach (var textMeshProUGUI in text) {
				textMeshProUGUI.gameObject.AddComponent<FontBackup>();
				
				textMeshProUGUI.GetComponent<FontBackup>().originalFont = textMeshProUGUI.font;
				
				textMeshProUGUI.font = accessibilityFont;
			}
		}
		
		
		if (SaveManager.LoadMessages().Count == 0) {
			SpawnNewNPC();
		}

		names = JsonUtility.FromJson<Names>((Resources.Load("names") as TextAsset).text);


		//Hide the interaction text
		uiPressEToTalk.anchoredPosition = new Vector3(0, -25, 0);



		cam.LookAt = playerCamRoot;
		await UniTask.Delay(100);
	}

	// Update is called once per frame
	void Update() {


		if (interactScript != lastInteractScript || (interactScript is null && lastInteractScriptEnabled) || (interactScript is not null && interactScript.enabled != lastInteractScriptEnabled) || talking != lastTalking) {

			if (interactScript is not null && interactScript.enabled && !talking) {
				interactScript.inView = true;
				Translate(uiPressEToPressBell, new Vector3(0, 20, 0), 5f, EasingFunction.Ease.EaseOutQuad);

			}
			else {
				if (interactScript is not null) {
					interactScript.inView = false;
				}
				Translate(uiPressEToPressBell, new Vector3(0, -25, 0), 5f, EasingFunction.Ease.EaseOutQuad);
			}

			lastTalking = talking;
			lastInteractScript = interactScript;
			lastInteractScriptEnabled = interactScript?.enabled ?? false;
		}

		if (interactScript && Input.GetKeyDown(KeyCode.E)) {
			interactScript.Interact();
		}





		playerInputs.cursorInputForLook = !lockInputs && (!talking && !inModelView);
		firstPersonController.enabled = !lockInputs && (!talking && !inModelView);
		uiReticule.SetActive(firstPersonController.enabled);

		if (talking || inModelView) {
			Cursor.lockState = CursorLockMode.Confined;
		}
		else {
			Cursor.lockState = CursorLockMode.Locked;
		}

		if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hitInfo, hangedPaintingHitDistance)) {

			if (hitInfo.transform.CompareTag("Interactable")) {

				if (hitInfo.transform != currentHitObject) {

					currentHitObject = hitInfo.transform;
					var interact = currentHitObject.GetComponent<InteractableScript>();
					if (interact is not null && interact.enabled) {

						interactScript = interact;
						uiPressEToPressBell.GetComponent<TextMeshProUGUI>().SetText(interact.interactionText);
					}
					else {
						if (interactScript is not null) {
							interactScript.inView = false;
						}
						interactScript = null;
						currentHitObject = null;
					}
				}
			}
			else {
				if (interactScript is not null) {
					interactScript.inView = false;
				}
				interactScript = null;
				currentHitObject = null;
			}

		}
		else {
			if (interactScript is not null) {
				interactScript.inView = false;
			}
			interactScript = null;
			currentHitObject = null;
		}






		if (inModelView != inlastModelView) {


			if (inModelView) {
				lockInputs = true;
				ShowModelViewer(modelViewerPainting);
			}

			else if (modelViewerPainting) {
				HideModelViewer(modelViewerPainting);
			}


			inlastModelView = inModelView;
		}

	}

	public ArtObjectScript SpawnRandomPainting(Transform paintingHolder, Transform paintingSpawnPosition, string name = "painting") {

		//Generate a painting
		var painting = Instantiate(FlipCoin() ? paintingAPrefab : paintingBPrefab, paintingHolder).GetComponent<ArtObjectScript>();

		//Set its name for the animator
		painting.name = name;

		painting.PresetValues = false;
		painting.PickRandomValues();
		painting.Render();

		//Sync it to the spawn location values
		painting.transform.position = paintingSpawnPosition.position;
		painting.transform.rotation = paintingSpawnPosition.rotation;
		painting.transform.localScale = paintingSpawnPosition.localScale;

		return painting;
	}

	public void LoadCollectedArtwork() {

		collectedPortraitArt = SaveManager.LoadArtList("collectedPortraitArt");
		collectedSquareArt = SaveManager.LoadArtList("collectedSquareArt");

		if (collectedPortraitArt is null) {
			collectedPortraitArt = new List<Art>();
			SaveManager.SaveArtList(collectedPortraitArt, "collectedSquareArt");
		}

		if (collectedSquareArt is null) {
			collectedSquareArt = new List<Art>();
			SaveManager.SaveArtList(collectedSquareArt, "collectedSquareArt");
		}



		foreach (var art in collectedPortraitArt) {
			portraitHangPoints[art.hangedPosition].gameObject.SetActive(true);
			portraitHangPoints[art.hangedPosition].LoadSavedArtwork(art);
		}


		foreach (var art in collectedSquareArt) {
			squareHangPoints[art.hangedPosition].gameObject.SetActive(true);
			squareHangPoints[art.hangedPosition].LoadSavedArtwork(art);
		}



	}

	public void SaveCollectedArtwork() {
		SaveManager.SaveArtList(collectedPortraitArt, "collectedPortraitArt");
		SaveManager.SaveArtList(collectedSquareArt, "collectedSquareArt");
	}

	public static string CreateName() {
		try {
			var test = $"{names.first[Random.Range(0, GameController.names.first.Length)]}";
		}
		catch {
			names = JsonUtility.FromJson<Names>((Resources.Load("names") as TextAsset).text);

		}

		return $"{names.first[Random.Range(0, names.first.Length)]} {(FlipCoin() ? names.middle[Random.Range(0, names.middle.Length)] : "")} {names.last[Random.Range(0, names.last.Length)]}".Replace("  ", " ");
	}


	public static bool FlipCoin() {
		bool heads = Random.Range(0, 2) == 0;
		return heads;
	}

	public void SpawnNewNPC() {
		if (currentNPC == null) {
			currentNPC = Instantiate(npcPrefab);
		}
	}

	public async void ShowModelViewer(Transform obj) {

		currentModel = obj;



		var startScale = obj.localScale;
		var art = obj.GetComponent<ArtObjectScript>();

		var rotator = obj.GetComponent<ObjectManipulation>();
		if (rotator is not null) {

			obj.localEulerAngles = rotator.startRotation;

			if (obj.localScale.x > 0) {
				rotator.startScale = startScale;
			}
			else {
				startScale = rotator.startScale;
			}
		}

		if (art is not null && art.artValues.hangedPosition == -1) {
			uiBackToDialogue.SetActive(true);
			uiExitModelViewer.SetActive(false);
		}
		else {
			uiBackToDialogue.SetActive(false);
			uiExitModelViewer.SetActive(true);
		}

		obj.localScale = Vector3.zero;

		obj.gameObject.SetActive(true);


		blurCanvas.gameObject.SetActive(true);
		await BlurBackground(true, 5f);

		await Scale(obj, startScale + (startScale * 0.1f), 3.5f);
		await Scale(obj, startScale, 3f);

	}

	public async UniTask HideModelViewer(Transform obj) {

		var startScale = obj.localScale;

		var art = obj.GetComponent<ArtObjectScript>();
		if (art is not null) {

			if (obj.localScale.x > 0) {
				obj.GetComponent<ArtObjectScript>().initialScale = obj.localScale;
			}
		}

		await Scale(obj, startScale + (startScale * 0.10f), 3.5f);
		await Scale(obj, Vector3.zero, 3f);


		await BlurBackground(false, 5f);
		lockInputs = false;
		blurCanvas.gameObject.SetActive(false);
		currentModel = null;

	}

	public void HideModelViewer() {
		inModelView = false;
		//HideModelViewer(currentModel);
	}

	public async void ReturnToDialogueOptions() {
		//await HideModelViewer(modelViewerPainting.transform);
		inModelView = false;
		await UniTask.Delay(1000);
		dialogue.StartDialogue($"Options{Random.Range(1, 4)}");
	}

	public void StartConversationWithNPC() {
		talking = true;
		dialogue.StartDialogue("Start");
	}

	public async void ViewPaintingFromInteract() {
		var gc = FindFirstObjectByType<GameController>();

		if (currentHitObject is not null) {

			var art = currentHitObject.GetComponent<ArtObjectScript>();

			if (art is not null) {


				var artValues = art.artValues;
				ArtObjectScript hangSlot;
				if (artValues.isSquare || art.isSquare) {
					hangSlot = gc.modelViewerSpawnPointSquare.GetComponent<ArtObjectScript>();
				}
				else {
					hangSlot = gc.modelViewerSpawnPointPortrait.GetComponent<ArtObjectScript>();
				}

				hangSlot.LoadSavedArtwork(artValues);
				gc.modelViewerPainting = hangSlot.transform;
				gc.inModelView = true;
			}
		}


	}

	public async void ViewLedgerFromInteract() {
		var gc = FindFirstObjectByType<GameController>();


		gc.modelViewerPainting = ledgerModelView;
		gc.inModelView = true;


	}

	public void FlagsCheck() {

		var newFlags = flags;
		var alreadyFlagged = false;

		//Check if we need to trigger the storm out flag
		if (newFlags.numberOfStormOuts > 4 && !newFlags.hasQueued_va5thCustomerWalkedOut) {
			alreadyFlagged = true;
			newFlags.hasQueued_va5thCustomerWalkedOut = true;

			answeringMachine.pendingMessages.Add(5);
			SaveManager.SaveMessages(answeringMachine.pendingMessages);
		}



		//Check if we need to trigger the too many fakes trigger
		if (!alreadyFlagged && newFlags.numberOfFakes > 4 && !newFlags.hasQueued_vaTooManyFakes) {
			alreadyFlagged = true;

			newFlags.hasQueued_vaTooManyFakes = true;

			answeringMachine.pendingMessages.Add(8);
			SaveManager.SaveMessages(answeringMachine.pendingMessages);
		}



		//Check if we need to trigger the customer left in a huff trigger
		if (!alreadyFlagged && newFlags.customerHasWalkedOut && !newFlags.hasQueued_vaCustomerNotServed) {
			alreadyFlagged = true;

			newFlags.hasQueued_vaCustomerNotServed = true;

			answeringMachine.pendingMessages.Add(1);
			SaveManager.SaveMessages(answeringMachine.pendingMessages);
		}



		//Check if we should trigger the amazon scam
		if (!alreadyFlagged && newFlags.hadGoodPurchase && !newFlags.hasQueued_vaGreatDealOnPurchase) {
			alreadyFlagged = true;

			newFlags.hasQueued_vaGreatDealOnPurchase = true;

			answeringMachine.pendingMessages.Add(2);
			SaveManager.SaveMessages(answeringMachine.pendingMessages);
		}



		//Check if we should trigger the amazon scam
		if (!alreadyFlagged && newFlags.hadGoodSale && !newFlags.hasQueued_vaGreatDealOnSale) {
			alreadyFlagged = true;

			newFlags.hasQueued_vaGreatDealOnSale = true;

			answeringMachine.pendingMessages.Add(3);
			SaveManager.SaveMessages(answeringMachine.pendingMessages);
		}



		//Check if we should trigger the amazon scam
		if (!alreadyFlagged && newFlags.hadBadPurchase && !newFlags.hasQueued_vaPaidTooMuchOverValue) {
			alreadyFlagged = true;

			newFlags.hasQueued_vaPaidTooMuchOverValue = true;

			answeringMachine.pendingMessages.Add(4);
			SaveManager.SaveMessages(answeringMachine.pendingMessages);
		}




		//Check if we should trigger the amazon scam
		if (!alreadyFlagged && Random.Range(0, 11) == 9 && !newFlags.hasQueued_vaAmazonScam) {
			alreadyFlagged = true;

			newFlags.hasQueued_vaAmazonScam = true;

			answeringMachine.pendingMessages.Add(7);
			SaveManager.SaveMessages(answeringMachine.pendingMessages);
		}



		//Check if we should trigger the LJ hooker call
		if (!alreadyFlagged && Random.Range(0, 20) == 17 && !newFlags.hasQueued_vaLJHookerLateRent) {
			alreadyFlagged = true;

			newFlags.hasQueued_vaLJHookerLateRent = true;

			answeringMachine.pendingMessages.Add(6);
			answeringMachine.pendingMessages.Add(9);
			SaveManager.SaveMessages(answeringMachine.pendingMessages);
		}





		flags = newFlags;
		SaveManager.SaveFlags(newFlags);
	}





	public async UniTask BlurBackground(bool shouldBlur, float speed = 1.05f, EasingFunction.Ease easingFunction = EasingFunction.Ease.EaseInQuad, float blurAmount = 20) {

		var t = 0f;
		var startValue = shouldBlur ? 0 : blurAmount;
		var endValue = shouldBlur ? blurAmount : 0;
		var startColourValue = shouldBlur ? new Color(255, 255, 255, 0) : new Color(255, 255, 255, 120);
		var endColourValue = shouldBlur ? new Color(255, 255, 255, 120) : new Color(255, 255, 255, 0);
		var easeing = new EasingFunction().GetEasingFunction(easingFunction);

		blurredBG.Strength = startValue;
		blurredBG.color = startColourValue;
		while (t < 1) {

			//Set the angle
			blurredBG.Strength = Mathf.Lerp(startValue, endValue, easeing(0, 1, t));
			blurredBG.color = Color.Lerp(startColourValue, endColourValue, easeing(0, 1, t));

			//Update the time value
			t = Mathf.Clamp(t + (Time.deltaTime * speed), 0, 1);
			await UniTask.Yield(PlayerLoopTiming.Update);
		}

		blurredBG.Strength = endValue;
	}

	private async Task<int> getHangerPoint(ArtObjectScript[] objects, List<GameController.Art> collectedArt) {

		//These are points that are valid to pick from
		var validPoints = new List<ArtObjectScript>();

		//These are points that are already taken
		var invalidPoints = new List<int> { };

		//For each previously collected art piece, add it's position to the do not pick list
		foreach (var collected in collectedArt) {
			invalidPoints.Add(collected.hangedPosition);
		}

		//For each of the hanging points
		for (var i = 0; i < objects.Length; i++) {

			//If it isn't in the invalid list
			if (!invalidPoints.Contains(i)) {

				//Add it to the valid list
				validPoints.Add(objects[i]);
			}
		}

		if (validPoints.Count == 0) {
			return -1;
		}
		else {
			return Random.Range(0, validPoints.Count);
		}
	}


	public async UniTask Rotate(Transform trans, Vector3 destination, float speed = 1.05f, EasingFunction.Ease easingFunction = EasingFunction.Ease.EaseInQuad) {

		float t = 0f;
		var startValue = trans.eulerAngles;
		var easeing = new EasingFunction().GetEasingFunction(easingFunction);

		while (t < 1) {

			//Set the angle
			trans.eulerAngles = Vector3.Lerp(startValue, destination, easeing(0, 1, t));

			//Update the time value
			t = Mathf.Clamp(t + (Time.deltaTime * speed), 0, 1);
			await UniTask.Yield(PlayerLoopTiming.Update);
		}
		trans.eulerAngles = destination;
	}


	public async UniTask Scale(Transform trans, Vector3 destination, float speed = 1.05f, EasingFunction.Ease easingFunction = EasingFunction.Ease.EaseInQuad) {

		float t = 0f;
		var startValue = trans.localScale;
		var easeing = new EasingFunction().GetEasingFunction(easingFunction);

		while (t < 1) {

			//Set the angle
			trans.localScale = Vector3.Lerp(startValue, destination, easeing(0, 1, t));

			//Update the time value
			t = Mathf.Clamp(t + (Time.deltaTime * speed), 0, 1);
			await UniTask.Yield(PlayerLoopTiming.Update);
		}

		trans.localScale = destination;
	}


	public async UniTask Translate(Transform trans, Vector3 destination, float speed = 1.05f, EasingFunction.Ease easingFunction = EasingFunction.Ease.EaseInQuad, CancellationToken cancellationToken = default) {


		float t = 0f;
		var startValue = trans.position;

		var easeing = new EasingFunction().GetEasingFunction(easingFunction);

		while (t < 1) {

			//Set the angle
			trans.position = Vector3.Lerp(startValue, destination, easeing(0, 1, t));

			//Update the time value
			t = Mathf.Clamp(t + (Time.deltaTime * speed), 0, 1);
			await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
		}

		trans.position = destination;
	}



	public async UniTask Translate(RectTransform trans, Vector3 destination, float speed = 1.05f, EasingFunction.Ease easingFunction = EasingFunction.Ease.EaseInQuad) {


		float t = 0f;
		var startValue = trans.anchoredPosition;

		var easeing = new EasingFunction().GetEasingFunction(easingFunction);

		while (t < 1) {

			//Set the angle
			trans.anchoredPosition = Vector3.Lerp(startValue, destination, easeing(0, 1, t));

			//Update the time value
			t = Mathf.Clamp(t + (Time.deltaTime * speed), 0, 1);
			await UniTask.Yield(PlayerLoopTiming.Update);
		}

		trans.anchoredPosition = destination;
	}









	[YarnCommand("purchase")]
	public async static void Purchase() {
		var gc = FindFirstObjectByType<GameController>();
		gc.yarnStorage.TryGetValue<float>($"$askingPrice", out var purchasePrice);
		Debug.Log($"Purchased for {purchasePrice}!");

		gc.uiRegisterText.SetText($"${purchasePrice}.00");

		gc.audioSource.PlayOneShot(gc.sfxRegisterChime);


		gc.npcInConversation.Leave();
		var artValues = gc.npcInConversation.painting.artValues;
		await gc.Scale(gc.npcInConversation.painting.transform, gc.npcInConversation.painting.transform.localScale + (gc.npcInConversation.painting.transform.localScale * 0.1f), 5.5f);
		await gc.Scale(gc.npcInConversation.painting.transform, Vector3.zero, 5.01f);

		ArtObjectScript hangSlot;

		if (artValues.isSquare) {
			artValues.hangedPosition = await gc.getHangerPoint(gc.squareHangPoints, gc.collectedSquareArt);
			hangSlot = gc.squareHangPoints[artValues.hangedPosition];

			gc.collectedSquareArt.Add(artValues);
		}
		else {
			artValues.hangedPosition = await gc.getHangerPoint(gc.portraitHangPoints, gc.collectedPortraitArt);
			hangSlot = gc.portraitHangPoints[artValues.hangedPosition];

			gc.collectedPortraitArt.Add(artValues);
		}

		gc.SaveCollectedArtwork();

		var sale = new Sale {
			type = "BUY",
			npcsName = gc.npcInConversation.npc.name,
			artistsName = artValues.artistsRealName,
			salePrice = (int)purchasePrice,
			actualValue = artValues.actualValue,
			profit = (int)(artValues.actualValue - purchasePrice),
			isFake = artValues.isFake,
		};

		//Add the pruchase to the purchase history
		gc.purchases.Add(sale);

		//Update the ledgers
		foreach (var ledger in gc.ledgers) {
			ledger.Render();
		}

		Debug.Log($"Proifit: {sale.profit} (actual value: {sale.actualValue} - purchase price: {sale.salePrice})");

		gc.SaveManager.SavePurchases(gc.purchases);


		//Update the flags
		var flags = gc.flags;

		flags.numberOfPurchases++;

		if (artValues.isFake) {
			flags.numberOfFakes++;
		}

		if (sale.profit > 150) {
			flags.hadGoodPurchase = true;
		}

		if (sale.profit < -70) {
			flags.hadBadPurchase = true;
		}



		gc.flags = flags;
		gc.SaveManager.SaveFlags(gc.flags);
		gc.FlagsCheck();





		var startScale = hangSlot.transform.localScale;
		hangSlot.transform.localScale = Vector3.zero;
		hangSlot.gameObject.SetActive(true);
		hangSlot.LoadSavedArtwork(artValues);

		gc.cam.LookAt = hangSlot.transform;
		await UniTask.Delay(1000);

		await gc.Scale(hangSlot.transform, startScale + (startScale * 0.25f), 3.5f);
		await gc.Scale(hangSlot.transform, startScale, 3.01f);


		Destroy(gc.npcInConversation.painting);

		await UniTask.Delay(1000);



		gc.cam.LookAt = gc.playerCamRoot;

		// Reset player's camera root rotation
		gc.playerCamRoot.eulerAngles = new Vector3(0, gc.playerCamRoot.eulerAngles.y, gc.playerCamRoot.eulerAngles.z);

		await UniTask.Delay(500);


		gc.playerCamRoot.eulerAngles = new Vector3(0, gc.playerCamRoot.eulerAngles.y, gc.playerCamRoot.eulerAngles.z);
		gc.firstPersonController._cinemachineTargetPitch = 0;

		await UniTask.Delay(300);
		gc.rotationComposer.Damping = new Vector2(0, 0);

		gc.talking = false;


		await UniTask.Delay(7000);
		gc.uiRegisterText.SetText($"$0.00");
	}


	[YarnCommand("storm_out")]
	public async static void StormOut() {
		var gc = FindFirstObjectByType<GameController>();
		Debug.Log("storm_out!");




		gc.cam.LookAt = gc.npcInConversation.cameraTarget;

		gc.npcInConversation.LeaveWithPainting();

		await UniTask.Delay(3000);

		gc.cam.LookAt = gc.playerCamRoot;

		// Reset player's camera root rotation
		gc.playerCamRoot.eulerAngles = new Vector3(0, gc.playerCamRoot.eulerAngles.y, gc.playerCamRoot.eulerAngles.z);

		await UniTask.Delay(500);


		gc.playerCamRoot.eulerAngles = new Vector3(0, gc.playerCamRoot.eulerAngles.y, gc.playerCamRoot.eulerAngles.z);
		gc.firstPersonController._cinemachineTargetPitch = 0;

		await UniTask.Delay(300);
		gc.rotationComposer.Damping = new Vector2(0, 0);

		gc.talking = false;

		gc.interactScript = null;


		//Update the flags
		var flags = gc.flags;

		flags.numberOfStormOuts++;

		gc.flags = flags;
		gc.SaveManager.SaveFlags(gc.flags);
		gc.FlagsCheck();

	}


	[YarnCommand("lookat")]
	public async static void LookAt(GameObject obj) {
		var gc = FindFirstObjectByType<GameController>();

		gc.playerCamRoot.eulerAngles = new Vector3(0, gc.playerCamRoot.eulerAngles.y, gc.playerCamRoot.eulerAngles.z);
		gc.rotationComposer.Damping = new Vector2(0.5f, 0.5f);

		gc.cam.LookAt = obj.transform;

	}


	[YarnCommand("viewpainting")]
	public async static void ViewPainting() {
		var gc = FindFirstObjectByType<GameController>();


		var artValues = gc.npcInConversation.painting.artValues;
		ArtObjectScript hangSlot;
		if (artValues.isSquare || gc.npcInConversation.painting.isSquare) {
			hangSlot = gc.modelViewerSpawnPointSquare.GetComponent<ArtObjectScript>();
		}
		else {
			hangSlot = gc.modelViewerSpawnPointPortrait.GetComponent<ArtObjectScript>();
		}


		hangSlot.LoadSavedArtwork(artValues);
		gc.modelViewerPainting = hangSlot.transform;
		gc.inModelView = true;


	}


}
