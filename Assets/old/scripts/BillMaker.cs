using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillMaker {

	public string noun;
	public string verb;
	public string prenoun;
	public string fluff;
	public float bias;

	private BillData billData;


	public void Create() {
		InitArrays();
		noun = billData.nouns[Random.Range(0, billData.nouns.Length)];
		verb = billData.verbs[Random.Range(0, billData.verbs.Length)];
		prenoun = billData.prenouns[Random.Range(0, billData.prenouns.Length)];
		fluff = billData.fluffs[Random.Range(0, billData.fluffs.Length)];
		bias = (Random.Range(0, 201) - 100) * 1;
	}

	private void InitArrays() {
		string derp = File.ReadAllText(Application.dataPath + "/data/jsonTest.json");
		billData = BillData.CreateFromJSON(derp);
	}



}

public class BillData {
	public string[] nouns;
	public string[] verbs;
	public string[] fluffs;
	public string[] prenouns;

	public static BillData CreateFromJSON(string jsonString) {
		return JsonUtility.FromJson<BillData>(jsonString);
	}
}
