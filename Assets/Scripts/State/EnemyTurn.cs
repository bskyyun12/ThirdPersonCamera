using System.Collections;
using UnityEngine;

class EnemyTurn : State
{
	public EnemyTurn(BattleSystem battleSystem) : base(battleSystem)
	{
	}

	public override IEnumerator Start()
	{		
		if (true) // if enemy has less than 20% hp
		{
			battleSystem.SetState(new Yield(battleSystem));
		}		

		Debug.Log("Enemy attacks!");

		bool isPlayerDead = false;

		yield return new WaitForSeconds(1f);

		if (isPlayerDead)
		{
			battleSystem.SetState(new Lost(battleSystem));
		}
		else
		{
			battleSystem.SetState(new PlayerTurn(battleSystem));
		}
	}
}