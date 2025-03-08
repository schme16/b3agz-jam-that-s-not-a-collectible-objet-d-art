using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class NPCArtSellerScript : MonoBehaviour {

	public GameController.Npc npc;
	public NavMeshAgent agent;
	public Animator animator;
	public Transform cameraTarget;

	public Transform[] paintingSpawnPositions;
	public Transform paintingHolder;
	public ArtObjectScript painting;
	public InteractableScript interact;
	public bool leaving;
	public bool lastLeaving;
	public float totalTimeRingingBell;
	public float bellTimer = 0;
	public float bellInterval = 3;



	[Header("Face")]
	public DecalProjector hairProjector;
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

		//Pick a spawn location
		paintingSpawnPositionIndex = Random.Range(0, paintingSpawnPositions.Length);

		//Shorthand it
		paintingSpawnPosition = paintingSpawnPositions[paintingSpawnPositionIndex];

		painting = gc.SpawnRandomPainting(paintingHolder, paintingSpawnPosition);

		interact.OnInteract.AddListener(() => {
			gc.StartConversationWithNPC();
		});

		//Set up the NPC's info
		var newNPC = new GameController.Npc();
		newNPC.name = GameController.CreateName();
		newNPC.thinksItsFake = GameController.FlipCoin();
		newNPC.askingPrice = newNPC.thinksItsFake || painting.artValues.isFake ? Random.Range(painting.artValues.actualValue / 2, painting.artValues.actualValue + 20) : Random.Range(painting.artValues.actualValue / 2, painting.artValues.actualValue + 75);
		newNPC.willAcceptPrice = (int)(newNPC.thinksItsFake ? (newNPC.askingPrice / 3) : (Random.Range(newNPC.askingPrice - newNPC.askingPrice * 0.3f, newNPC.askingPrice - newNPC.askingPrice * 0.1f)));
		newNPC.willStormOut = Random.Range(0, 4) == 3;
		newNPC.artPiece = painting.artValues;
		
		Debug.Log($"real value: {painting.artValues.actualValue}, asking: {newNPC.askingPrice}");

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
	async void Update() {

		if (!leaving && interact.enabled && !gc.talking) {
			if (!gc.inTalkTrigger) {
				bellTimer += Time.deltaTime;

				totalTimeRingingBell += Time.deltaTime;
				if (bellTimer > bellInterval) {

					gc.deskBell.RingBell();
					bellInterval = Mathf.Clamp(bellInterval - Random.Range(0.25f, 0.5f), 0.25f, 10);
					bellTimer = 0;

					//Add a little money onto their asking price for every ring
					var priceIncrement = Random.Range(1, 6);
					var newNPCData = npc;
					newNPCData.askingPrice += priceIncrement;
					newNPCData.willAcceptPrice += priceIncrement;
					npc = newNPCData;
	
					gc.yarnStorage.SetValue("$askingPrice", npc.askingPrice);
					gc.yarnStorage.SetValue("$willAcceptPrice", npc.willAcceptPrice);


					if (totalTimeRingingBell > 15 && GameController.FlipCoin()) {
						
						LeaveWithPainting();
						
						await UniTask.Delay(2500);
						
						if (!gc.flags.customerHasWalkedOut) {
							gc.flags.customerHasWalkedOut = true;
							gc.FlagsCheck();
						}
					}
				}
			}
			else {
				bellInterval = 3;
				bellTimer = 0;
			}
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
		interact.enabled = true;
	}

	public async void Leave() {

		//Turn the agent off
		agent.enabled = true;
		leaving = true;

		interact.enabled = false;

		agent.autoBraking = false;
		agent.stoppingDistance = 2;
		await GoToWaypoint(gc.waypointInsideDoor);
		await GoToWaypoint(gc.waypointLeave);


		await UniTask.Delay(2000);

		gc.currentNPC = null;
		

		if (gc.answeringMachine.pendingMessages.Count == 0) {
			gc.SpawnNewNPC();
		}

		Destroy(gameObject);

	}

	public async void LeaveWithPainting() {
		if (leaving) {
			return;
		}

		interact.enabled = false;
		leaving = true;

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
		var hair = gc.hair[Random.Range(0, gc.hair.Length)];
		var leftEye = gc.eyes[Random.Range(0, gc.eyes.Length)];
		var rightEye = gc.eyes[Random.Range(0, gc.eyes.Length)];
		var leftEyebrow = gc.eyebrows[Random.Range(0, gc.eyebrows.Length)];
		var rightEyebrow = gc.eyebrows[Random.Range(0, gc.eyebrows.Length)];
		var nose = gc.noses[Random.Range(0, gc.noses.Length)];
		var mouth = gc.mouths[Random.Range(0, gc.mouths.Length)];

		var hasHat = GameController.FlipCoin();
		
		if (hasHat) {
			Instantiate(gc.hats[Random.Range(0, gc.hats.Length)], transform);
			hairProjector.enabled = false;
		}


		//Set the projectors to instanced materials
		hairProjector.material = new Material(hairProjector.material);
		leftEyeProjector.material = new Material(leftEyeProjector.material);
		rightEyeProjector.material = new Material(rightEyeProjector.material);
		leftEyeBrowProjector.material = new Material(leftEyeBrowProjector.material);
		rightEyeBrowProjector.material = new Material(rightEyeBrowProjector.material);
		noseProjector.material = new Material(noseProjector.material);
		mouthProjector.material = new Material(mouthProjector.material);


		hairProjector.material.SetTexture("Base_Map", hair);
		leftEyeProjector.material.SetTexture("Base_Map", leftEye);
		rightEyeProjector.material.SetTexture("Base_Map", leftEye);
		leftEyeBrowProjector.material.SetTexture("Base_Map", leftEyebrow);
		rightEyeBrowProjector.material.SetTexture("Base_Map", leftEyebrow);
		noseProjector.material.SetTexture("Base_Map", nose);
		mouthProjector.material.SetTexture("Base_Map", mouth);

	}
}
