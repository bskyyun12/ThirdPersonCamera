using UnityEngine;

public class CharacterStats : MonoBehaviour
{
	public float maxHp = 100f;
	public float currentHp { get; private set; }

	public Stat damage;
	public Stat armor;	

	private void Awake()
	{
		currentHp = maxHp;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.T))
		{
			TakeDamage(10f);
		}
	}

	public void TakeDamage(float damageIncoming)
	{
		damageIncoming -= armor.GetValue();
		damageIncoming = Mathf.Clamp(damageIncoming, 0f, float.MaxValue);

		currentHp -= damageIncoming;
		Debug.Log(transform.name + " takes " + damageIncoming + " damage.");
		Debug.Log(transform.name + "'s current Hp is " + currentHp + ".");

		if (currentHp <= 0f)
		{
			Die();
		}
	}

	public virtual void Die()
	{
		// Die in some way
		// this method is meant to be overwritten
		Debug.Log(transform.name + " died.");
	}

}
