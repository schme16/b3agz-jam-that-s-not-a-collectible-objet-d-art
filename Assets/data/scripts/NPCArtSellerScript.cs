using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
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
	public Transform waypointCounter;
	public Transform waypointInsideDoor;
	public Transform waypointOutsideDoor;
	public Transform waypointLeave;
	public bool leaving;
	public bool lastLeaving;
	private GameController gc;
	private int paintingSpawnPositionIndex;
	private Transform paintingSpawnPosition;

	async void Start() {
		
		//Shorthand the game controller
		gc = FindFirstObjectByType<GameController>();

		//Turn the animator off until we generate the painting
		//animator.enabled = false;
		
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
	

		//Rebind the animator to include the new painting
		animator.Rebind();
		
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
		await GoToWaypoint(destination.position);
	}

	private async UniTask GoToWaypoint(Vector3 destination) {
		agent.SetDestination(destination);

		while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance) {
			await UniTask.Yield();
		}
	}

	private async void GoToCounter() {

		//Stop the auto brake, and increase the stopping distance
		//Makes for smoother waypoints
		agent.autoBraking = false;
		agent.stoppingDistance = 2;
		
		//Go to the the outside door waypoint
		await GoToWaypoint(waypointOutsideDoor);
		
		//Go to the the inside door waypoint
		await GoToWaypoint(waypointInsideDoor);
		
		//Turn braking back on, and make the agent stop on the dot
		agent.autoBraking = true;
		agent.stoppingDistance = 0;
		
		//Re-enable the animator
		//animator.enabled = true;
		
		//Go to the counter
		await GoToWaypoint(waypointCounter);
		
		//Trigger the painting placement animation
		animator.SetInteger("state", paintingSpawnPositionIndex + 1);
		animator.SetTrigger("trigger");
		await UniTask.Delay(1500);
		animator.ResetTrigger("trigger");
	}

	private async void Leave() {

		agent.autoBraking = false;
		agent.stoppingDistance = 2;
		await GoToWaypoint(waypointInsideDoor.position);
		await GoToWaypoint(waypointLeave.position);
	}
}
