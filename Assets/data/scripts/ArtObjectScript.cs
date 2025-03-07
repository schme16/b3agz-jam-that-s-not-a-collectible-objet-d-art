using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ArtObjectScript : MonoBehaviour {


	//Were the details provided?
	public bool PresetValues;

	//The values that create this artwork
	public GameController.Art artValues;

	//All possible signature locations 
	public TextMeshPro[] signatureLocations;

	//All possible signature fonts 
	public TMP_FontAsset[] signatureFonts;

	//The signature element
	private TextMeshPro signature;

	//The signature element
	public bool isSquare;

	//All frame options
	public GameObject[] frameOptions;

	//All available frame materials
	public Material[] frameMaterials;

	//All available frame materials
	public Material[] frameTransparentMaterials;

	//The artwork texture
	public Texture[] realArtwork;
	public Vector3 initialScale;

	//The artwork texture
	public Texture[] fakeArtwork;

	//The artwork texture
	private Texture artwork;

	//The canvas element
	public MeshRenderer canvas;

	//The canvas element
	public Material transparentCanvasMaterial;

	//The canvas element
	public bool useTransparentMaterials;

	//The frame element
	private MeshRenderer frame;

	//Game controller
	private GameController gc;

	void Start() {

		//Get the game controller
		gc = FindFirstObjectByType<GameController>();

		initialScale = transform.localScale;


		//Not a preset artwork, aka saved and loaded artwork?
		if (!PresetValues) {

			//Pick some random values
			PickRandomValues();
			Render();
		}

		Render();
	}

	// Update is called once per frame
	void Update() {

	}


	public void PickRandomValues() {
		var randomArtValues = new GameController.Art();

		randomArtValues.hangedPosition = -1;
		randomArtValues.isSquare = isSquare;

		//Is it fake? Flip a coin
		randomArtValues.isFake = GameController.FlipCoin();

		//If it IS fake
		if (randomArtValues.isFake) {

			//Is it a good fake? Flip a coin
			randomArtValues.isGoodFake = GameController.FlipCoin();
		}

		//Pick a frame
		randomArtValues.frameOption = Random.Range(0, frameOptions.Length);

		//Pick a frame matereial
		randomArtValues.frameMaterial = Random.Range(0, frameMaterials.Length);

		//Pick a font
		randomArtValues.signatureFont = Random.Range(0, signatureFonts.Length);

		//Set the artists real name 
		randomArtValues.artistsRealName = GameController.CreateName();

		//If fake set the name of the artist being impersonated, if real set it to artistsRealName
		randomArtValues.impersonatedArtistsName = randomArtValues.isFake ? GameController.CreateName() : randomArtValues.artistsRealName;

		//If fake make up a new name, or use the artistsRealName, if real set it to the artistsRealName
		randomArtValues.signatureName = randomArtValues.isFake ? (GameController.FlipCoin() ? randomArtValues.artistsRealName : GameController.CreateName()) : randomArtValues.artistsRealName;


		//Pick a font
		randomArtValues.signatureFont = Random.Range(0, signatureFonts.Length);

		randomArtValues.whichArtwork = Random.Range(0, realArtwork.Length);

		//Is the artwork fake? (but not a good fake)
		if (randomArtValues.isFake && !randomArtValues.isGoodFake) {
			artwork = fakeArtwork[randomArtValues.whichArtwork];
			
			
			randomArtValues.actualValue = Random.Range(150, 350);
			//Pick a signature location, mandatory for low quality fakes
			randomArtValues.signatureLocation = Random.Range(0, signatureLocations.Length);
		}

		//Real art, or good fake
		else {
			artwork = realArtwork[randomArtValues.whichArtwork];
			randomArtValues.actualValue = Random.Range(250, 750);

			//Pick a signature location, or leave it off entirely 
			randomArtValues.signatureLocation = GameController.FlipCoin() ? Random.Range(-1, signatureLocations.Length) : -1;
		}

		artValues = randomArtValues;

	}

	public void LoadSavedArtwork(GameController.Art values) {

		artValues = values;

		//Is the artwork fake? (but not a good fake)
		if (values.isFake && !values.isGoodFake) {
			artwork = fakeArtwork[values.whichArtwork];
		}

		//Real art, or good fake
		else {
			artwork = realArtwork[values.whichArtwork];
		}
		Render();
	}

	public void Render() {

		//Get the frame renderer
		frame = frameOptions[artValues.frameOption].GetComponent<MeshRenderer>();

		//Instantiate the frame material
		frame.material = new Material(useTransparentMaterials ? frameTransparentMaterials[artValues.frameMaterial] : frameMaterials[artValues.frameMaterial]);

		//Instantiate the canvas material
		canvas.material = new Material(useTransparentMaterials ? transparentCanvasMaterial : canvas.material);


		//Hide all non-active frames
		foreach (var _frame in frameOptions) {
			_frame.SetActive(_frame == frameOptions[artValues.frameOption]);
		}

		//Set the artwork texture
		canvas.material.mainTexture = artwork;


		//Hide all non-active signatures
		foreach (var _signatureLocation in signatureLocations) {
			_signatureLocation.gameObject.SetActive(artValues.signatureLocation > -1 && _signatureLocation == signatureLocations[artValues.signatureLocation]);
		}

		if (artValues.signatureLocation > -1) {

			//Shorthand the signature
			signature = signatureLocations[artValues.signatureLocation];

			//Set the font
			signature.font = signatureFonts[artValues.signatureFont];

			//Set the text
			signature.SetText(artValues.signatureName);
		}
		else {
			//Debug.Log("No signature for this artwork");
		}

	}
}
