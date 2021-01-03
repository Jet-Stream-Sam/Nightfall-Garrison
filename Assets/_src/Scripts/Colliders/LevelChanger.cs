﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    private LoadingManager loadingManager;
    [SerializeField] private string levelToLoadName;

    private void Start()
    {
        loadingManager = LoadingManager.Instance;
    }
    public void ChangeLevel()
    {
        Debug.Log(gameObject.scene.name);
        loadingManager.InitiateLoad(gameObject.scene.name, levelToLoadName);
    }
}
