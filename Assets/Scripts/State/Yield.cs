using System.Collections;
using UnityEngine;

internal class Yield : State
{
	public Yield(BattleSystem battleSystem) : base(battleSystem)
	{
	}

	public override IEnumerator Start()
	{
		Debug.Log("Enemy has yielded!");
		yield break;
	}
}