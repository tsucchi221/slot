using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cutinFlash : MonoBehaviour
{
    private float lightStrength;
    private int count;
    private int flg;
    public float maxLight;
    // Start is called before the first frame update
    void Start()
    {
        lightStrength = GetComponent<Light>().intensity;
        lightStrength = 0;
        count = 0;
    }

    // Update is called once per frame
    void Update()
    {
        count += 1;
        if (count >= 60)
            count = 0;

        lightStrength = count >= 30 && flg == 1 ? maxLight : 0f;
        GetComponent<Light>().intensity = lightStrength;
    }

    public void On()
    {
        flg = 1;
    }
    public void Off()
    {
        flg = 0;
    }
}
