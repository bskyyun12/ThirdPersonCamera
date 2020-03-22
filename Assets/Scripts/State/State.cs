using System.Collections;

public abstract class State 
{
	protected BattleSystem battleSystem;

	public State(BattleSystem battleSystem)
	{
		this.battleSystem = battleSystem;
	}

	public virtual IEnumerator Start()
	{
		yield break;
	}
	public virtual IEnumerator Attack()
	{
		yield break;
	}
	public virtual IEnumerator Heal()
	{
		yield break;
	}
}
