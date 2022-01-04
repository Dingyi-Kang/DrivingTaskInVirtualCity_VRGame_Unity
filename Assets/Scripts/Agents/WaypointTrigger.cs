using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointTrigger : MonoBehaviour
{
    // Enforce same RNG for debugging
    public bool sameSeed = false;

    // Stores all agents who have hit this waypoint and need a new one assigned.
    private readonly List<Collider> agents = new List<Collider>();

    // List of possible waypoints that can be assigned to agents.
    public GameObject[] waypoints = new GameObject[0];

    private void Start()
    {
        if (!sameSeed)
        {
            // Set the RNG seed to be the hashed time of the system in ticks.
            int seed = System.DateTime.Now.Ticks.GetHashCode();
            Random.InitState(seed);
            //Debug.Log("Seed: " + seed);
        }
    }

    private void LateUpdate()
    {
        // Set new waypoints for all agents who've hit this waypoint.
        for (int i = 0; i < agents.Count; i++)
            UpdateWaypoint(agents[i]);
    }

    // Updates the waypoint of an agent given its collider.
    private void UpdateWaypoint(Collider agent)
    {
        // Get the AgentControl component of the agent.
        AgentControl controller = agent.gameObject.GetComponent<AgentControl>();

        // Only set the next waypoint if the agent was currently attempting to reach this waypoint.
        if (controller != null && controller.waypoint != null && controller.waypoint == this.gameObject)
        {
            // Choose a waypoint randomly from the stored selection of possible ones.
            int chosen = (int)Random.Range(0f, waypoints.Length - 0.00001f);
            //Debug.Log("Trigger Hit: " + chosen);
            controller.waypoint = waypoints[chosen];
            agents.Remove(agent);
        }
        // Collider was not an agent; ignore it.
        else if (controller == null)
            agents.Remove(agent);
    }

    // Queue the agent to receive a new waypoint if it isn't already in the queue.
    private void OnTriggerEnter(Collider other)
    {
        if (!agents.Contains(other))
            agents.Add(other);
    }
}
