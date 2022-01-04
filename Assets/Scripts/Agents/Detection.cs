using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detection : MonoBehaviour
{
    private AgentControl agent; // reference to the parent's AgentControll component

    // Start is called before the first frame update
    void Start()
    {
        agent = transform.parent.gameObject.GetComponent<AgentControl>(); // a grab parent's agent controll component
    }

    // called when another trigger is hit
    private void OnTriggerEnter(Collider other) 
    {
        // if the trigger is not a waypoint or the objective, then it belongs to a vehicle object
        if (other.gameObject.GetComponent<WaypointTrigger>() == null && other.gameObject.GetComponent<DeliveryScript>() == null)
        {
            // tell the vehicle to stop
            agent.shouldStop = true;
        }
    }

    // called when a trigger is exited
    private void OnTriggerExit(Collider other) 
    {
        // again, check if the other trigger is a waypoint or the objective; if not, it belongs to a vehicle
        if (other.gameObject.GetComponent<WaypointTrigger>() == null && other.gameObject.GetComponent<DeliveryScript>() == null)
        {
            // tell the vehicle to resume movement
            agent.shouldStop = false;
        }
    }
}
