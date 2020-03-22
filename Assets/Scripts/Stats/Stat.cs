using UnityEngine;

[System.Serializable]
public class Stat
{
	[SerializeField]
	private float baseValue = 0f;

	public float GetValue()
	{
		return baseValue;
	}
}
