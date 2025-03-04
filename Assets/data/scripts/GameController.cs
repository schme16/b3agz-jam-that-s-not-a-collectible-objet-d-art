using System.Collections.Generic;
using System.Threading.Tasks;
using CI.QuickSave;
using Cysharp.Threading.Tasks;
using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;
using Yarn.Unity;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour {

	public Transform playerCamRoot;
	public FirstPersonController firstPersonController;
	public NPCArtSellerScript npcInConversation;
	public StarterAssetsInputs playerInputs;
	public GameObject npcPrefab;
	public bool readyToTalk;
	public bool inTalkTrigger;
	public bool talking;
	public bool canInteract;
	private bool lastCanInteract;

	public Transform waypointCounter;
	public Transform waypointInsideDoor;
	public Transform waypointOutsideDoor;
	public Transform waypointLeave;

	public GameObject uiPressEToTalk;

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



	void Start() {

		collectedPortraitArt = new List<Art>();
		collectedSquareArt = new List<Art>();

		LoadCollectedArtwork();


		names = JsonUtility.FromJson<Names>((Resources.Load("names") as TextAsset).text);
		SpawnNewNPC();

		//Hide the interaction text
		uiPressEToTalk.transform.position = (uiPressEToTalk.transform.position + new Vector3(0, -7.5f, 0));
	}

	// Update is called once per frame
	void Update() {

		canInteract = !talking && readyToTalk && inTalkTrigger;

		if (canInteract != lastCanInteract) {
			if (canInteract) {
				//uiPressEToTalk.SetActive(true);
				Translate(uiPressEToTalk.transform, uiPressEToTalk.transform.position + new Vector3(0, 7.5f, 0), 8f, EasingFunction.Ease.EaseOutQuad);

			}
			else {
				//uiPressEToTalk.SetActive(false);
				Translate(uiPressEToTalk.transform, uiPressEToTalk.transform.position + new Vector3(0, -7.5f, 0), 8f, EasingFunction.Ease.EaseOutQuad);

			}

		}

		if (uiPressEToTalk.activeSelf && Input.GetKeyDown(KeyCode.E)) {
			talking = true;
			dialogue.StartDialogue("Start");
		}

		playerInputs.enabled = !talking;
		playerInputs.cursorInputForLook = !talking;
		firstPersonController.enabled = !talking;

		if (talking) {
			Cursor.lockState = CursorLockMode.Confined;
		}
		else {
			Cursor.lockState = CursorLockMode.Locked;
		}

		lastCanInteract = canInteract;

	}

	public void LoadCollectedArtwork() {


		//Setting do exist, so read them
		if (QuickSaveReader.RootExists("Settings")) {

			//Read the settings
			var settingsReader = QuickSaveReader.Create("Settings");
			settingsReader.TryRead<List<Art>>("collectedPortraitArt", out collectedPortraitArt);
			settingsReader.TryRead<List<Art>>("collectedSquareArt", out collectedSquareArt);

		}

		//Settings don't exists, create them	
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
			Debug.Log(1111);
			squareHangPoints[art.hangedPosition].gameObject.SetActive(true);
			squareHangPoints[art.hangedPosition].LoadSavedArtwork(art);
		}




	}

	public void SaveCollectedArtwork() {

		var writer = QuickSaveWriter.Create("Settings");
		writer.Write("collectedPortraitArt", collectedPortraitArt);
		writer.Write("collectedSquareArt", collectedSquareArt);
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

	private void OnTriggerEnter(Collider other) {
		inTalkTrigger = true;
	}

	private void OnTriggerExit(Collider other) {
		inTalkTrigger = false;
	}

	public void SpawnNewNPC() {
		Instantiate(npcPrefab);
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






	[YarnCommand("purchase")]
	public async static void Purchase() {
		var gc = FindFirstObjectByType<GameController>();
		gc.yarnStorage.TryGetValue<float>($"$askingPrice", out var purchasePrice);
		Debug.Log($"Purchased for {purchasePrice}!");
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
		}

		gc.SaveCollectedArtwork();


		var startScale = hangSlot.transform.localScale;
		hangSlot.transform.localScale = Vector3.zero;
		hangSlot.gameObject.SetActive(true);
		hangSlot.LoadSavedArtwork(artValues);

		await UniTask.Delay(1000);
		await gc.Scale(hangSlot.transform, startScale + (gc.npcInConversation.painting.transform.localScale * 0.1f), 5.5f);
		await gc.Scale(hangSlot.transform, startScale, 5.01f);

		Destroy(gc.npcInConversation.painting);

	}

	[YarnCommand("storm_out")]
	public static void StormOut() {
		var gc = FindFirstObjectByType<GameController>();
		Debug.Log("storm_out!");

		gc.npcInConversation.LeaveWithPainting();
	}


}
