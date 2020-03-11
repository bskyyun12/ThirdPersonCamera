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
	Quaternion cameraRotation;
	Vector2 cameraEulerAngle = new Vector2(45f, 0f);
	Vector3 cameraDirection;
	Vector3 cameraFocusPosition;
	Vector3 desiredCameraPosition;

	// Camera Collision
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
	[SerializeField, Range(1f, 20f)] float maxCameraDistance = 15f;

	[Header("Layer Filtering")]
	[SerializeField] LayerMask whatToDetect = -1;

	[Header("Rotation")]
	[SerializeField] bool reverseYAxis = true;
	[SerializeField, Range(-89f, 89f)] float minVerticalAngle = -30f;
	[SerializeField, Range(-89f, 89f)] float maxVerticalAngle = 80f;
	[SerializeField, Range(1f, 360f)] float rotateSpeed = 180f;

	[Header("Zoom")]
	[SerializeField] bool reverseZoom = true;
	[SerializeField, Range(.01f, .5f)] float lerpSpeed = .1f;
	[SerializeField, Range(1f, 50f)] float zoomSpeed = 10f;

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
		cameraRotation = Quaternion.Euler(cameraEulerAngle);
	}

	// Make sure the camera movement happens after player's movement. Otherwise, the camera will jiggle.
	void LateUpdate()	
	{
		// Manual rotation
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
		{ ManualRotation(); }

		// Zoom
		if (Input.GetAxis(mouseWheelId) != 0f)
		{ Zoom(); }

		cameraFocusPosition = cameraFocus.transform.position;
		cameraDirection = cameraRotation * Vector3.forward;

		boxCastHit = BoxCastFromTargetToCamera();

		// When camera is too close -> change view to first person
		if (boxCastHit && hit.distance < minCameraDistance)
		{ hit.distance = 0f; }

		// Smooth zoom-in and out on collision
		currentCameraDistance = Vector3.Distance(cameraFocusPosition, transform.position);
		currentCameraDistance = Mathf.Lerp(
							currentCameraDistance,
							(boxCastHit ? hit.distance : desiredCameraDistance),
							lerpSpeed);
		desiredCameraPosition = cameraFocusPosition - cameraDirection * currentCameraDistance;

		transform.SetPositionAndRotation(desiredCameraPosition, cameraRotation);
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
					cameraRotation,
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
		Vector2 mouseInput = new Vector2(
			(reverseYAxis ? -Input.GetAxis(mouseYId) : Input.GetAxis(mouseYId)),
			Input.GetAxis(mouseXId));

		cameraEulerAngle += rotateSpeed * Time.unscaledDeltaTime * mouseInput;
		ConstrainAngles();

		cameraRotation = Quaternion.Euler(cameraEulerAngle);

		if (Input.GetMouseButton(1))
		{
			Vector3 cameraForward = transform.forward;
			cameraForward.y = 0f;
			player.transform.forward = cameraForward;
		}
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
