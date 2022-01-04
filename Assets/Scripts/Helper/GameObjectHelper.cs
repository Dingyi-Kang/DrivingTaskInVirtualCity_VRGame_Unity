using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Helper class containing a few helpful methods for managing GameObjects.
public static class GameObjectHelper
{
    // Stores method implementation for private method.
    private static Func<int, UnityEngine.Object> m_FindObjectFromInstanceID = null;

    // Grabs the private method implementation of Object.FindObjectFromInstanceID() and makes it publicly available through this class.
    #if UNITY_EDITOR
    static GameObjectHelper()
    {
        var methodInfo = typeof(UnityEngine.Object).GetMethod("FindObjectFromInstanceID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (methodInfo == null)
            Debug.LogError("FindObjectFromInstanceID was not found in UnityEngine.Object");
        else
            m_FindObjectFromInstanceID = (Func<int, UnityEngine.Object>) Delegate.CreateDelegate(typeof(Func<int, UnityEngine.Object>), methodInfo);
    }
    #else
    [RuntimeInitializeOnLoadMethod]
    static void LoadMethod()
    {
        var methodInfo = typeof(UnityEngine.Object).GetMethod("FindObjectFromInstanceID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (methodInfo == null)
            Debug.LogError("FindObjectFromInstanceID was not found in UnityEngine.Object");
        else
            m_FindObjectFromInstanceID = (Func<int, UnityEngine.Object>)Delegate.CreateDelegate(typeof(Func<int, UnityEngine.Object>), methodInfo);
    }
    #endif

    // Public access to Object.FindObjectFromInstanceID(int instanceID).
    public static UnityEngine.Object FindObjectFromInstanceID(int instanceID)
    {
        if (m_FindObjectFromInstanceID == null)
            return null;
        return m_FindObjectFromInstanceID(instanceID);
    }

    // Returns an array of all direct children for a given GameObject.
    public static GameObject[] GetDirectChildren(GameObject parent)
    {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in parent.transform)
            children.Add(child.gameObject);
        return children.ToArray();
    }

    // Returns the path to a GameObject within a Scene's hierarchy.
    public static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        while (obj.transform.parent != null)
        {
            path = "/" + path;
            obj = obj.transform.parent.gameObject;
            path = obj.name + path;
        }
        return path;
    }
}
