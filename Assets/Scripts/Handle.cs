using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Handle : MonoBehaviour
{
    float rotation;
    int counter;
    // Start is called before the first frame update
    void Start()
    {
        rotation = 0.0f;
        counter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Transform myTransform = this.transform;

        if (counter < 40 && rotation > 0.4f)
        {
            myTransform.Rotate(-rotation, 0.0f, 0.0f);
            counter += 1;
        }
        if (counter >= 40 && rotation > 0.4f)
        {
            myTransform.Rotate(rotation, 0.0f, 0.0f);
            counter += 1;
        }
        if (counter >= 80)
        {
            counter = 0;
            rotation = 0.0f;
        }
    }
    public void HandleMove()
    {
        rotation = 1.0f;
    }
}