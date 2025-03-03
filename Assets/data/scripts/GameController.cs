using UnityEngine;
using Yarn.Unity;

public class GameController : MonoBehaviour {

	public DoorScript door;

	public GameObject[] ArtPrefabs;
	public AudioSource audioSource;
	public VariableStorageBehaviour yarnStorage;
	public Names names;
	public ArtObjectScript[] portraitHangPoints;
	public ArtObjectScript[] squareHangPoints;
	public Art[] collectedArt;
	public Transform counterPaintingHolder;
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
		public bool isFake;
		public bool isGoodFake;
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
		names = JsonUtility.FromJson<Names>((Resources.Load("names") as TextAsset).text);
	}

	// Update is called once per frame
	void Update() {

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


	[YarnCommand("purchase")]
	public static void Purchase() {
		var gc = FindFirstObjectByType<GameController>();
		gc.yarnStorage.TryGetValue<System.Single>($"$askingPrice", out var purchasePrice);
		Debug.Log($"Purchased for {purchasePrice}!");
	}

	[YarnCommand("storm_out")]
	public static void StormOut() {
		var gc = FindFirstObjectByType<GameController>();
		Debug.Log("storm_out!");
	}


}
