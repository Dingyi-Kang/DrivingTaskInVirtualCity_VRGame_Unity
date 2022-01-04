using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class VRCarController : MonoBehaviour
{
    //get reference to steering wheel object
    public SteeringWheel wheel;

    //get reference to sound sources
    public AudioSource runningSound;
    public AudioSource brakingSound;

    //get reference to controll sources
    public InputActionAsset actions;
    public InputActionReference triggerRight;
    public InputActionReference triggerRightClick;
    public InputActionReference triggerLeft;
    public InputActionReference triggerLeftClick;

    //properties of car driving
    private float steerAngle;
    private bool isReverse;

    private bool isBraking;

    public float maxPitch = 4f;
    public float minPitch = 0.5f;
    private float currentPitch;
    private float pitchBlend = 1f;

    public float maxVelocity = 16.76f;
    public float motorForce = 500f;
    public float brakeForce = 0f;
    public float maxSteerAngle = 30f;

    public float minDrag = 0.05f;
    public float maxDrag = 0.75f;

    //get reference to wheel collider
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;
    public Transform frontLeftWheeelTransform;
    public Transform frontRightWheeelTransform;
    public Transform rearLeftWheeelTransform;
    public Transform rearRightWheeelTransform;

    private void Start()
    {
        //enable inputs from XR controller
        actions.Enable();
        currentPitch = runningSound.pitch;
    }

    private void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    //used to switch the driving state of car -- forward or backward
    public void OnReverse()
    {
        isReverse = !isReverse;
    }

    //used to handle the driving force and sounds effect of the car
    private void HandleMotor()
    {
        //get value from XR controller input
        float brake = 0f;
        if (triggerLeftClick.action.ReadValue<float>() == 1f)
            brake = 1f;
        else
            brake = triggerLeft.action.ReadValue<float>();
        float accelerate = 0f;
        if (triggerRightClick.action.ReadValue<float>() == 1f)
            accelerate = 1f;
        else
            accelerate = triggerRight.action.ReadValue<float>();

        //get value fo the speed of the car
        float speed = gameObject.GetComponent<Rigidbody>().velocity.magnitude;

        

        //change properties based on the input
        //if brake is not pressed
        if (brake == 0)
        {
            //if the driving state is backward
            if (isReverse)
            {
                accelerate *= -1;
            }
            //if the speed exceed the max speed, used cannot accelerate more
            if (gameObject.GetComponent<Rigidbody>().velocity.magnitude > maxVelocity)
                accelerate = 0;

            //assign motor force to the front two wheels collider of the car
            frontLeftWheelCollider.motorTorque = accelerate * motorForce;
            frontRightWheelCollider.motorTorque = accelerate * motorForce;

            //get relative speed of the car to the max speed
            float blend = Mathf.Abs(speed / (maxVelocity * 0.8f));
            //update the pitch of the sounds of driving which is assoicate with the relative speed of the car
            runningSound.pitch = Mathf.Lerp(minPitch, maxPitch, Mathf.Clamp(blend, 0, 1));
            currentPitch = runningSound.pitch;
            pitchBlend = 0f;

            //update the braking state when car is running
            isBraking = false;

            //if there is no moving force for the wheel and the speed is low enough, car stop
            if (accelerate == 0 && speed < 1.67625f * (1f / 5) && !GetComponent<AntiFlipScript>().flipped)
                gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        //if the brake is pressed
        else {
            //if this is the first time call braking and If speed is at half or more of max speed, play a long brake noise
            if (!isBraking && speed >= 8)
            {
                //the time property indicates when in sound clip that it begins to play
                brakingSound.time = 1f;
                brakingSound.Play();
                isBraking = true;
            } else if (!isBraking && speed < 2f * (3.5f / 5))
            {
                //If speed is very slow, play no brake noise
                isBraking = true;
            } else if (!isBraking && speed < 8)
            {
                //If speed is below half of max speed, play a short brake noise
                brakingSound.time = 1.3f;
                brakingSound.Play();
                isBraking = true;
            }

            // Blend running sound pitch smoothly to minimum.
            pitchBlend = Mathf.Clamp(pitchBlend + Time.deltaTime, 0f, 1f);
            runningSound.pitch = Mathf.Lerp(currentPitch, minPitch, pitchBlend);

            //if speed is low enough, stop the car
            if (speed < 1.67625f * (3.5f / 5) && !GetComponent<AntiFlipScript>().flipped)
                gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }

        //assign the drag friction to the car
        gameObject.GetComponent<Rigidbody>().drag = 
            Mathf.Lerp(maxDrag, minDrag, Mathf.Clamp(speed / (maxVelocity * 0.25f), 0f, 1f));

        //update the bake force based on the input
        brakeForce = brake * 4000f;
        //assign the brake force to the four wheel colliders
        frontLeftWheelCollider.brakeTorque = brakeForce;
        frontRightWheelCollider.brakeTorque = brakeForce;
        rearLeftWheelCollider.brakeTorque = brakeForce;
        rearRightWheelCollider.brakeTorque = brakeForce;
    }

    //this function is used to assign the steer angle to the two front wheel colliders
    private void HandleSteering() {
        //get relative value of the steer angle of steering wheel
        steerAngle = -maxSteerAngle * (wheel.Angle / wheel.angleLimit);
        //assign the relative steer angle to the two front wheel colliders
        frontLeftWheelCollider.steerAngle = steerAngle;
        frontRightWheelCollider.steerAngle = steerAngle;
        //get reference to the object
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        //assign the drag for turning
        rb.angularDrag = 0.5f + Mathf.Lerp(0, 1.5f, rb.velocity.magnitude / maxVelocity);
    }


    //this function is used to call functions to update the position of four wheel of the car
    private void UpdateWheels()
    {
        UpdateWheelPos(frontLeftWheelCollider, frontLeftWheeelTransform);
        UpdateWheelPos(frontRightWheelCollider, frontRightWheeelTransform);
        UpdateWheelPos(rearLeftWheelCollider, rearLeftWheeelTransform);
        UpdateWheelPos(rearRightWheelCollider, rearRightWheeelTransform);

    }

    //this function is used to change the position/orientation of a wheel object
    private void UpdateWheelPos(WheelCollider wheelCollider, Transform trans) {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        trans.rotation = rot;
        trans.position = pos;
    }


}
