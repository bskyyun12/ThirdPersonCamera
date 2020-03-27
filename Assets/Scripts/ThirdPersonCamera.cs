using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Camera))]
public class ThirdPersonCamera : MonoBehaviour
{
	#region Input Ids
	const string mouseWheelId = "Mouse ScrollWheel";
	const string mouseXId = "Mouse X";
	const string mouseYId = "Mouse Y";
	#endregion Input Ids

	Camera cam = null;
	float currentCameraDistance;
	float desiredCameraDistance = 8f;
	Quaternion desiredRotation;
	Quaternion currentRotation;
	float slerpRotationSpeed = .2f;
	Vector2 cameraEulerAngle = new Vector2(45f, 0f);
	Vector3 cameraDirection;
	Vector3 cameraFocusPosition;
	Vector3 desiredCameraPosition;

	[Header("Camera Collision")]
	[SerializeField] bool smoothZoomInToCollisionPoint = false;
	[SerializeField, Range(.01f, .5f)] float lerpSpeed = .175f;
	float currentLerpTime;
	bool boxCastHit;
	RaycastHit hit;
	Vector3 castDirection;
	Vector3 castLine;

	[Header("Camera Focus")]
	public GameObject cameraFocus = null;
	[Tooltip("This object will turn toward to camera's direction when mouse right button is pressed")]
	public GameObject player = null;

	[Header("Distance")]
	[SerializeField, Range(0f, 20f)] float minCameraDistance = 2f;
	[SerializeField, Range(1f, 50f)] float maxCameraDistance = 35f;

	[Header("Layer Filtering")]
	[SerializeField] LayerMask whatToDetect = -1;

	[Header("Rotation")]
	[SerializeField] bool reverseYAxis = true;
	[SerializeField, Range(-89f, 89f)] float minVerticalAngle = -30f;
	[SerializeField, Range(-89f, 89f)] float maxVerticalAngle = 80f;
	[SerializeField, Range(1f, 360f)] float rotateSpeed = 180f;

	[Header("Zoom")]
	[SerializeField] bool reverseZoom = true;
	[SerializeField] bool lerpZoom = false;
	[SerializeField, Range(10f, 200f)] float zoomSpeed = 50f;

	private void OnValidate()
	{
		if (maxVerticalAngle < minVerticalAngle)
		{
			maxVerticalAngle = minVerticalAngle;
		}
		if (maxCameraDistance < minCameraDistance)
		{
			maxCameraDistance = minCameraDistance;
		}
	}

	void Awake()
	{
		Assert.IsNotNull(cameraFocus, "cameraFocus == null");
		Assert.IsNotNull(player, "player == null");
		cam = GetComponent<Camera>();
	}

	void Start()
	{
		// Set initial rotation
		desiredRotation = Quaternion.Euler(cameraEulerAngle);

		//Cursor.lockState = CursorLockMode.Locked;
	}

	// Make sure the camera movement happens after player's movement. Otherwise, the camera will jiggle.
	void LateUpdate()
	{
		// Rotation
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
		{ ManualRotation(); }

		currentRotation = Quaternion.Slerp(currentRotation, desiredRotation, slerpRotationSpeed);

		// Zoom
		if (!Mathf.Approximately(Input.GetAxis(mouseWheelId), 0f))
		{ Zoom(); }

		cameraFocusPosition = cameraFocus.transform.position;
		cameraDirection = currentRotation * Vector3.forward;
		currentCameraDistance = Vector3.Distance(cameraFocusPosition, transform.position);

		boxCastHit = BoxCastFromTargetToCamera();

		#region When the camera is too close
		// change view to first person
		if (boxCastHit && hit.distance < minCameraDistance)
		{ hit.distance = 0f; }
		if (Mathf.Approximately(desiredCameraDistance, minCameraDistance))
		{ desiredCameraDistance = 0f; }
		#endregion When the camera is too close

		// No smooth when the camera zooms-in toward colliding point.
		if (!smoothZoomInToCollisionPoint && boxCastHit && currentCameraDistance > hit.distance)
		{ currentCameraDistance = hit.distance; }

		// Getting ease-out effect
		if (boxCastHit)
		{ currentLerpTime = 0f; }

		currentLerpTime += Time.deltaTime * lerpSpeed;
		currentLerpTime = Mathf.Clamp01(currentLerpTime);

		float t = currentLerpTime;
		t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
		t = Mathf.Sqrt(t);
		t = Mathf.Clamp01(t);

		// Smooth zoom-in and out on collision
		currentCameraDistance = Mathf.Lerp(
							currentCameraDistance,
							(boxCastHit ? hit.distance : desiredCameraDistance),
							t);

		desiredCameraPosition = cameraFocusPosition - cameraDirection * currentCameraDistance;

		transform.SetPositionAndRotation(desiredCameraPosition, currentRotation);
	}

	private bool BoxCastFromTargetToCamera()
	{
		desiredCameraPosition = cameraFocusPosition - cameraDirection * desiredCameraDistance;
		castLine = desiredCameraPosition - cameraFocusPosition;
		castDirection = castLine.normalized;
		return Physics.BoxCast(cameraFocusPosition,
					GetCameraHalfExtends(),
					castDirection,
					out hit,
					desiredRotation,
					castLine.magnitude,
					whatToDetect
					);
	}

	private Vector3 GetCameraHalfExtends()
	{
		Vector3 halfExtends = Vector3.zero;
		halfExtends.y = cam.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * cam.fieldOfView);
		halfExtends.x = halfExtends.y * cam.aspect;
		halfExtends.z = 0f;
		return halfExtends;
	}

	void ManualRotation()
	{
		if (!Mathf.Approximately(Input.GetAxis(mouseXId), 0f) || !Mathf.Approximately(Input.GetAxis(mouseYId), 0f))
		{
			if (currentLerpTime >= 1f)
			{ currentLerpTime = 0f; }

			Vector2 mouseInput = new Vector2(
								(reverseYAxis ? -Input.GetAxis(mouseYId) : Input.GetAxis(mouseYId)),
								Input.GetAxis(mouseXId));

			cameraEulerAngle += rotateSpeed * Time.unscaledDeltaTime * mouseInput;
			ConstrainAngles();

			desiredRotation = Quaternion.Euler(cameraEulerAngle);
			currentRotation = desiredRotation;

			// Moved this to PlayerInput.cs
			//if (Input.GetMouseButton(1))
			//{
			//	Vector3 cameraForward = transform.forward;
			//	cameraForward.y = 0f;
			//	//player.transform.forward = cameraForward;
			//	Quaternion lookRotation = Quaternion.LookRotation(cameraForward);
			//	player.transform.rotation = Quaternion.Slerp(player.transform.rotation, lookRotation, Time.deltaTime * 5f);
			//}
		}
	}

	public void ManualRotation(Vector3 newRotation)
	{
		cameraEulerAngle += (Vector2)newRotation;
		ConstrainAngles();
		desiredRotation = Quaternion.Euler(cameraEulerAngle);
	}

	void ConstrainAngles()
	{
		cameraEulerAngle.x = Mathf.Clamp(cameraEulerAngle.x, minVerticalAngle, maxVerticalAngle);

		if (cameraEulerAngle.y < 0f)
		{
			cameraEulerAngle.y += 360f;
		}
		else if (cameraEulerAngle.y >= 360f)
		{
			cameraEulerAngle.y -= 360f;
		}
	}

	void Zoom()
	{
		currentLerpTime = lerpZoom ? lerpSpeed : 1f;

		float wheelInput = Input.GetAxis(mouseWheelId);
		desiredCameraDistance = currentCameraDistance;
		desiredCameraDistance += (reverseZoom ? -wheelInput : wheelInput) * zoomSpeed;
		desiredCameraDistance = Mathf.Clamp(desiredCameraDistance, minCameraDistance, maxCameraDistance);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		if (boxCastHit)
		{
			Gizmos.DrawRay(cameraFocusPosition, castDirection * hit.distance);
			Gizmos.DrawWireCube(cameraFocusPosition + castDirection * hit.distance, transform.lossyScale);
		}
		else
		{
			Gizmos.DrawRay(cameraFocusPosition, castDirection * castLine.magnitude);
		}
	}
}
