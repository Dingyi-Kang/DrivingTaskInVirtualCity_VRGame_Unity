using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiFlipScript : MonoBehaviour
{
    [Tooltip("The maximum angle the car can be at before being considered flipped."), Range(0.1f, 180f)]
    public float maxAngle = 89f;
    [Tooltip("How long to wait for the car to recover itself before flipping it with the script.")]
    public float defaultTimer = 3f;

    // Internal timer for the countdown.
    private float timer = -1f;
    public bool flipped { get; private set; }

    // Update is called once per frame
    void Update()
    {
        // Get the angle of the car's local Z axis
        float currentAngle = Vector3.SignedAngle(transform.up, Vector3.up, transform.forward);
        //Debug.Log(currentAngle);
        
        // While a timer is active
        if (timer > 0f)
        {
            // Decrease timer by realtime
            timer -= Time.deltaTime;
            
            // Once the timer has expired
            if (timer < 0f)
            {
                // Move the car slightly upward to avoid clipping in the floor
                transform.Translate(new Vector3(0f, 0.5f, 0f), Space.World);

                // Reset the Z rotation of the car.
                Vector3 localRot = transform.localEulerAngles;
                localRot.z = 0f;
                transform.localEulerAngles = localRot;
            }
        }

        // If the car is flipped and no timer is active, start a timer.
        if (timer < 0f && Mathf.Abs(currentAngle) > maxAngle)
        {
            timer = defaultTimer;
            flipped = true;
        }
        // If the car corrects itself before the timer has expired, stop the timer.
        else if (Mathf.Abs(currentAngle) < maxAngle)
        {
            timer = -1f;
            flipped = false;
        }
            
    }

    // Gizmos to show angle of car for debugging.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.up);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }
}
