using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class NPCArtSellerScript : MonoBehaviour {

	public GameController.Npc npc;
	public NavMeshAgent agent;
	public Animator animator;

	public Transform[] paintingSpawnPositions;
	public Transform paintingHolder;
	public ArtObjectScript painting;
	public GameObject paintingAPrefab;
	public GameObject paintingBPrefab;
	public bool leaving;
	public bool lastLeaving;



	[Header("Face")]
	public DecalProjector leftEyeBrowProjector;
	public DecalProjector rightEyeBrowProjector;
	public DecalProjector leftEyeProjector;
	public DecalProjector rightEyeProjector;
	public DecalProjector noseProjector;
	public DecalProjector mouthProjector;






	private GameController gc;
	private int paintingSpawnPositionIndex;
	private Transform paintingSpawnPosition;

	async void Start() {

		//Shorthand the game controller
		gc = FindFirstObjectByType<GameController>();


		//Generate a painting
		painting = Instantiate(gc.FlipCoin() ? paintingAPrefab : paintingBPrefab, paintingHolder).GetComponent<ArtObjectScript>();

		//Set its name for the animator
		painting.name = "painting";

		//Pick a spawn location
		paintingSpawnPositionIndex = Random.Range(0, paintingSpawnPositions.Length);

		//Shorthand it
		paintingSpawnPosition = paintingSpawnPositions[paintingSpawnPositionIndex];

		//Sync it to the spawn location values
		painting.transform.position = paintingSpawnPosition.position;
		painting.transform.rotation = paintingSpawnPosition.rotation;
		painting.transform.localScale = paintingSpawnPosition.localScale;



		//Set up the NPC's info
		var newNPC = new GameController.Npc();
		newNPC.name = gc.CreateName();
		newNPC.askingPrice = Random.Range(45, 769);
		newNPC.thinksItsFake = gc.FlipCoin();
		newNPC.willAcceptPrice = (int)(newNPC.thinksItsFake ? (newNPC.askingPrice / 3) : (newNPC.askingPrice - newNPC.askingPrice * 0.1));
		newNPC.willStormOut = gc.FlipCoin();
		newNPC.artPiece = painting.artValues;

		npc = newNPC;

		gc.yarnStorage.SetValue("$signatureName", npc.artPiece.signatureName);
		gc.yarnStorage.SetValue("$artistsRealName", npc.artPiece.artistsRealName);
		gc.yarnStorage.SetValue("$impersonatedArtistsName", npc.artPiece.impersonatedArtistsName);
		gc.yarnStorage.SetValue("$isFake", npc.artPiece.isFake);
		gc.yarnStorage.SetValue("$isGoodFake", npc.artPiece.isGoodFake);
		gc.yarnStorage.SetValue("$actualValue", npc.artPiece.actualValue);

		gc.yarnStorage.SetValue("$npcName", npc.name);
		gc.yarnStorage.SetValue("$thinksItsFake", npc.thinksItsFake);
		gc.yarnStorage.SetValue("$willStormOut", npc.willStormOut);
		gc.yarnStorage.SetValue("$askingPrice", npc.askingPrice);
		gc.yarnStorage.SetValue("$willAcceptPrice", npc.willAcceptPrice);


		//Rebind the animator to include the new painting
		animator.Rebind();

		BuildFace();

		//Wait a bit
		await UniTask.Delay(Random.Range(100, 4000 + 1));

		//Then go inside
		GoToCounter();
	}

	// Update is called once per frame
	void Update() {

		if (lastLeaving != leaving && leaving) {
			lastLeaving = leaving;
			Leave();
		}
	}

	private async UniTask GoToWaypoint(Transform destination) {
		await GoToWaypoint(destination.position, new Vector3(-999, -999, -999));
	}

	private async UniTask GoToWaypoint(Transform destination, Vector3 facingDirectionAtEnd) {
		await GoToWaypoint(destination.position, facingDirectionAtEnd);
	}

	private async UniTask GoToWaypoint(Vector3 destination, Vector3 facingDirectionAtEnd, bool startBeforeEnd = false) {
		agent.SetDestination(destination);
		var hasRotated = false;
		var angularSpeed = agent.angularSpeed;

		while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance) {

			if (facingDirectionAtEnd.x != -999 && !startBeforeEnd && !hasRotated && agent.remainingDistance < 2) {

				agent.angularSpeed = 0;

				hasRotated = true;

				gc.Rotate(transform, facingDirectionAtEnd);
			}

			await UniTask.Yield();
		}

		if (facingDirectionAtEnd.x != -999 && !startBeforeEnd) {
			gc.Rotate(transform, facingDirectionAtEnd);
		}
		agent.angularSpeed = angularSpeed;

	}

	private async void GoToCounter() {

		//Stop the auto brake, and increase the stopping distance
		//Makes for smoother waypoints
		agent.autoBraking = false;
		agent.stoppingDistance = 2;

		//Go to the the outside door waypoint
		await GoToWaypoint(gc.waypointOutsideDoor);

		//Go to the the inside door waypoint
		await GoToWaypoint(gc.waypointInsideDoor);

		//Turn braking back on, and make the agent stop on the dot
		agent.autoBraking = true;
		agent.stoppingDistance = 0;

		//Re-enable the animator
		//animator.enabled = true;

		//Go to the counter
		await GoToWaypoint(gc.waypointCounter, gc.waypointCounter.eulerAngles);

		//Turn the agent off
		agent.enabled = false;

		//Trigger the painting placement animation
		animator.SetInteger("state", paintingSpawnPositionIndex + 1);
		animator.SetTrigger("trigger");
		await UniTask.DelayFrame(60);
		animator.ResetTrigger("trigger");

		await UniTask.Delay(1150);


		//Move the painting over to the countertop
		var paintingPosBackup = painting.transform.position;
		var paintingRotBackup = painting.transform.rotation;

		//Move the painting over to the countertop
		painting.transform.parent = gc.counterPaintingHolder;

		//Rebind the animator, to release the painting
		animator.Rebind();

		//Update the animator a non-frame
		animator.Update(0);

		//Re-apply the position
		painting.transform.position = paintingPosBackup;

		//Re-apply the rotation
		painting.transform.rotation = paintingRotBackup;

		gc.npcInConversation = this;
		gc.readyToTalk = true;
	}

	public async void Leave() {

		//Turn the agent off
		agent.enabled = true;
		
		gc.talking = false;
		
		agent.autoBraking = false;
		agent.stoppingDistance = 2;
		await GoToWaypoint(gc.waypointInsideDoor);
		await GoToWaypoint(gc.waypointLeave);
		
		
		await UniTask.Delay(4000);
		gc.SpawnNewNPC();
		Destroy(gameObject);

	}

	public async void LeaveWithPainting() {

		gc.talking = false;
		
		//Backup the pianting data
		var paintingPosBackup = painting.transform.position;
		var paintingRotBackup = painting.transform.rotation;

		//Move the painting back to the npc
		painting.transform.parent = paintingHolder;

		//Rebind the animator, to release the painting
		animator.Rebind();

		//Update the animator a non-frame
		animator.Update(0);

		//Re-apply the position
		painting.transform.position = paintingPosBackup;

		//Re-apply the rotation
		painting.transform.rotation = paintingRotBackup;

		//Trigger the painting placement animation
		animator.SetInteger("state", (paintingSpawnPositionIndex + 1) + 3);
		animator.SetTrigger("trigger");
		await UniTask.DelayFrame(60);
		animator.ResetTrigger("trigger");

		await UniTask.Delay(900);
		
		Leave();
		
	}


	private void BuildFace() {

		//Roll the die and get the face parts
		var leftEye = gc.eyes[Random.Range(0, gc.eyes.Length)];
		var rightEye = gc.eyes[Random.Range(0, gc.eyes.Length)];
		var leftEyebrow = gc.eyebrows[Random.Range(0, gc.eyebrows.Length)];
		var rightEyebrow = gc.eyebrows[Random.Range(0, gc.eyebrows.Length)];
		var nose = gc.noses[Random.Range(0, gc.noses.Length)];
		var mouth = gc.mouths[Random.Range(0, gc.mouths.Length)];


		//Set the projectors to instanced materials
		leftEyeProjector.material = new Material(leftEyeProjector.material);
		rightEyeProjector.material = new Material(rightEyeProjector.material);
		leftEyeBrowProjector.material = new Material(leftEyeBrowProjector.material);
		rightEyeBrowProjector.material = new Material(rightEyeBrowProjector.material);
		noseProjector.material = new Material(noseProjector.material);
		mouthProjector.material = new Material(mouthProjector.material);


		leftEyeProjector.material.SetTexture("Base_Map", leftEye);
		rightEyeProjector.material.SetTexture("Base_Map", rightEye);
		leftEyeBrowProjector.material.SetTexture("Base_Map", leftEyebrow);
		rightEyeBrowProjector.material.SetTexture("Base_Map", rightEyebrow);
		noseProjector.material.SetTexture("Base_Map", nose);
		mouthProjector.material.SetTexture("Base_Map", mouth);

	}
}
