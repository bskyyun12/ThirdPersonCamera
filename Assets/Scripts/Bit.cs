using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bit : MonoBehaviour
{
	//private int foo; // Heap allocation

	//public void Test()
	//{
	//	int foo2 = 0; // stack allocation
	//}

	/*
	 * Bit operators:
	 * |=	- ADD
	 * &= ~ - Remove
	 * ~	- Reverse
	 * &	- And
	 * <<	- bit shift left
	 * &=	- shift bit on and off (XOR)
	*/

	private int flyMagic = 1 << 0;
	private int fireMagic = 1 << 1;

	private int magic;


	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Q))
		{
			AddMagic(fireMagic);
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			AddMagic(flyMagic);
		}
		if (Input.GetKeyDown(KeyCode.N))
		{
			RemoveMagic(flyMagic);
		}
		if (Input.GetKeyDown(KeyCode.L))
		{
			if ((magic & flyMagic) == flyMagic)
			{
				Debug.Log("Has flying");
				PrintAttributes();
			}
		}
		if (Input.GetKeyDown(KeyCode.K))
		{
			// add all
			magic |= (flyMagic | fireMagic);
			PrintAttributes();
		}

		if (Input.GetKeyDown(KeyCode.J))
		{
			// remove all
			magic &= ~(flyMagic | fireMagic);
			PrintAttributes();
		}
		if (Input.GetKeyDown(KeyCode.A))
		{
			magic ^= flyMagic;
			PrintAttributes();
		}
	}

	public void AddMagic(int magicToAdd)
	{
		magic |= magicToAdd;
		PrintAttributes();
	}

	public void RemoveMagic(int magicToRemove)
	{
		magic &= ~magicToRemove;
		PrintAttributes();
	}

	public void HasMagic(int magicToCheck)
	{

	}
	
	private void PrintAttributes()
	{
		Debug.Log(Convert.ToString(magic, 2).PadLeft(8, '0'));
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

}
