using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControls : MonoBehaviour
{
	// Components
	CharacterController controller;

	[Header("Input Control")]
	public Controls controls;
	Vector2 moveInput;
	float turnAxis;
	bool runInput = false;
	bool jumpInput = false;

	[Header("Movement")]
	public bool ControlMovementInAir = false;
	Vector3 velocity;

	[Header("Speed")]
	[SerializeField] float baseSpeed = 1f;
	public float runSpeed = 3;
	float currentSpeed = 0f;
	public float turnSpeed = 1f;

	[Header("Jump")]
	public float jumpPower = 5f;

	[Header("Gravity")]
	public float gravityScale = 5f;

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
		if (turnAxis != 0)
		{ Turn(); }

		GetVelocity();

		// Ground Check
		if (controller.isGrounded)
		{
			// Jump
			if (jumpInput)
			{ velocity.y = jumpPower; }
		}
		else
		{
			// Apply gravity
			velocity.y += Physics.gravity.y * gravityScale * Time.deltaTime;
		}

		// Move
		controller.Move(velocity * Time.deltaTime);
	}

	private void Turn()
	{
		Vector3 characterRotation = transform.eulerAngles + new Vector3(0, turnAxis * turnSpeed, 0);
		transform.eulerAngles = characterRotation;
	}

	/// <summary>
	/// Get velocity with moveInput and currentSpeed.
	/// This should be called before modifying velocity.y value.
	/// </summary>
	private void GetVelocity()
	{
		GetSpeed();
		if (ControlMovementInAir)
		{
			float yStore = velocity.y;
			velocity = (transform.forward * moveInput.y +
						transform.right * moveInput.x);
			velocity = velocity.normalized * currentSpeed;
			velocity.y = yStore;
		}
		else
		{
			if (controller.isGrounded)
			{
				velocity = (transform.forward * moveInput.y +
							transform.right * moveInput.x);
				velocity = velocity.normalized * currentSpeed;
			}
		}
	}

	private void GetSpeed()
	{
		currentSpeed = baseSpeed;
		if (runInput)
		{ currentSpeed = runSpeed; }

		// Speed decrease on walking backwards
		//if (inputs.y < 0)
		//	currentSpeed *= .5f;
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
	}
}
