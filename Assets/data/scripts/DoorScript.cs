using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class DoorScript : MonoBehaviour {

	public Vector3 start;
	public Vector3 end;
	public float speed;
	public bool open;
	public bool lastOpenState;
	public Transform pivot;
	public bool debug;
	public float posInRotation;
	public AudioClip sfxDoorHandle;
	public float minPitch;
	public float maxPitch;
	public AudioSource audioSource;
	private bool hasOpened;




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


		if (open != lastOpenState) {
			if (open && !hasOpened) {
				hasOpened = true;
				audioSource.pitch = Random.Range(0.9f, 1.1f);
				audioSource.PlayOneShot(sfxDoorHandle);
			}
		}

		if (hasOpened && !open && !lastOpenState && posInRotation == 0) {
			audioSource.pitch = Random.Range(0.9f, 1.1f);
			audioSource.PlayOneShot(sfxDoorHandle);
			hasOpened = false;
		}

		lastOpenState = open;

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
