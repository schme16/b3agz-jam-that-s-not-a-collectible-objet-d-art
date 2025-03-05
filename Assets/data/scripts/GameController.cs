using System.Collections.Generic;
using System.Threading.Tasks;
using CI.QuickSave;
using Cysharp.Threading.Tasks;
using Kamgam.UGUIBlurredBackground;
using StarterAssets;
using TMPro;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Yarn.Unity;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour {

	public Transform playerCamRoot;
	public GameObject paintingAPrefab;
	public GameObject paintingBPrefab;

	public DeskBellScript deskBell;
	public CinemachineRotationComposer rotationComposer;
	public FirstPersonController firstPersonController;
	public NPCArtSellerScript npcInConversation;
	public StarterAssetsInputs playerInputs;
	public CinemachineVirtualCameraBase cam;
	public GameObject npcPrefab;
	public bool inModelView;
	public bool inlastModelView;
	public bool readyToTalk;
	public bool inTalkTrigger;
	public bool inBellTrigger;
	public bool talking;
	public bool canTalk;
	private bool lastCanTalk;
	public bool canPressBell;
	private bool lastCanPressBell;


	public TextMeshProUGUI uiRegisterText;
	public AudioClip sfxRegisterChime;

	public GameObject blurCanvas;
	public BlurredBackgroundImage blurredBG;
	public Transform modelViewerHolder;
	public Transform modelViewerSpawnPointPortrait;
	public Transform modelViewerSpawnPointSquare;
	public ArtObjectScript modelViewerPainting;


	public Transform waypointCounter;
	public Transform waypointInsideDoor;
	public Transform waypointOutsideDoor;
	public Transform waypointLeave;

	public RectTransform uiPressEToTalk;
	public RectTransform uiPressEToPressBell;

	public DoorScript door;

	public GameObject[] ArtPrefabs;
	public AudioSource audioSource;
	public DialogueRunner dialogue;
	public VariableStorageBehaviour yarnStorage;
	public Names names;
	public ArtObjectScript[] portraitHangPoints;
	public ArtObjectScript[] squareHangPoints;
	public List<Art> collectedPortraitArt;
	public List<Art> collectedSquareArt;
	public Transform counterPaintingHolder;
	//public 

	public Texture[] hair;
	public Texture[] eyebrows;
	public Texture[] eyes;
	public Texture[] noses;
	public Texture[] mouths;




	public struct Names {
		public string[] first;
		public string[] middle;
		public string[] last;
	}

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


	async void Start() {


		uiRegisterText.SetText("$0.00");

		collectedPortraitArt = new List<Art>();
		collectedSquareArt = new List<Art>();

		LoadCollectedArtwork();


		names = JsonUtility.FromJson<Names>((Resources.Load("names") as TextAsset).text);
		SpawnNewNPC();


		//Hide the interaction text
		uiPressEToTalk.anchoredPosition = new Vector3(0, -25, 0);

		cam.LookAt = playerCamRoot;
		await UniTask.Delay(100);
	}

	// Update is called once per frame
	void Update() {

		canTalk = !talking && readyToTalk && inTalkTrigger;
		canPressBell = !talking && inBellTrigger;

		if (canTalk != lastCanTalk) {
			if (canTalk) {
				Translate(uiPressEToTalk, new Vector3(0, 20, 0), 5f, EasingFunction.Ease.EaseOutQuad);

			}
			else {
				Translate(uiPressEToTalk, new Vector3(0, -25, 0), 5f, EasingFunction.Ease.EaseOutQuad);
			}

		}

		if (canTalk && Input.GetKeyDown(KeyCode.E)) {
			talking = true;
			dialogue.StartDialogue("Start");
		}



		if (canPressBell != lastCanPressBell) {

			if (canPressBell) {
				Translate(uiPressEToPressBell, new Vector3(0, 20, 0), 5f, EasingFunction.Ease.EaseOutQuad);

			}
			else {
				Translate(uiPressEToPressBell, new Vector3(0, -25, 0), 5f, EasingFunction.Ease.EaseOutQuad);
			}
			lastCanPressBell = canPressBell;
		}

		if (canPressBell && Input.GetKeyDown(KeyCode.E)) {
			deskBell.RingBell();
		}







		playerInputs.enabled = !talking || !inModelView;
		playerInputs.cursorInputForLook = !talking || !inModelView;
		firstPersonController.enabled = !talking || !inModelView;

		if (talking || inModelView) {
			Cursor.lockState = CursorLockMode.Confined;
		}
		else {
			Cursor.lockState = CursorLockMode.Locked;
		}

		lastCanTalk = canTalk;


		if (Input.GetKeyDown(KeyCode.P)) {
			inModelView = !inModelView;
		}

		if (inModelView != inlastModelView) {


			if (inModelView) {

				ShowModelViewer(modelViewerPainting.transform);
			}

			else if (modelViewerPainting) {
				HideModelViewer(modelViewerPainting.transform);
			}


			inlastModelView = inModelView;
		}

	}

	public ArtObjectScript SpawnRandomPainting(Transform paintingHolder, Transform paintingSpawnPosition, string name = "painting") {

		//Generate a painting
		var painting = Instantiate(FlipCoin() ? paintingAPrefab : paintingBPrefab, paintingHolder).GetComponent<ArtObjectScript>();

		//Set its name for the animator
		painting.name = name;

		//Sync it to the spawn location values
		painting.transform.position = paintingSpawnPosition.position;
		painting.transform.rotation = paintingSpawnPosition.rotation;
		painting.transform.localScale = paintingSpawnPosition.localScale;

		return painting;
	}

	public void LoadCollectedArtwork() {
		var settings = new QuickSaveSettings {
			CompressionMode = CompressionMode.Gzip,
		};

		//Setting do exist, so read them
		if (QuickSaveReader.RootExists("Settings")) {

			//Read the settings
			var settingsReader = QuickSaveReader.Create("Settings", settings);
			settingsReader.TryRead<List<Art>>("collectedPortraitArt", out collectedPortraitArt);
			settingsReader.TryRead<List<Art>>("collectedSquareArt", out collectedSquareArt);

		}

		//Settings don't exist, create them	
		else {

			SaveCollectedArtwork();
		}

		if (collectedPortraitArt is null) {
			collectedPortraitArt = new List<Art>();
		}

		if (collectedSquareArt is null) {
			collectedSquareArt = new List<Art>();
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
		var settings = new QuickSaveSettings {
			CompressionMode = CompressionMode.Gzip,
		};

		var writer = QuickSaveWriter.Create("Settings", settings);
		writer.Write("collectedPortraitArt", collectedPortraitArt);
		writer.Write("collectedSquareArt", collectedSquareArt);

		Debug.Log($"collectedPortraitArt: {collectedPortraitArt.Count}");
		Debug.Log($"collectedSquareArt: {collectedSquareArt.Count}");
		writer.TryCommit();
	}

	public string CreateName() {

		try {
			var test = $"{names.first[Random.Range(0, names.first.Length)]}";
		}
		catch {
			names = JsonUtility.FromJson<Names>((Resources.Load("names") as TextAsset).text);

		}

		return $"{names.first[Random.Range(0, names.first.Length)]} {(FlipCoin() ? names.middle[Random.Range(0, names.middle.Length)] : "")} {names.last[Random.Range(0, names.last.Length)]}".Replace("  ", " ");
	}

	public bool FlipCoin() {
		bool heads = Random.Range(0, 2) == 0;
		return heads;
	}

	public void SpawnNewNPC() {
		Instantiate(npcPrefab);
	}

	public async void ShowModelViewer(Transform obj) {

		obj.localEulerAngles = new Vector3(0, 180, 0);

		var startScale = obj.localScale;
		if (obj.localScale.x > 0) {
			obj.GetComponent<ArtObjectScript>().initialScale = startScale;
		}
		else {
			startScale = obj.GetComponent<ArtObjectScript>().initialScale;

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
		if (obj.localScale.x > 0) {
			obj.GetComponent<ArtObjectScript>().initialScale = obj.localScale;
		}
		await Scale(obj, startScale + (startScale * 0.10f), 3.5f);
		await Scale(obj, Vector3.zero, 3f);


		await BlurBackground(false, 5f);

		blurCanvas.gameObject.SetActive(false);
	}

	public async void ReturnToDialogueOptions() {
		//await HideModelViewer(modelViewerPainting.transform);
		inModelView = false;
		await UniTask.Delay(1000);
		dialogue.StartDialogue($"Options{Random.Range(1, 4)}");
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

	private async Task<int> getHangerPoint(ArtObjectScript[] objects, List<Art> collectedArt) {

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


	public async UniTask Translate(Transform trans, Vector3 destination, float speed = 1.05f, EasingFunction.Ease easingFunction = EasingFunction.Ease.EaseInQuad) {


		float t = 0f;
		var startValue = trans.position;

		var easeing = new EasingFunction().GetEasingFunction(easingFunction);

		while (t < 1) {

			//Set the angle
			trans.position = Vector3.Lerp(startValue, destination, easeing(0, 1, t));

			//Update the time value
			t = Mathf.Clamp(t + (Time.deltaTime * speed), 0, 1);
			await UniTask.Yield(PlayerLoopTiming.Update);
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


		gc.readyToTalk = false;

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


		await UniTask.Delay(4000);
		gc.uiRegisterText.SetText($"$0.00");
	}


	[YarnCommand("storm_out")]
	public async static void StormOut() {
		var gc = FindFirstObjectByType<GameController>();
		Debug.Log("storm_out!");

		gc.readyToTalk = false;



		gc.cam.LookAt = gc.npcInConversation.cameraTarget;

		gc.npcInConversation.LeaveWithPainting();

		await UniTask.Delay(5000);

		gc.cam.LookAt = gc.playerCamRoot;

		// Reset player's camera root rotation
		gc.playerCamRoot.eulerAngles = new Vector3(0, gc.playerCamRoot.eulerAngles.y, gc.playerCamRoot.eulerAngles.z);

		await UniTask.Delay(500);


		gc.playerCamRoot.eulerAngles = new Vector3(0, gc.playerCamRoot.eulerAngles.y, gc.playerCamRoot.eulerAngles.z);
		gc.firstPersonController._cinemachineTargetPitch = 0;

		await UniTask.Delay(300);
		gc.rotationComposer.Damping = new Vector2(0, 0);



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
		gc.modelViewerPainting = hangSlot;
		gc.inModelView = true;


	}



}
