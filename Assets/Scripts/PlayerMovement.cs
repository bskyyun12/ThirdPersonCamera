using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
	// Components
	CharacterController controller;

	[Header("Debug")]
	[SerializeField] bool showGroundDirection = true;
	[SerializeField] bool showMoveDirection = true;

	[Header("Input Control")]
	public Vector2 moveInput;
	public float turnAxis;
	public bool runInput = false;
	public bool jumpInput = false;
	public bool mouseDrag = false;
	//[Header("Dash")]
	//public float DashPower = 5f;
	//public bool dashInput = false;
	//public bool pressedOnce = false;
	//public float time = 0f;
	//public float timerLength = .5f;

	[Header("Movement")]
	public bool ControlMovementInAir = false;
	public float minVelocityY = -25f; // related to max fall speed
	[SerializeField, Tooltip("Debug Only")] Vector3 velocity;

	[Header("Speed")]
	[SerializeField] float baseSpeed = 5f;
	public float runSpeed = 10f;
	public bool forwardMultiply = false;
	[SerializeField, Tooltip("Debug Only")] float forwardMultiplier = 1f;
	public float currentSpeed = 0f;

	[Header("Turn")]
	public float turnSpeed = 5f;
	public Quaternion lookRotation;
	float time;
	public float manualTurnFrequency = .5f;
	public float turnDegreePerFrequency = 90f;

	[Header("Jump")]
	public float jumpHeight = 5f;
	[SerializeField, Tooltip("Debug Only")] bool isJumping = false;

	[Header("Gravity")]
	public float gravityScale = 2.5f;
	float gravity = Physics.gravity.y;

	[Header("Slope")]
	[SerializeField, Tooltip("Debug Only")] bool isSliding = false;
	[SerializeField, Tooltip("Debug Only")] bool onSlope = false;
	[SerializeField, Tooltip("Debug Only")] float fallSpeed;
	[SerializeField, Tooltip("Debug Only")] float slopeAngle;

	[Header("Ground")]
	public float groundRayDistance = .2f;
	Ray groundRay;
	RaycastHit groundHit;
	Vector3 collisionPoint;
	Vector3 groundUpDir;
	Vector3 groundRightDir;
	Vector3 groundForwardDir;
	Vector3 moveForwardDir;
	Vector3 moverightDir;

	[Header("Impact")]
	public float impactDamping = 5f;
	Vector3 impact = Vector3.zero;

	// Follow
	public Vector3 followDir;
	
	// lecture
	float Lerp(float a, float b, float t) => (1 - t) * a + b * t;
	float InverseLerp(float a, float b, float v) => (v - a) / (b - a);

	float Remap(float iMin, float iMax, float oMin, float oMax, float v)
	{
		float t = InverseLerp(iMin, iMax, v);
		return Lerp(oMin, oMax, t);
	}

	// Volume InvLerp(20,10,distance) = t
	// 10 - 1, 20 - 0

	private void Awake()
	{
		controller = GetComponent<CharacterController>();
	}

	void Update()
	{
		Move();
	}

	void Move()
	{
		GroundDirection();
		isSliding = slopeAngle >= controller.slopeLimit;
		onSlope = groundHit.normal != Vector3.up;

		#region Calculate Speed
		if (controller.isGrounded && !isSliding)
		{
			if (moveInput != Vector2.zero || followDir != Vector3.zero)
			{
				currentSpeed = baseSpeed;
			}
			else
			{
				currentSpeed = 0f;
			}

			if (runInput)
			{ currentSpeed = runSpeed; }
		}
		#endregion Calculate Speed

		#region Apply Gravity
		if (!controller.isGrounded || isSliding)
		{
			if (velocity.y >= minVelocityY)
			{
				velocity.y += gravity * gravityScale * Time.deltaTime;
				fallSpeed = Mathf.Abs(velocity.y);
			}
		}
		#endregion Apply Gravity

		#region Jump
		// Jump
		if (jumpInput && controller.isGrounded && !isSliding)
		{
			isJumping = true;
			velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
			onSlope = false;
		}
		#endregion Jump

		#region Impact handle
		// Impact handle
		if (impact.magnitude > .2f)
		{
			impact = Vector3.Lerp(impact, Vector3.zero, impactDamping * Time.deltaTime);
			velocity += impact;
		}
		#endregion Impact handle

		#region Calculate Velocity
		// Disable move control on sliding
		if (isSliding)
		{ moveInput = Vector2.zero; }

		float yStore = velocity.y;
		if (ControlMovementInAir)
		{
			velocity = (moveForwardDir * moveInput.y + moverightDir * moveInput.x + followDir) *
						(currentSpeed * forwardMultiplier) + groundForwardDir * fallSpeed;
		}
		else
		{
			if (controller.isGrounded)
			{
				velocity = (moveForwardDir * moveInput.y + moverightDir * moveInput.x + followDir) *
							(currentSpeed * forwardMultiplier) + groundForwardDir * fallSpeed;
			}
		}

		// Assign stored velocity.y only on ground or sharp slope
		if (!onSlope || (onSlope && isSliding))
		{ velocity.y = yStore; }
		#endregion Calculate Velocity

		// Move
		controller.Move(velocity * Time.deltaTime);

		// Turn
		transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);

		// Ground Check
		if (controller.isGrounded)
		{
			if (!isSliding)
			{
				velocity.y = -2f;
				fallSpeed = 0f;
			}

			if (isJumping)
			{ isJumping = false; }

			// Dash
			//if (dashInput)
			//{
			//	Debug.Log("Dash! - Needs to implement");
			//	dashInput = false;
			//	//AddImpact(transform.forward, DashPower);
			//}
		}
	}

	void GroundDirection()
	{
		groundRay.origin = transform.position + collisionPoint + Vector3.up * 0.05f;
		groundRay.direction = Vector3.down;

		if (Physics.Raycast(groundRay, out groundHit, controller.height * .5f * groundRayDistance))
		{
			groundUpDir = groundHit.normal;
			groundRightDir = Vector3.Cross(groundUpDir, groundRay.direction);
			groundForwardDir = Vector3.Cross(groundRightDir, groundUpDir);
			moveForwardDir = Vector3.Cross(transform.right, groundUpDir);
			moverightDir = Vector3.Cross(groundUpDir, moveForwardDir);

			// Acos(Dot(a,b)) gives a radian angle between a and b.			
			float slopeAngleInRad = Mathf.Acos(Vector2.Dot(Vector3.up, groundHit.normal));
			slopeAngle = slopeAngleInRad * Mathf.Rad2Deg;
			// slopeAngle = Vector3.Angle(Vector3.up, groundHit.normal); // or simply do this

			// Setting forwardMultiplier, faster downhill and slower uphill
			if (forwardMultiply)
			{
				float dot = Vector3.Dot(transform.forward, groundForwardDir);
				forwardMultiplier = 1f + moveInput.y * dot;
			}
		}
	}

	public void AddImpact(Vector3 direction, float force)
	{
		direction.Normalize();
		//if (direction.y < 0)
		//{
		//	direction.y *= -1f;
		//}
		impact += direction * force; // devide mass after multiplying force if there is a mass
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		collisionPoint = hit.point;
		collisionPoint = (collisionPoint - transform.position);
	}

	private void OnDrawGizmos()
	{
		// ground check
		Gizmos.color = Color.red;
		Gizmos.DrawRay(groundRay.origin, groundRay.direction * groundRayDistance);

		if (showGroundDirection)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(groundHit.point, groundHit.point + groundUpDir);

			Gizmos.color = Color.red;
			Gizmos.DrawLine(groundHit.point, groundHit.point + groundRightDir);

			Gizmos.color = Color.blue;
			Gizmos.DrawLine(groundHit.point, groundHit.point + groundForwardDir);
		}

		if (showMoveDirection)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(groundHit.point, groundHit.point + groundUpDir);

			Gizmos.color = Color.red;
			Gizmos.DrawLine(groundHit.point, groundHit.point + moverightDir);

			Gizmos.color = Color.blue;
			Gizmos.DrawLine(groundHit.point, groundHit.point + moveForwardDir);
		}
	}

}
