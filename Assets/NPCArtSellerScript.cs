using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class NPCArtSellerScript : MonoBehaviour {

	public GameController.Npc npc;
	public NavMeshAgent agent;

	public Transform[] paintingSpawnPositions;
	public Transform paintingHolder;
	public GameObject paintingAPrefab;
	public GameObject paintingBPrefab;

	async void Start() {
		await UniTask.Delay(Random.Range(100, 4000 + 1));
		agent.SetDestination()
	}

	// Update is called once per frame
	void Update() {

	}
}
