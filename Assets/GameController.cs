using UnityEngine;

public class GameController : MonoBehaviour {

	public DoorScript door;

	public GameObject[] ArtPrefabs;
	public AudioSource audioSource;
	public Names names;
	
	public struct Names {
		public string[] first;
		public string[] middle;
		public string[] last;
	}

	
	void Start() {
		names = JsonUtility.FromJson<Names>((Resources.Load("names") as TextAsset).text);
	}

	// Update is called once per frame
	void Update() {

	}
	
	public string CreateName() {
		return $"{names.first[Random.Range(0, names.first.Length)]} {(Random.Range(0, 2) == 1 ? names.middle[Random.Range(0, names.middle.Length)] : "")} {names.last[Random.Range(0, names.last.Length)]}";
	}
}
