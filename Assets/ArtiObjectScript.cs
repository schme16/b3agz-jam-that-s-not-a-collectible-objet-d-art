using TMPro;
using UnityEngine;

public class ArtiObjectScript : MonoBehaviour {

	//This is shown in the signature, if a signature is shown at all
	public string signatureName;

	//Who actually pained it
	public string artistsRealName;

	//The real artist's name
	public string impersonatedArtistsName;

	//Is it a fake?
	public bool isFake;

	//Is it a fake?
	public bool isGoodFake;

	//How much did they want
	public float askValue;

	//How much would they settle for
	public float settleValue;

	//What's it actually worth
	public float actualValue;

	//Which of the signatures, if any, should be shown
	public int signatureLocation;

	//All possible signature locations 
	public TextMeshPro[] signatureLocations;

	//The chosen font for the signature
	public int signatureFont;

	//All possible signature fonts 
	public TMP_FontAsset[] signatureFonts;

	//The signature element
	private TextMeshPro signature;

	//Which frame should be displayed
	public int frameOption;

	//All frame options
	public GameObject[] frameOptions;

	//The specific material chosen for the frame
	public int frameMaterial;

	//All available frame materials
	public Material[] frameMaterials;

	//The artwork texture
	public Texture[] realArtwork;

	//The artwork texture
	public Texture[] fakeArtwork;

	//The artwork texture
	public Texture artwork;

	//The canvas element
	public MeshRenderer canvas;

	//The frame element
	private MeshRenderer frame;

	//Game controller
	private GameController gc;



	void Start() {

		//Get the game controller
		gc = FindFirstObjectByType<GameController>();

		PickRandomValues();

		//Hide all non-active frames
		foreach (var _frame in frameOptions) {
			_frame.SetActive(_frame == frameOptions[frameOption]);
		}

		//Get the frame renderer
		frame = frameOptions[frameOption].GetComponent<MeshRenderer>();

		//Instantiate the frame material
		frame.material = new Material(frameMaterials[frameMaterial]);


		//Instantiate the canvas material
		canvas.material = new Material(canvas.material) {

			//Set the artwork texture
			mainTexture = artwork,
		};


		//Hide all non-active signatures
		foreach (var _signatureLocation in signatureLocations) {
			_signatureLocation.gameObject.SetActive(_signatureLocation == signatureLocations[signatureLocation]);
		}

		if (signatureLocation > -1) {
			
			//Shorthand the signature
			signature = signatureLocations[signatureLocation];
			
			//Set the font
			signature.font = signatureFonts[signatureFont];
			
			//Set the text
			signature.SetText(signatureName);
		}
		else {
			Debug.Log("No signature for this artwork");
		}



	}

	// Update is called once per frame
	void Update() {

	}


	void PickRandomValues() {

		//Is it fake? Flip a coin
		isFake = Random.Range(0, 2) == 1;

		//If it IS fake
		if (isFake) {

			//Is it a good fake? Flip a coin
			isGoodFake = Random.Range(0, 2) == 1;
		}

		//Pick a frame
		frameOption = Random.Range(0, frameOptions.Length);

		//Pick a frame matereial
		frameMaterial = Random.Range(0, frameMaterials.Length);


		//Pick a font
		signatureFont = Random.Range(0, signatureFonts.Length);



		//Set the artists real name 
		artistsRealName = gc.CreateName();

		//If fake set the name of the artist being impersonated, if real set it to artistsRealName
		impersonatedArtistsName = isFake ? gc.CreateName() : artistsRealName;

		//If fake make up a new name, or use the artistsRealName, if real set it to the artistsRealName
		signatureName = isFake ? (Random.Range(0, 2) == 1 ? artistsRealName : gc.CreateName()) : artistsRealName;


		//Pick a font
		signatureFont = Random.Range(0, signatureFonts.Length);


		//Is the artwork fake? (but not a good fake)
		if (isFake && !isGoodFake) {
			artwork = fakeArtwork[Random.Range(0, fakeArtwork.Length)];

			//Pick a signature location, mandatory for low quality fakes
			signatureLocation = Random.Range(0, signatureLocations.Length);
		}

		//Real art, or good fake
		else {
			artwork = realArtwork[Random.Range(0, realArtwork.Length)];

			//Pick a signature location, or leave it off entirely 
			signatureLocation = Random.Range(-1, signatureLocations.Length);
		}


	}
}
