using System.Collections;

using System.Collections.Generic;
using UnityEngine;

public class SerialReset : MonoBehaviour
{

	/* SerialMasterオブジェクトのSerialMaster.csを参照する */
	public SerialMaster serialHandler;

	void Start()
	{
	}

	void Update()
	{
		serialHandler.Write("0");
	}
}
