using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControls : MonoBehaviour
{
	// Components
	CharacterController controller;
	public Transform groundDirection, fallDirection; // Todo: merge these two in one?

	[Header("Debug")]
	public bool showGroundNormal = true;
	public bool showFallNormal = true;

	[Header("Input Control")]
	public Controls controls;
	Vector2 moveInput;
	float turnAxis;
	bool runInput = false;
	bool jumpInput = false;
	bool dashInput = false;

	[Header("Movement")]
	public bool ControlMovementInAir = false;
	public float minVelocityY = -25f; // related to max fall speed
	[SerializeField, Tooltip("Debug Only")] Vector3 velocity;

	[Header("Speed")]
	[SerializeField] float baseSpeed = 5f;
	public float runSpeed = 10f;
	public float turnSpeed = 1f;
	[SerializeField, Tooltip("Debug Only")] float currentSpeed = 0f;

	[Header("Jump")]
	public float jumpHeight = 5f;
	[SerializeField, Tooltip("Debug Only")] bool isJumping = false;

	[Header("Dash")]
	public float DashPower = 5f;
	bool pressedOnce = false;
	float time = 0f;
	float timerLength = .5f;

	[Header("Gravity")]
	public float gravityScale = 5f;
	float gravity = Physics.gravity.y;

	// Slope
	[SerializeField, Tooltip("Debug Only")] bool isSliding = false;
	[SerializeField, Tooltip("Debug Only")] bool onSlope = false;
	[SerializeField, Tooltip("Debug Only")] float fallSpeed;

	// Ground
	public float groundRayDistance = .2f;
	Ray groundRay;
	RaycastHit groundHit;
	Vector3 forwardDirection;
	Vector3 collisionPoint;
	[SerializeField, Tooltip("Debug Only")] float slopeAngle;
	[SerializeField, Tooltip("Debug Only")] float forwardAngle;
	[SerializeField, Tooltip("Debug Only")] float forwardMultiplier;

	// Impact
	public float impactDamping = 5f;
	Vector3 impact = Vector3.zero;

	private void Awake()
	{
		controller = GetComponent<CharacterController>();
	}

	// Update is called once per frame
	void Update()
	{
		GetInput();
		Move();
	}

	void Move()
	{
		GroundDirection();
		isSliding = slopeAngle > controller.slopeLimit;
		onSlope = groundHit.normal != Vector3.up;

		#region Calculate Speed
		if (controller.isGrounded && !isSliding)
		{
			currentSpeed = baseSpeed;

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

		// Jump
		if (jumpInput && controller.isGrounded && !isSliding)
		{
			isJumping = true;
			velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
			onSlope = false;
		}

		// Impact handle
		if (impact.magnitude > .2f)
		{
			impact = Vector3.Lerp(impact, Vector3.zero, impactDamping * Time.deltaTime);
			velocity += impact;
		}

		#region Calculate Velocity
		// Disable move control on sliding
		if (isSliding)
		{ moveInput = Vector2.zero; }

		float yStore = velocity.y;
		if (ControlMovementInAir)
		{
			velocity = (groundDirection.forward * moveInput.y + groundDirection.right * moveInput.x) *
						(currentSpeed * forwardMultiplier) + -fallDirection.up * fallSpeed;
		}
		else
		{
			if (controller.isGrounded)
			{
				velocity = (groundDirection.forward * moveInput.y + groundDirection.right * moveInput.x) *
							(currentSpeed * forwardMultiplier) + -fallDirection.up * fallSpeed;
			}
		}

		// Assign stored velocity.y only on ground or sharp slope
		if (!onSlope || (onSlope && isSliding))
		{ velocity.y = yStore; }
		#endregion Calculate Velocity

		// Move
		controller.Move(velocity * Time.deltaTime);

		// Turn
		if (turnAxis != 0)
		{ Turn(); }

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
			if (dashInput)
			{
				Debug.Log("Dash! - Needs to implement");
				dashInput = false;
				//AddImpact(transform.forward, DashPower);
			}
		}
	}

	void GroundDirection()
	{
		forwardDirection = transform.position + transform.forward;
		forwardDirection.y -= controller.height * .5f;

		groundDirection.LookAt(forwardDirection);
		fallDirection.rotation = transform.rotation;

		groundRay.origin = transform.position + collisionPoint + Vector3.up * 0.05f;
		groundRay.direction = Vector3.down;

		forwardMultiplier = 1f;

		if (Physics.Raycast(groundRay, out groundHit, controller.height * .5f * groundRayDistance))
		{
			slopeAngle = Vector3.Angle(transform.up, groundHit.normal);
			forwardAngle = Vector3.Angle(groundDirection.forward, groundHit.normal) - 90f;

			// Rotate groundDirection
			groundDirection.eulerAngles += new Vector3(-forwardAngle, 0f, 0f);

			// When going down normal slope
			if (forwardAngle < 0f && slopeAngle <= controller.slopeLimit)
			{
				forwardMultiplier = 1f / Mathf.Cos(forwardAngle * Mathf.Deg2Rad);
			}
			// When going down sharp slope
			else if (slopeAngle > controller.slopeLimit)
			{
				float groundDistance = Vector3.Distance(groundRay.origin, groundHit.point);

				if (groundDistance <= .1f)
				{
					Vector3 groundCross = Vector3.Cross(groundHit.normal, Vector3.up);
					Vector3 slopeCross = Vector3.Cross(groundCross, groundHit.normal);
					fallDirection.rotation = Quaternion.FromToRotation(transform.up, slopeCross);
				}
			}
			// Todo: should the player be slower when going up slope?
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

	private void Turn()
	{
		Vector3 characterRotation = transform.eulerAngles + new Vector3(0, turnAxis * turnSpeed, 0);
		transform.eulerAngles = characterRotation;
	}

	void GetInput()
	{
		#region Forwards and Backwards
		// forwards
		if (Input.GetKey(controls.forwards))
			moveInput.y = 1;

		// backwards
		if (Input.GetKey(controls.backwards))
		{
			if (Input.GetKey(controls.forwards))
				moveInput.y = 0;
			else
				moveInput.y = -1;
		}

		// forwards, backwards nothing
		if (!Input.GetKey(controls.forwards) && !Input.GetKey(controls.backwards))
			moveInput.y = 0;
		#endregion Forwards and Backwards

		#region Right and Left
		// Right
		if (Input.GetKey(controls.Right))
			moveInput.x = 1;

		// Left
		if (Input.GetKey(controls.Left))
		{
			if (Input.GetKey(controls.Right))
				moveInput.x = 0;
			else
				moveInput.x = -1;
		}

		// Left, Right nothing
		if (!Input.GetKey(controls.Right) && !Input.GetKey(controls.Left))
			moveInput.x = 0;
		#endregion Right and Left

		#region TurnRight and TurnLeft
		// TurnRight
		if (Input.GetKey(controls.turnRight))
			turnAxis = 1;

		// TurnLeft
		if (Input.GetKey(controls.turnLeft))
		{
			if (Input.GetKey(controls.turnRight))
				turnAxis = 0;
			else
				turnAxis = -1;
		}

		// TurnRight, TurnLeft nothing
		if (!Input.GetKey(controls.turnRight) && !Input.GetKey(controls.turnLeft))
			turnAxis = 0;
		#endregion TurnRight and TurnLeft

		#region Run
		if (Input.GetKey(controls.run))
		{
			runInput = true;
			if (moveInput.y < 0)
				runInput = false;
		}

		if (Input.GetKeyUp(controls.run))
			runInput = false;
		#endregion Run

		#region Jump
		if (Input.GetKeyDown(controls.jump))
		{
			jumpInput = true;
		}
		if (Input.GetKeyUp(controls.jump))
		{
			jumpInput = false;
		}
		#endregion Jump

		#region Dash
		if (Input.GetKeyDown(controls.forwards))
		{
			if (!pressedOnce)
			{
				pressedOnce = true;
				time = Time.time;
			}
			else
			{
				dashInput = true;
				pressedOnce = false;
			}
		}

		if (pressedOnce)
		{
			if (Time.time - time > timerLength)
			{
				pressedOnce = false;
			}
		}

		#endregion Dash
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

		// ground direction(character's move forward direction)
		if (showGroundNormal)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawRay(groundDirection.position, groundDirection.forward);
		}

		// fall direction
		if (showFallNormal)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawRay(fallDirection.position, -fallDirection.up);
		}
	}
}
