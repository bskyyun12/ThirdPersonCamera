using UnityEngine;

public class Interactable : MonoBehaviour
{
	public float radius = 3f;
	public Transform interactionTransform => transform;

	bool isFocus = false;
	GameObject player;

	bool hasInteracted = false;

	public bool isItem = false;

	public virtual void Interact()
	{
		// This method is meant to be overwritten
		Debug.Log("Interacting with " + interactionTransform.name);
	}

	private void Update()
	{
		if (isFocus && !hasInteracted)
		{
			float distance = Vector3.Distance(player.transform.position, interactionTransform.position);
			if (distance <= radius)
			{
				Interact();
				hasInteracted = true;
			}
		}
	}

	public virtual void OnFocused(GameObject playerObject)
	{
		Debug.Log("Current Focus: " + gameObject.name);
		isFocus = true;
		player = playerObject;
		hasInteracted = false;
	}

	public virtual void OnDefocused()
	{
		isFocus = false;
		player = null;
		hasInteracted = false;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(interactionTransform.position, radius);
	}
}
