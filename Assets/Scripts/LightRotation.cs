using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightRotation : MonoBehaviour
{

    public int speed = 1;
    Light rotLight;

    // Start is called before the first frame update
    void Start()
    {
        rotLight = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        //Decreases the intensity of light as the sun sets, and increases it when the sun rises.
        if (this.transform.eulerAngles.x > 260 && this.transform.eulerAngles.x < 350)
        {
            if (rotLight.intensity != 0)
            {
                rotLight.intensity = rotLight.intensity - 0.001f;
            }
        }
        else if (rotLight.intensity !< 1)
        {
            rotLight.intensity = rotLight.intensity + 0.001f;
        }

        //Rotates the sun
        this.transform.Rotate(0, 0.001f * speed, 0);
    }
}
