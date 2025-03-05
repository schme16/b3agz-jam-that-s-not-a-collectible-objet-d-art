using UnityEngine;

public class ObjectRotate : MonoBehaviour {
	public Vector3 startDragDir;
	public Vector3 currentDragDir;
	public Quaternion initialRotation;
	public float angleFromStart;

	void OnMouseDown() {
		Debug.Log(1111);
		startDragDir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;

		initialRotation = transform.rotation;
	}

	void OnMouseDrag() {
		Debug.Log(2222);
		currentDragDir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;

		//gives you the angle in degrees the mouse has rotated around the object since starting to drag
		angleFromStart = Vector3.Angle(startDragDir, currentDragDir);

		transform.rotation = initialRotation;
		transform.Rotate(0.0f, angleFromStart, 0.0f);
	}
}
