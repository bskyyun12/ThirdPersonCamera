using System;
using UnityEngine;

public class ItemPickup : Interactable
{
	PlayerController playerController;

	private void Start()
	{
		isItem = true;
	}

	public override void Interact()
	{
		base.Interact();
		Pickup();
	}

	private void Pickup()
	{
		Debug.Log("Picking up item");
		Destroy(gameObject);
	}

	public override void OnFocused(GameObject playerObject)
	{
		base.OnFocused(playerObject);
		playerController = playerObject.GetComponent<PlayerController>();
		playerController.StartFollowing();
	}

	public override void OnDefocused()
	{
		base.OnDefocused();
		playerController.StopFollowing();
	}
}
