using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour
{
	PlayerMovement playerMovement;
	private Camera playerCamera;

	[Header("Interact")]
	public Interactable focus;
	public Interactable tempFocus;
	float timeLimitToGetFocus = 0.5f;
	float timerToGetFocus;

	public bool isFollowing = false;
	private Coroutine followCoroutine;
	private float followingDelay = .2f;

	private void Awake()
	{
		playerMovement = GetComponent<PlayerMovement>();
		playerCamera = Camera.main;
	}

	public void SetFocus()
	{
		if (Time.time - timerToGetFocus <= timeLimitToGetFocus && tempFocus)
		{
			if (tempFocus != focus)
			{
				if (focus)
				{ focus.OnDefocused(); }

				focus = tempFocus;
				tempFocus = null;
			}

			focus.OnFocused(gameObject);
		}
	}

	public void RemoveFocus()
	{
		if (focus)
		{ focus.OnDefocused(); }

		focus = null;
	}

	public void StartFollowing()
	{
		isFollowing = true;
		followCoroutine = StartCoroutine(FollowTarget());
	}

	public void StopFollowing()
	{
		isFollowing = false;
		playerMovement.followDir = Vector3.zero;
		StopCoroutine(followCoroutine);
	}

	private IEnumerator FollowTarget()
	{
		while (true)
		{
			Follow();
			yield return new WaitForSeconds(followingDelay);
		}
	}

	private void Follow()
	{
		if (!isFollowing || focus == null)
		{
			StopFollowing();
			return;
		}

		// Set y values 0f - So the player won't try to reach a target in different height
		Vector3 focusGroundPos = focus.transform.position;
		focusGroundPos.y = 0f;
		Vector3 playerGroundPos = transform.position;
		playerGroundPos.y = 0f;

		// Set FollowDir until it reaches the Target
		playerMovement.followDir = (focusGroundPos - playerGroundPos).normalized;
		float followDist = focus.radius * .8f; // Make sure the player goes into the focus' radius to Interact with
		if (Vector3.Distance(focusGroundPos, playerGroundPos) <= followDist)
		{ playerMovement.followDir = Vector3.zero; }

		// Check if the player is looking at the Target already
		float forwardDot = Vector3.Dot(transform.forward, (focusGroundPos - playerGroundPos).normalized);
		forwardDot = Mathf.Clamp(forwardDot, -1f, 1f);
		if (!Mathf.Approximately(forwardDot, 1f))
		{
			// Get turnAngle
			float turnAngleInRad = Mathf.Acos(forwardDot);
			float turnAngleInDeg = turnAngleInRad * Mathf.Rad2Deg;

			// Check if the Target is on left side or right side. if rightDot is minus, the target is on the left side
			Vector3 rotation = new Vector3(0, turnAngleInDeg, 0);
			float rightDot = Vector3.Dot(transform.right, (focusGroundPos - playerGroundPos).normalized);
			playerMovement.lookRotation.eulerAngles += rotation * Mathf.Sign(rightDot);
		}
	}

	public void GetTargetUnderMouse()
	{
		Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 100f))
		{
			tempFocus = hit.collider.GetComponent<Interactable>();
			if (tempFocus)
			{ timerToGetFocus = Time.time; }
		}
	}
}
