using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoplightClient : MonoBehaviour
{
    private enum State
    {
        Red = 2,
        Yellow = 1,
        Green = 0,
    }
    private State m_CurrentState;
    private State CurrentState 
    { 
        get { return m_CurrentState; } 
        set 
        {
            m_CurrentState = value;
            if (animator != null)
                animator.SetInteger("Light Color", (int)m_CurrentState);
        }
    }

    // Starting waypoint of the intersection.
    public WaypointTrigger waypoint;

    // Length of ALL yellow lights
    public static float yellowTimer = 3f;
    private float timer;
    private bool changing = false;

    // Animator for light changes
    private Animator animator;

    private void Start()
    {
        timer = 0f;
        if (waypoint != null)
            waypoint.gameObject.GetComponent<Collider>().enabled = false;
        animator = gameObject.GetComponent<Animator>();
        CurrentState = State.Red;
    }

    public void Update()
    {
        if (changing)
        {
            timer -= Time.deltaTime;
            if (timer < 0f)
                CompleteChange();
        }
    }

    // Queue the light change after pausing for yellow.
    public void StartChange()
    {
        timer = yellowTimer;
        if (CurrentState == State.Green)
            CurrentState = State.Yellow;
        changing = true;
    }

    // Turn the light either green or red depending on what it currently is.
    public void CompleteChange()
    {
        if (CurrentState == State.Green || CurrentState == State.Yellow)
        {
            CurrentState = State.Red;
            if (waypoint != null)
                waypoint.gameObject.GetComponent<Collider>().enabled = false;
        }
        else
        {
            CurrentState = State.Green;
            if (waypoint != null)
                waypoint.gameObject.GetComponent<Collider>().enabled = true;
        }
        changing = false;
    }
}
