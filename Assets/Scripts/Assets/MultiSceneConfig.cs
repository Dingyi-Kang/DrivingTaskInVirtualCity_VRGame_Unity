using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

[System.Serializable, CreateAssetMenu(fileName = "MSConfig.asset", menuName = "Multi-Scene/Configuration")]
public class MultiSceneConfig : ScriptableObject
{
    [SerializeField]
    private SceneSetupWrapper[] m_Scenes;
    public SceneSetupWrapper[] Scenes { get { return m_Scenes; } }

    #if UNITY_EDITOR
    // Saves the current multi-Scene configuration
    private void SaveMultiSceneConfig()
    {
        SceneSetup[] sceneSetups = EditorSceneManager.GetSceneManagerSetup();
        m_Scenes = new SceneSetupWrapper[sceneSetups.Length];
        for (int i = 0; i < m_Scenes.Length; i++)
            m_Scenes[i] = new SceneSetupWrapper(sceneSetups[i]);
    }

    // Loads the saved multi-Scene configuration
    private void LoadMultiSceneConfig()
    {
        if (m_Scenes != null)
        {
            SceneSetup[] sceneSetups = new SceneSetup[m_Scenes.Length];
            for (int i = 0; i < sceneSetups.Length; i++)
                sceneSetups[i] = m_Scenes[i].Unwrap();
            foreach (SceneSetup sceneInfo in sceneSetups)
                if (File.Exists(sceneInfo.path))
                    EditorSceneManager.OpenScene(sceneInfo.path, OpenSceneMode.Additive);
            EditorSceneManager.RestoreSceneManagerSetup(sceneSetups);
        }
    }
    #endif

    /*
     *  Wrapper class required for serializing SceneSetup data
     */
    [System.Serializable]
    public class SceneSetupWrapper
    {
        public bool isActive;
        public bool isLoaded;
        public bool isSubScene;
        public string path;

        #if UNITY_EDITOR
        public SceneSetupWrapper(SceneSetup sceneInfo)
        {
            isActive = sceneInfo.isActive;
            isLoaded = sceneInfo.isLoaded;
            isSubScene = sceneInfo.isSubScene;
            path = sceneInfo.path;
        }

        public SceneSetup Unwrap()
        {
            SceneSetup sceneSetup = new SceneSetup()
            {
                isActive = this.isActive,
                isLoaded = this.isLoaded,
                isSubScene = this.isSubScene,
                path = this.path
            };
            return sceneSetup;
        }
        #endif
    }

    /*
     *  Custom Inspector for Saving and Loading
     */
    #if UNITY_EDITOR
    [CustomEditor(typeof(MultiSceneConfig))]
    public class MultiSceneConfigEditor : Editor
    {
        private MultiSceneConfig config;

        private bool scenesFoldout = false;

        private void OnEnable()
        {
            if (target != null)
                config = (MultiSceneConfig)target;
        }

        private void OnDisable()
        {
            AssetDatabase.SaveAssetIfDirty(config);
        }

        public override void OnInspectorGUI()
        {
            // Wait for asset to be named
            if (config == null)
                return;

            // Save button
            if (GUILayout.Button("Save Multi-Scene Configuration"))
            {
                if (config.m_Scenes == null || EditorUtility.DisplayDialog("Multi-Scene Configuration", "Overwrite the existing configuration?", "Yes", "Cancel"))
                {
                    config.SaveMultiSceneConfig();
                    EditorUtility.SetDirty(config);
                    scenesFoldout = true;
                }
            }

            // Load button
            EditorGUI.BeginDisabledGroup(config.m_Scenes == null || config.m_Scenes.Length < 1);
            
            if (GUILayout.Button("Load Multi-Scene Configuration"))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    config.LoadMultiSceneConfig();
            }
                
            EditorGUI.EndDisabledGroup();

            // List the paths of the saved scenes in order.
            scenesFoldout = EditorGUILayout.Foldout(scenesFoldout, "Scenes");
            if (scenesFoldout)
            {
                EditorGUI.indentLevel++;
                int order = 0;
                if (config.m_Scenes != null)
                    foreach (SceneSetupWrapper sceneInfo in config.m_Scenes)
                    {
                        string prefix = order == 0 ? "Active" : "Scene " + order;
                        if (File.Exists(sceneInfo.path))
                            EditorGUILayout.LabelField(prefix, sceneInfo.path);
                        else
                            EditorGUILayout.LabelField(prefix, sceneInfo.path + " (Missing)");
                        order++;
                    }
                if (config.m_Scenes == null || config.m_Scenes.Length < 1)
                    EditorGUILayout.LabelField("None");
                EditorGUI.indentLevel--;
            }
        }
    }
    #endif
}