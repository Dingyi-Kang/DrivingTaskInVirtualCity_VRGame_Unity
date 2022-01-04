using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

// Serializatble class for managing references to objects in other scenes more easily.
[System.Serializable]
public class CrossReference
{
    [SerializeField]
    private string m_ScenePath;
    public string ScenePath { get { return m_ScenePath; }  }

    [SerializeField]
    private string m_ObjectPath;
    public string ObjectPath { get { return m_ObjectPath; } }

    [SerializeField]
    private int m_ObjectID;

    // Passthrough to gain access to the object present in another scene during runtime.
    private GameObject m_GameObject;
    [HideInInspector]
    public GameObject gameObject {
        get
        {
            if (m_GameObject == null)
                m_GameObject = (GameObject)GameObjectHelper.FindObjectFromInstanceID(m_ObjectID);
            return m_GameObject;
        } 
    }

    // Direct access to the transform of the object like other components.
    public Transform transform
    {
        get
        {
            return gameObject.transform;
        }
    }

    public CrossReference()
    {
        m_ScenePath = "None";
        m_ObjectPath = "None";
        m_ObjectID = -1;
    }

    // Internal script definition.
    public CrossReference(string scenePath, string objectPath)
    {
        m_ScenePath = scenePath;
        m_ObjectPath = objectPath;
    }
}

// Utility class containing a few methods for getting components on cross references.
public static class CrossReferenceUtility
{
    public static T GetComponent<T>(this CrossReference crossReference) where T : Component
    {
        return crossReference.gameObject.GetComponent<T>();
    }

    public static Component GetComponent(this CrossReference crossReference, string type)
    {
        return crossReference.gameObject.GetComponent(type);
    }

    public static Component GetComponent(this CrossReference crossReference, System.Type type)
    {
        return crossReference.gameObject.GetComponent(type);
    }
}

// Property drawer class for showing better UI in the inspector for public variables of the CrossReference type.
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(CrossReference))]
public class CrossReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw Label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Draw Box
        GUI.Box(position, GUIContent.none);

        // Calculate rects
        var sceneRect = new Rect(position.x + 1, position.y + 1, position.width - 2, (position.height - 2) / 2);
        var objectRect = new Rect(position.x + 1, position.y + (position.height / 2), position.width - 2, (position.height - 2) / 2);

        // Draw fields
        if (!Application.isPlaying)
        {
            List<string> sceneOptions = GetScenesList();
            sceneOptions.Insert(0, "None");
            string[] sceneDisplay = sceneOptions.ToArray();
            for (int i = 1; i < sceneDisplay.Length; i++)
                sceneDisplay[i] = sceneDisplay[i].Substring(sceneDisplay[i].LastIndexOf('/') + 1).TrimEnd(new char[] { '.', 'u', 'n', 'i', 't', 'y' });
            int sceneIndex = 0;
            if (sceneOptions.Contains(property.FindPropertyRelative("m_ScenePath").stringValue))
            {
                sceneIndex = sceneOptions.IndexOf(property.FindPropertyRelative("m_ScenePath").stringValue);
                int chosenIndex = EditorGUI.Popup(sceneRect, sceneIndex, sceneDisplay);
                property.FindPropertyRelative("m_ScenePath").stringValue = sceneOptions[chosenIndex];
                sceneIndex = chosenIndex;
            }
            else
            {
                List<string> otherOptions = new List<string>(sceneOptions) { property.FindPropertyRelative("m_ScenePath").stringValue + " (Missing)" };
                int chosenIndex = EditorGUI.Popup(sceneRect, sceneOptions.Count, otherOptions.ToArray());
                if (chosenIndex < sceneOptions.Count)
                {
                    property.FindPropertyRelative("m_ScenePath").stringValue = sceneOptions[chosenIndex];
                    sceneIndex = chosenIndex;
                } 
            }

            EditorGUI.BeginDisabledGroup(!(sceneIndex != 0 && sceneIndex < sceneOptions.Count));

            // This section could be cleaned up a fair bit.
            List<int> instanceIDs = new List<int>();
            List<string> objectOptions = (sceneIndex != 0 && sceneIndex < sceneOptions.Count) 
                ? GetObjectList(property.FindPropertyRelative("m_ScenePath").stringValue, out instanceIDs) : new List<string>();
            objectOptions.Insert(0, "None");
            instanceIDs.Insert(0, -1);
            if (objectOptions.Contains(property.FindPropertyRelative("m_ObjectPath").stringValue))
            {
                int objectIndex = objectOptions.IndexOf(property.FindPropertyRelative("m_ObjectPath").stringValue);
                int chosenIndex = EditorGUI.Popup(objectRect, objectIndex, objectOptions.ToArray());
                property.FindPropertyRelative("m_ObjectID").intValue = instanceIDs[chosenIndex];
                property.FindPropertyRelative("m_ObjectPath").stringValue = objectOptions[chosenIndex];
            }
            else if (instanceIDs.Contains(property.FindPropertyRelative("m_ObjectID").intValue))
            {
                int objectIndex = instanceIDs.IndexOf(property.FindPropertyRelative("m_ObjectID").intValue);
                int chosenIndex = EditorGUI.Popup(objectRect, objectIndex, objectOptions.ToArray());
                property.FindPropertyRelative("m_ObjectPath").stringValue = objectOptions[chosenIndex];
            }
            else
            {
                List<string> otherOptions = new List<string>(objectOptions) { property.FindPropertyRelative("m_ObjectPath").stringValue + " (Missing)" };
                int chosenIndex = EditorGUI.Popup(objectRect, objectOptions.Count, otherOptions.ToArray());
                if (chosenIndex < objectOptions.Count)
                {
                    property.FindPropertyRelative("m_ObjectPath").stringValue = objectOptions[chosenIndex];
                    property.FindPropertyRelative("m_ObjectID").intValue = instanceIDs[chosenIndex];
                }   
            }

            EditorGUI.EndDisabledGroup();
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            GameObject item = (GameObject)GameObjectHelper.FindObjectFromInstanceID(property.FindPropertyRelative("m_ObjectID").intValue);
            EditorGUI.Popup(sceneRect, 0, new string[] { item.scene.name });
            EditorGUI.Popup(objectRect, 0, new string[] { GameObjectHelper.GetGameObjectPath(item) });
            EditorGUI.EndDisabledGroup();
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) * 2 + 4;
    }

    private static List<string> GetScenesList()
    {
        SceneSetup[] setups = EditorSceneManager.GetSceneManagerSetup();
        List<string> scenePaths = new List<string>();
        foreach (SceneSetup setup in setups)
            scenePaths.Add(setup.path);
        return scenePaths;
    }

    private static List<string> GetObjectList(string scenePath, out List<int> instanceIDs)
    {
        instanceIDs = new List<int>();

        // Load the Scene
        Scene scene = SceneManager.GetSceneByPath(scenePath);
        if (!scene.IsValid())
        {
            Debug.LogError(scenePath + " does not exist!");
            return new List<string>();
        }
        else if (!scene.isLoaded)
        {
            Debug.LogError(scenePath + " is unloaded!");
            return new List<string>();
        }

        // Get all top-level objects in the Scene
        GameObject[] currentObjects = scene.GetRootGameObjects();

        List<string> objectPaths = GetAllObjectPaths(currentObjects, ref instanceIDs);
        return objectPaths;
    }

    private static List<string> GetAllObjectPaths(GameObject[] parents, ref List<int> instanceIDs)
    {
        List<string> objectPaths = new List<string>();
        foreach (var gameObject in parents)
        {
            objectPaths.Add(GameObjectHelper.GetGameObjectPath(gameObject));
            instanceIDs.Add(gameObject.GetInstanceID());
            objectPaths.AddRange(GetAllObjectPaths(GameObjectHelper.GetDirectChildren(gameObject), ref instanceIDs));
        }
        return objectPaths;
    }
}
#endif