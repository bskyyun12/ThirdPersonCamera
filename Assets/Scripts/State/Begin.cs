using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Begin : State
{
	public Begin(BattleSystem battleSystem) : base(battleSystem)
	{
	}

	public override IEnumerator Start()
	{
		Debug.Log("Battle Start!");
		yield return new WaitForSeconds(2f);

		battleSystem.SetState(new PlayerTurn(battleSystem));
	}
}

