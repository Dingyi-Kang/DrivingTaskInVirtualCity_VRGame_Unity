using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class carsSoundEffectScript : MonoBehaviour
{

    public AudioSource runningSound;

    public float maxPitch = 4f;
    public float minPitch = 0.5f;

    void Start()
    {
        //play the sound when start
        runningSound.Play();
    }


    // Update is called once per frame
    void Update()
    {

        //get reference to the speed of the car
        float speed = gameObject.GetComponent<Rigidbody>().velocity.magnitude;

        //blend the sounds which is associated the speed of the car
        //get the percentage speed of the car which is relative to the max speed of car
        float blend = Mathf.Abs(speed / (5f * 0.8f));
        //assign corresponding pitch to the driving sounds of the car
        runningSound.pitch = Mathf.Lerp(minPitch, maxPitch, Mathf.Clamp(blend, 0, 1));


        //below is the old code which is not used anymore
        //if (speed < minPitch)
        //{
        //    runningSound.pitch = minPitch;
        //}
        //else if (speed > maxPitch)
        //{
        //    runningSound.pitch = maxPitch;
        //}
        //else
        //{
        //    runningSound.pitch = speed;
        //}


    }
    }

