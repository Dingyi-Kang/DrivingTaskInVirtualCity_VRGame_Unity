using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SteeringWheel : MonoBehaviour
{
    // The object holding the wheel.
    private GameObject m_Target;
    public GameObject Target
    {
        get
        {
            return m_Target;
        }
        set
        {
            lastAngle = Mathf.Infinity;
            m_Target = value;
        }
    }

    // The current vector used for rotating the wheel.
    private Vector3 projected;

    // Vector representing the projection of the world's up axis on the plane of the steering wheel.
    private Vector3 m_WheelUp;
    private Vector3 WheelUp
    { 
        get
        {
            if (m_WheelUp == Vector3.zero)
            {
                // Grab current neutral position upward
                Vector3 upwardPoint = transform.parent.up + transform.position;
                Vector3 a = transform.position - upwardPoint;
                Vector3 n = -transform.up;

                float projAngle = Vector3.Angle(a.normalized, n);
                float length2Plane = Mathf.Cos(projAngle * Mathf.Deg2Rad) * a.magnitude;
                Vector3 projectedPoint = upwardPoint + n * length2Plane;

                m_WheelUp = Vector3.Normalize(projectedPoint - transform.position);
            }
            return m_WheelUp;
        }
        set
        {
            m_WheelUp = value;
        }
    }

    // The current angle of the wheel.
    public float Angle { get; private set; }
    private float lastAngle;

    [Tooltip("How much the wheel can be turned in either direction."), Range(0f, 180f)]
    public float angleLimit = 150f;

    [Tooltip("How fast the wheel should return to neutral when the target isn't being tracked.\n[0: The wheel doesn't return to neutral]\n[1: The wheel snaps back to neutral immediately]"), 
        Range(0f, 1f)]
    public float returnSpeed = 0.5f;

    [Tooltip("How far the wheel can remain held from."), Range(0f, 1f)]
    public float maxDistance = 1f;

    private Vector3 pos;

    // Start is called before the first frame update
    void Start()
    {
        Angle = 0f;
        lastAngle = Mathf.Infinity;
        m_WheelUp = Vector3.zero;
        pos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // Reset WheelUp
        WheelUp = Vector3.zero;

        TOOFAR:
        if (Target != null)
        {
            // Project hand point onto 2D plane of steering wheel
            Vector3 a = transform.position - Target.transform.position;
            Vector3 n = -transform.up;
            float projAngle = Vector3.Angle(a.normalized, n);
            float length2Plane = Mathf.Cos(projAngle * Mathf.Deg2Rad) * a.magnitude;
            Vector3 projectedPoint = Target.transform.position + n * length2Plane;
            projected = Vector3.Normalize(projectedPoint - transform.position);

            // Cancel out if the point is too far from the wheel
            if (Vector3.Distance(Target.transform.position, transform.position + (projected * 0.2f)) > maxDistance)
            {
                Target = null;
                goto TOOFAR;
            }

            // Get the angle of the vector
            float currentAngle = Vector3.SignedAngle(WheelUp, projected, -transform.up);

            // Store initial angle if previously not tracked
            if (lastAngle == Mathf.Infinity)
                lastAngle = currentAngle;

            // If stored is different from current, calculate the difference to apply
            float correction = lastAngle - currentAngle;

            // Validate whether or not the new angle is a valid rotation
            bool validRotation = 
                Mathf.Abs(correction) < 180f &&         // If absolute value of the difference is greater than 180, then the angle has swapped signs and should be ignored this frame.
               (Mathf.Abs(Angle) < angleLimit ||        // If the current angle is within the limited range, then it is valid to be clamped at the threshold.
                Angle > 0f && correction < 0f ||        // If the current angle is equal / past the positive threshold, then it is only valid if the correction is negative.
                Angle < 0f && correction > 0f);         // If the current angle is equal / past the negative threshold, then it is only valid if the correction is positive.

            if (validRotation) 
            {
                // Clamp the new angle to threshold so either threshold is a reachable value
                float clampedAngle = Mathf.Clamp(Angle + correction, -angleLimit, angleLimit);
                float clampedCorrection = Angle - clampedAngle;

                // Rotate the wheel by the clamped correction
                transform.RotateAround(transform.position, -transform.up, clampedCorrection);
                Angle = Vector3.SignedAngle(-transform.forward, WheelUp, -transform.up);
            }   

            // Update the last known angle
            lastAngle = currentAngle;
        }
        else
        {
            // Mark last angle as unset by setting it as an impossible value
            lastAngle = Mathf.Infinity;

            // Set projected to WorldUp so it is shown by Gizmos
            projected = WheelUp;

            // Gradually move wheel back to neutral position
            float remainingOffset = Vector3.SignedAngle(-transform.forward, WheelUp, -transform.up);
            float blend = (remainingOffset != 0) ? returnSpeed * remainingOffset * Time.deltaTime : 0f;
            transform.RotateAround(transform.position, -transform.up, blend);
            Angle = Vector3.SignedAngle(-transform.forward, WheelUp, -transform.up);
        }

        // Fix for wheel transform drift
        transform.localPosition = pos;
    }

    private void OnDrawGizmos()
    {
        // Only show Gizmos in Play Mode
        if (!Application.isPlaying)
            return;

        // Target vector
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(transform.position + (projected * 0.2f), 0.1f);

        if (Target != null)
        { 
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawSphere(Target.transform.position, 0.75f);
        }

        // Forward vector
        Gizmos.color = Gizmos.color = new Color(0, 1, 0, 0.5f);
        Vector3 upPosition = transform.position + (-transform.forward * 0.2f);
        Gizmos.DrawSphere(upPosition, 0.05f);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SteeringWheel)), CanEditMultipleObjects]
public class SteeringWheelEditor : Editor
{
    // The script being inspected
    private SteeringWheel instance;

    // Prefix label text for Target
    private readonly static GUIContent targetPrefix = new GUIContent("Target", "The Game Object currently holding the wheel.");

    private void OnEnable()
    {
        instance = (SteeringWheel)target;
    }

    public override void OnInspectorGUI()
    {
        // Make sure object is obtained first
        if (instance == null)
            return;
        
        // Update serialized values
        serializedObject.Update();

        // Serialized properties
        EditorGUILayout.PropertyField(serializedObject.FindProperty("angleLimit"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("returnSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxDistance"));

        // Properties only usable in Play Mode
        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("The following properties should only be set during runtime.", MessageType.Info);

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);

        // Only change when target is modified so that last angle isn't constantly reset.
        EditorGUI.BeginChangeCheck();
        GameObject newTarget = (GameObject)EditorGUILayout.ObjectField(targetPrefix, instance.Target, typeof(GameObject), true);
        if (EditorGUI.EndChangeCheck())
            instance.Target = newTarget;

        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndVertical();

        // Apply changed serialized properties
        serializedObject.ApplyModifiedProperties();
    }
}
#endif