using UnityEngine;

public class GameController : MonoBehaviour {

	public DoorScript door;

	public GameObject[] ArtPrefabs;
	public AudioSource audioSource;
	public Names names;
	public ArtiObjectScript portraitHangPoints;
	public ArtiObjectScript squareHangPoints;
	public Art[] collectedArt;

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
		public float actualValue;
		public int signatureLocation;
		public int signatureFont;
		public int frameOption;
		public int frameMaterial;
		public int whichArtwork;
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


}
