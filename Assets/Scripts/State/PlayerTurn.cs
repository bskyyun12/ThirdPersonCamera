using System.Collections;
using UnityEngine;

public class PlayerTurn : State
{
	public PlayerTurn(BattleSystem battleSystem) : base(battleSystem)
	{
	}

	public override IEnumerator Start()
	{
		Debug.Log("Choose an action");
		yield break;
	}

	public override IEnumerator Attack()
	{
		bool isDead = false;

		yield return new WaitForSeconds(1f);

		if (isDead)
		{
			battleSystem.SetState(new Won(battleSystem));
		}
		else
		{
			battleSystem.SetState(new EnemyTurn(battleSystem));
		}
	}

	public override IEnumerator Heal()
	{
		Debug.Log("Heal!");

		yield return new WaitForSeconds(1f);

		battleSystem.SetState(new EnemyTurn(battleSystem));
	}
}
