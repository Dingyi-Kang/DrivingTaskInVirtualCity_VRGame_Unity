using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentControl : MonoBehaviour
{
    public NavMeshAgent agent;  // Must have this to use NavMesh
    public GameObject waypoint; // The current waypoint; initially set in inspector as vehicle's first waypoint

    [HideInInspector]
    public bool shouldStop;     // Set to true when a vehicle is detected; prevents collisions

    private Vector3 targetPoint;                                   // Point the vehicle is attempting to reach
    private Vector3 stopVelocity = new Vector3 (0.0f, 0.0f, 0.0f); // A zero vector

    // Update is called once per frame
    void Update()
    {
        if ((waypoint != null) && (!shouldStop)) // if we have a waypoint and a vehicle has not been detected
        {
            // navigate toward the waypoint
            targetPoint = waypoint.transform.position; 
            agent.SetDestination(targetPoint);
        }
        else if (shouldStop) // else if a vehicle has been detected
        {
            // stop
            agent.velocity = stopVelocity;
            agent.SetDestination(agent.transform.position);
        }
    }

    // OnDrawGizmos is called in the scene view when Gizmos are active
    void OnDrawGizmos()
    {
        // essentially, this if-else block marks the location of the vehicles current waypoint if one is set
        if (targetPoint != Vector3.zero)
        {
            // draw a blue sphere to indicate the current waypoint
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawSphere(targetPoint, 1);
        }
        else if (!Application.isPlaying && waypoint != null)
        {
            // draw sphere
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawSphere(waypoint.transform.position, 1);
        }
    }
}
