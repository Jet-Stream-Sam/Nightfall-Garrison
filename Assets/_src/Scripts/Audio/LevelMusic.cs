﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMusic : MonoBehaviour
{
    private SoundManager soundManager;
    [SerializeField] private GameMusic musicToPlay;
    private void Start()
    {
        soundManager = SoundManager.Instance;
        soundManager.PlayMusic(musicToPlay.name);
    }
}
