using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : StateMachine
{
	private void Start()
	{
		SetState(new Begin(this));
	}

	public void OnAttackButton()
	{
		StartCoroutine(currentState.Attack());
	}

	public void OnHealButton()
	{
		StartCoroutine(currentState.Heal());
	}
}
