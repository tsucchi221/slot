// SerialSend.cs
using UnityEngine;

public class SerialSend : MonoBehaviour
{
    public SerialMaster serialMaster; // Inspectorでセット
    public int numberToSend = 42;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendNumber(numberToSend);
        }
    }

    public void SendNumber(int number)
    {
        if (serialMaster != null && serialMaster.IsPortOpen())
        {
            serialMaster.Write(number.ToString() + "\n"); // 改行つけるとESP32でReadLineしやすい
            Debug.Log("Sent: " + number);
        }
        else
        {
            Debug.LogWarning("Serial port is not open.");
        }
    }
}
