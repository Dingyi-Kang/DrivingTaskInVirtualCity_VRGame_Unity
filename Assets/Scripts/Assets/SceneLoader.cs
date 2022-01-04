using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public MultiSceneConfig Config;

    // Start is called before the first frame update
    void Start()
    {
        //Get all the unloaded scenes, and load them
        for (int i = 1; i < Config.Scenes.Length; i++)
        {
            if (!SceneManager.GetSceneByPath(Config.Scenes[i].path).IsValid())
                SceneManager.LoadScene(Config.Scenes[i].path, LoadSceneMode.Additive);
        }
    }
}
