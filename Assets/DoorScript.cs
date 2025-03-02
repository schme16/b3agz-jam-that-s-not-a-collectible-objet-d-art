using System;
using UnityEngine;

public class DoorScript : MonoBehaviour {

	public Vector3 start;
	public Vector3 end;
	public float speed;
	public bool open;
	public Transform pivot;
	public bool debug;
	public float posInRotation;




	void Start() {
		posInRotation = 0;
		start = pivot.eulerAngles;
	}

	// Update is called once per frame
	void Update() {

		if (open) {
			posInRotation = Math.Clamp(posInRotation + (Time.deltaTime * speed), 0, 1);
		}
		else {
			posInRotation = Math.Clamp(posInRotation - (Time.deltaTime * speed), 0, 1);
		}
		
	
		pivot.eulerAngles = (Vector3.Lerp(start, end, posInRotation));
	}

	private void OnTriggerEnter(Collider other) {
		open = true;
	}

	private void OnTriggerStay(Collider other) {
		open = true;
	}
	
	private void OnTriggerExit(Collider other) {
		open = false;
	}
}
