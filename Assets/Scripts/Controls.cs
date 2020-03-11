using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Controls
{
	public KeyCode forwards = KeyCode.W,
				   backwards = KeyCode.S,
				   Left = KeyCode.A,
				   Right = KeyCode.D,
				   turnLeft = KeyCode.Q,
				   turnRight = KeyCode.E,
				   run = KeyCode.LeftShift,
				   jump = KeyCode.Space;
}
