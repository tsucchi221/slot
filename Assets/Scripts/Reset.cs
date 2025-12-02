using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reset : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] targetSlotObject;
    // Update is called once per frame
    public void OnButtonPressed()
    {
        slot slotScript1 = targetSlotObject[0].GetComponent<slot>();
        slot slotScript2 = targetSlotObject[1].GetComponent<slot>();
        slot slotScript3 = targetSlotObject[2].GetComponent<slot>();
        Handle handle = targetSlotObject[3].GetComponent<Handle>();
        if (slotScript1 != null)
        {
            slotScript1.Restart();
        }
        if (slotScript2 != null)
        {
            slotScript2.Restart();
        }
        if (slotScript3 != null)
        {
            slotScript3.Restart();
        }
        if (handle != null)
        {
            handle.HandleMove();
        }
    }
}
