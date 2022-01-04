using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWheel : MonoBehaviour
{
    // Steering wheel instance.
    public SteeringWheel wheel;

    // Sets the target of the wheel to the object attempting to grab the wheel.
    public void setTarget() {
        wheel.Target = this.gameObject;
    }

    // Removes the current target of the wheel by nulling its current value.
    public void removeTarget() {
        wheel.Target = null;
    }
}
