using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flash : MonoBehaviour
{ 
    private float lightStrength;
    private bool maxStrength;
    // Start is called before the first frame update
    void Start()
    {
        lightStrength = GetComponent<Light>().intensity;
        lightStrength = 0;
        maxStrength = false;
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Light>().intensity = lightStrength;
        if (10 <= lightStrength)
        {
            maxStrength = true;
        }

        if (lightStrength <= 0)
        {
            maxStrength = false;
        }

        if (maxStrength)
        {
            lightStrength -= 20 * Time.deltaTime;
        }
        else
        {
            lightStrength += 20 * Time.deltaTime;
        }
    }
}
