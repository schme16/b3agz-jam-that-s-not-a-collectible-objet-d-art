using UnityEngine;

public class ObjectManipulation : MonoBehaviour {
	public float rotationSpeed = 200f;
	public float moveSpeed = 0.01f; // Reduced speed for smooth movement
	public float zoomSpeed = 5f;
	public float zLockPosition = 0f; // Keeps the object Z-centered
	public Vector3 startRotation; // Keeps the object Z-centered
	public Vector3 startScale; // Keeps the object Z-centered

	private Vector3 _previousMousePosition;
	private bool _isRotating = false;
	private bool _isMoving = false;

	void Start() {
		// Ensure object starts locked on Z-axis
		//transform.position = new Vector3(transform.position.x, transform.position.y, zLockPosition);
	}

	void Update() {
		HandleRotation();
		//HandleTranslation();
		HandleZoom();
		//LockZAxis();
	}

	private void HandleRotation() {
		if (Input.GetMouseButtonDown(0)) {
			_isRotating = true;
			_previousMousePosition = Input.mousePosition;
		}
		if (Input.GetMouseButtonUp(0)) {
			_isRotating = false;
		}

		if (_isRotating) {
			Vector3 mouseDelta = (Input.mousePosition - _previousMousePosition) * Time.deltaTime;

			float rotX = mouseDelta.y * rotationSpeed;
			float rotY = -mouseDelta.x * rotationSpeed;

			transform.Rotate(Vector3.right, rotX, Space.Self);
			transform.Rotate(Vector3.up, rotY, Space.World);

			_previousMousePosition = Input.mousePosition;
		}
	}

	private void HandleTranslation() {
		
		if (Input.GetMouseButtonDown(1)) {
			_isMoving = true;
			_previousMousePosition = Input.mousePosition;
		}
		if (Input.GetMouseButtonUp(1)) {
			_isMoving = false;
		}

		if (_isMoving) {
			Vector3 mouseDelta = (Input.mousePosition - _previousMousePosition) * moveSpeed * Time.deltaTime;

			// Move in local X and Y
			Vector3 localMove = transform.right * mouseDelta.x + transform.up * mouseDelta.y;
			transform.position += localMove;

			_previousMousePosition = Input.mousePosition;
		}
	}

	private void HandleZoom() {
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		if (scroll != 0) {
			transform.position += transform.forward * scroll * zoomSpeed;
		}
	}

	private void LockZAxis() {
		// Keep object centered on the Z-axis
		transform.position = new Vector3(transform.position.x, transform.position.y, zLockPosition);
	}
}
