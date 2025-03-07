using System.Collections.Generic;
using UnityEngine;

public class LedgerScript : MonoBehaviour {
	private GameController gc;
	public List<JournalEntryScript> journalEntries;

	private void Start() {
		if (gc is not null) {
			gc = FindFirstObjectByType<GameController>();
		}
	}


	// Update is called once per frame
	void Update() {

	}


	public void Render() {
		if (gc is null) {
			gc = FindFirstObjectByType<GameController>();
		}

		if (gc.purchases is not null) {

			for (var i = 0; i < gc.purchases.Count; i++) {
				if (i < journalEntries.Count) {
					journalEntries[i].type = gc.purchases[i].type;
					journalEntries[i].npcsName = gc.purchases[i].npcsName;
					journalEntries[i].artistsName = gc.purchases[i].artistsName;
					journalEntries[i].salePrice = gc.purchases[i].salePrice;
					journalEntries[i].realAValue = gc.purchases[i].actualValue;
					journalEntries[i].profit = gc.purchases[i].profit;
					journalEntries[i].isFake = gc.purchases[i].isFake;
				}
			}
		}
		else {
			Debug.Log("gc.purchases is null!");
		}
	}
}
