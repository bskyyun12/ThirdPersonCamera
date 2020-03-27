using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerController))]
public class PlayerInput : MonoBehaviour
{
	private PlayerMovement playerMovement;
	private PlayerController playerController;
	private ThirdPersonCamera playerCamera;

	#region Input Ids
	const string mouseWheelId = "Mouse ScrollWheel";
	const string mouseXId = "Mouse X";
	const string mouseYId = "Mouse Y";
	#endregion Input Ids

	[Header("Input Control")]
	public Controls controls;

	float time;

	private void Awake()
	{
		playerMovement = GetComponent<PlayerMovement>();

		playerController = GetComponent<PlayerController>();
		playerCamera = Camera.main.GetComponent<ThirdPersonCamera>();
	}

	private void Update()
	{
		#region Forwards and Backwards
		// forwards
		if (Input.GetKey(controls.forwards))
			playerMovement.moveInput.y = 1;

		// backwards
		if (Input.GetKey(controls.backwards))
		{
			if (Input.GetKey(controls.forwards))
				playerMovement.moveInput.y = 0;
			else
				playerMovement.moveInput.y = -1;
		}

		// forwards, backwards nothing
		if (!Input.GetKey(controls.forwards) && !Input.GetKey(controls.backwards))
			playerMovement.moveInput.y = 0;
		#endregion Forwards and Backwards

		#region Right and Left
		// Right
		if (Input.GetKey(controls.Right))
			playerMovement.moveInput.x = 1;

		// Left
		if (Input.GetKey(controls.Left))
		{
			if (Input.GetKey(controls.Right))
				playerMovement.moveInput.x = 0;
			else
				playerMovement.moveInput.x = -1;
		}

		// Left, Right nothing
		if (!Input.GetKey(controls.Right) && !Input.GetKey(controls.Left))
			playerMovement.moveInput.x = 0;
		#endregion Right and Left

		#region TurnRight and TurnLeft
		// set time on Keydown
		if (Input.GetKeyDown(controls.turnRight) || Input.GetKeyDown(controls.turnLeft))
		{
			if (Mathf.Approximately(time, Time.time)) // prevent spamming key
			{
				time = Time.time - playerMovement.manualTurnFrequency; // no initial delay
			}
		}

		if (Input.GetKey(controls.turnRight) || Input.GetKey(controls.turnLeft))
		{
			// TurnRight
			if (Input.GetKey(controls.turnRight))
			{
				playerMovement.turnAxis = 1;
			}

			// TurnLeft
			if (Input.GetKey(controls.turnLeft))
			{
				if (Input.GetKey(controls.turnRight))
					playerMovement.turnAxis = 0;
				else
					playerMovement.turnAxis = -1;
			}

			if (Time.time - time >= playerMovement.manualTurnFrequency)
			{
				time = Time.time;
				Vector3 rotation = new Vector3(0, playerMovement.turnAxis * playerMovement.turnDegreePerFrequency, 0);
				playerCamera.ManualRotation(rotation);
				playerMovement.lookRotation.eulerAngles += rotation;
			}
		}

		// TurnRight, TurnLeft nothing
		if (!Input.GetKey(controls.turnRight) && !Input.GetKey(controls.turnLeft))
			playerMovement.turnAxis = 0;
		#endregion TurnRight and TurnLeft

		#region Run
		// Todo: change run function togglable
		if (Input.GetKey(controls.run))
		{
			playerMovement.runInput = true;
			if (playerMovement.moveInput.y < 0)
				playerMovement.runInput = false;
		}

		if (Input.GetKeyUp(controls.run))
			playerMovement.runInput = false;
		#endregion Run

		#region Jump
		if (Input.GetKeyDown(controls.jump))
		{
			playerMovement.jumpInput = true;
		}
		if (Input.GetKeyUp(controls.jump))
		{
			playerMovement.jumpInput = false;
		}
		#endregion Jump

		#region Dash
		//if (Input.GetKeyDown(controls.forwards))
		//{
		//	if (!movement.pressedOnce)
		//	{
		//		movement.pressedOnce = true;
		//		movement.time = Time.time;
		//	}
		//	else
		//	{
		//		movement.dashInput = true;
		//		movement.pressedOnce = false;
		//	}
		//}

		//if (movement.pressedOnce)
		//{
		//	if (Time.time - movement.time > movement.timerLength)
		//	{
		//		movement.pressedOnce = false;
		//	}
		//}

		#endregion Dash

		#region Mouse Drag


		playerMovement.mouseDrag = !Mathf.Approximately(Input.GetAxis(mouseXId), 0f) || !Mathf.Approximately(Input.GetAxis(mouseYId), 0f);
		if (playerMovement.mouseDrag)
		{ playerController.tempFocus = null; }

		#endregion Mouse Drag

		#region MouseClick
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
		{
			// Face the camera's forward
			if (Input.GetMouseButton(1) && playerMovement.mouseDrag)
			{
				Vector3 cameraForward = playerCamera.transform.forward;
				cameraForward.y = 0f;
				playerMovement.lookRotation = Quaternion.LookRotation(cameraForward);
			}
		}

		// Down
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
		{
			playerController.GetTargetUnderMouse();
		}

		// Up
		if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
		{
			playerController.SetFocus();
		}
		#endregion MouseClick

		#region Escape Key
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (playerController.focus)
			{
				playerController.RemoveFocus();
			}
			else
			{
				// Todo: Escape key - open menu/option
			}
		}
		#endregion Escape Key

		// Stop following when the player moves
		if (playerMovement.moveInput != Vector2.zero)
		{
			playerController.isFollowing = false;
		}
	}
}
