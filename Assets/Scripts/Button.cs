using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    // Start is called before the first frame update
    public Serial serialHandler;
    public GameObject targetSlotObject;
    //public slot[] slot_ = new slot[3];
    private int button, button_temp, state;
    // Update is called once per frame
    public void OnButtonPressed()
    {
        slot slotScript = targetSlotObject.GetComponent<slot>();
        if(slotScript != null)
        {
            slotScript.Stop();
        }
    }
    /*void Start()
    {
        button = 0;
        button_temp = 0;
        state = 0;
        serialHandler.OnDataReceived += OnDataReceived;
    }
    void Update()
    {
        button_temp = button;
        if(button_temp == 0 && button == 1 && state == 0)
        {
            slot_[0].Stop();
            state += 1;
        }
        if (button_temp == 0 && button == 1 && state == 0)
        {
            slot_[1].Stop();
            state += 1;
        }
        if (button_temp == 0 && button == 1 && state == 0)
        {
            slot_[2].Stop();
            state += 1;
        }
        if (button_temp == 0 && button == 1 && state == 0)
        {
            for (int i = 0;i < 3; i++)
            {
                slot_[i].Restart();
            }
            state += 1;
        }
        if (state >= 4)
        {
            state = 0;
        }
    }
    private void OnDataReceived(string message)
    {
        Debug.Log("Received from Serial: " + message);
        if (message == "1")
        {
            button = 1;
        }
        else
        {
            button = 0;
        }
    }
    */
}
