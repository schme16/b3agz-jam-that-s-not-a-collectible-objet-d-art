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
	
	//How much did they want
	public float askValue;
	
	//How much would they settle for
	public float settleValue;
	
	//What's it actually worth
	public float actualValueValueValue;
	
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
	
	
	
	void Start() {
		
		//Hide all non-active frames
		foreach (var _frame in frameOptions) {
			_frame.SetActive(_frame == frameOptions[frameOption]);
		}
		
		//Get the frame renderer
		frame = frameOptions[frameOption].GetComponent<MeshRenderer>();
				
		//Instantiate the frame material
		frame.material = new Material(frameMaterials[frameMaterial]);
		
		
		//Pick an artwork
		if (isFake) {
			artwork = fakeArtwork[Random.Range(0, fakeArtwork.Length)];
		}
		else {
			artwork = realArtwork[Random.Range(0, realArtwork.Length)];
		}
		
		
		
		//Instantiate the canvas material
		canvas.material = new Material(canvas.material) {
			
			//Set the artwork texture
			mainTexture = artwork,
		};


		//Hide all non-active signatures
		if (signatureLocation > -1) {
			
			foreach (var _signatureLocation in signatureLocations) {
				_signatureLocation.gameObject.SetActive(_signatureLocation == signatureLocations[signatureLocation]);
			}

			signature = signatureLocations[signatureLocation];
			signature.font = signatureFonts[signatureFont];

		}
		else {
			Debug.Log("No signature for this artwork");
		}
		
	}

	// Update is called once per frame
	void Update() {

	}
}
