using System.Collections;
using UnityEngine;

internal class Won : State
{
	public Won(BattleSystem battleSystem) : base(battleSystem)
	{
	}

	public override IEnumerator Start()
	{
		Debug.Log("You Won!");
		yield break;
	}
}