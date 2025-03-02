using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class NPCArtSellerScript : MonoBehaviour {

	public GameController.Npc npc;
	public NavMeshAgent agent;

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

	async void Start() {
		gc = FindFirstObjectByType<GameController>();


		//Generate a painting
		painting = Instantiate(gc.FlipCoin() ? paintingAPrefab : paintingBPrefab, paintingHolder).GetComponent<ArtObjectScript>();
		var spawnPosition = paintingSpawnPositions[Random.Range(0, paintingSpawnPositions.Length)];
		painting.transform.position = spawnPosition.position;
		painting.transform.rotation = spawnPosition.rotation;
		painting.transform.localScale = spawnPosition.localScale;
		
		await UniTask.Delay(Random.Range(100, 4000 + 1));


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

		agent.autoBraking = false;
		agent.stoppingDistance = 2;
		await GoToWaypoint(waypointOutsideDoor);
		await GoToWaypoint(waypointInsideDoor);
		await GoToWaypoint(waypointCounter);
	}

	private async void Leave() {

		agent.autoBraking = false;
		agent.stoppingDistance = 2;
		await GoToWaypoint(waypointInsideDoor.position);
		await GoToWaypoint(waypointLeave.position);
	}
}
