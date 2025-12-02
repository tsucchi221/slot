using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialLight : MonoBehaviour
{

	public SerialMaster serialHandler;

	// Use this for initialization
	void Start()
	{
		serialHandler.OnDataReceived += OnDataReceived;
	}

	// Update is called once per frame
	void Update()
	{
	}

	void OnDataReceived(string message)
	{
		try
		{
			Debug.Log(message); // シリアルで受信した値をデバッグログに表示
		}
		catch (System.Exception e)
		{
			Debug.LogWarning(e.Message);
		}
	}
}
