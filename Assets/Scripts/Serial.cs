using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

public class Serial : MonoBehaviour
{
	public delegate void SerialDataReceivedEventHandler(string message);
	public event SerialDataReceivedEventHandler OnDataReceived;

	public string portName = "COM5";
	public int baudRate = 115200;
	private SerialPort serialPort_;
	private Thread thread_;
	private bool isRunning_ = false;

	private string message_;
	private bool isNewMessageReceived_ = false;

	// Use this for initialization
	void Start()
	{
		Open();
	}

	// Update is called once per frame
	void Update()
	{
		if (isNewMessageReceived_)
		{
			Debug.Log($"Serial: Message received = {message_}");
			isNewMessageReceived_ = false;
			if (OnDataReceived != null)
			{
				OnDataReceived(message_);
			}
		}
	}
	void OnDestroy()
	{
		Close();
	}
	private void Open()
	{
		serialPort_ = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
		//‚Ü‚½‚Í
		//serialPort_ = new SerialPort(portName, baudRate);
		serialPort_.ReadTimeout = 500;
		serialPort_.Open();

		isRunning_ = true;

		thread_ = new Thread(Read);
		thread_.Start();
	}

	private void Close()
	{
		isNewMessageReceived_ = false;
		isRunning_ = false;

		if (thread_ != null && thread_.IsAlive)
		{
			thread_.Join();
		}

		if (serialPort_ != null && serialPort_.IsOpen)
		{
			serialPort_.Close();
			serialPort_.Dispose();
		}
	}

	private void Read()
	{
		while (isRunning_ && serialPort_ != null && serialPort_.IsOpen)
		{
			try
			{
				message_ = serialPort_.ReadLine();
				isNewMessageReceived_ = true;
			}
			catch (System.Exception e)
			{
				Debug.LogWarning(e.Message);
			}
		}
	}

	public void Write(string message)
	{
		Debug.Log(message);
		try
		{
			serialPort_.Write(message);
			Debug.Log("Serial signal sent: " + message);
		}
		catch (System.Exception e)
		{
			Debug.LogWarning(e.Message);
		}
	}
}

