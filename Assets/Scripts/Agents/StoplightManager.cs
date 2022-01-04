using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoplightManager : MonoBehaviour
{
    [Tooltip("Length of a green light in seconds.")]
    public float stateInterval = 10f;
    private float timer;

    [Tooltip("Stoplights at this intersection.")]
    public StoplightClient[] stoplights;
    [SerializeField, Tooltip("The index of the stoplight that is currently green in the list.")]
    private int activeLight = 0;

    private bool LateStart = true;

    // Start is called before the first frame update
    void Start()
    {
        timer = stateInterval;
    }

    // Update is called once per frame
    void Update()
    {
        if (LateStart)
        {
            // Start the default active light as green.
            stoplights[activeLight].CompleteChange();
            LateStart = false;
        }

        timer -= Time.deltaTime;
        if (timer < 0f)
        {
            UpdateLights();
            timer = stateInterval;
        }
    }

    // Turns the active light red and the next light green.
    void UpdateLights()
    {
        stoplights[activeLight].StartChange();
        if (stoplights.Length > 1)
        {
            // Move to the next element in the list or wrap back to the start.
            activeLight = (activeLight + 1) % stoplights.Length;
            stoplights[activeLight].StartChange();
        }
    }
}
