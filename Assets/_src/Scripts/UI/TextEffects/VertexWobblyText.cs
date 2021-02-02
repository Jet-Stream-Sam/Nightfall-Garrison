﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VertexWobblyText : MonoBehaviour
{
    [SerializeField] private TMP_Text textComponent;
    [SerializeField] private Vector2 wobbleSpeed = new Vector2(1, 1);
    [SerializeField] private Vector2 wobbleOffset = new Vector2(1, 1);
    private Mesh mesh;
    private Vector3[] vertices;

    private void Update()
    {
        textComponent.ForceMeshUpdate();

        mesh = textComponent.mesh;
        vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 offset = Wobble(Time.time + i);

            vertices[i] = vertices[i] + offset;
        }

        mesh.vertices = vertices;
        textComponent.canvasRenderer.SetMesh(mesh);
    }

    private Vector3 Wobble(float time)
    {
        return new Vector3(Mathf.Sin(time * wobbleSpeed.x) * wobbleOffset.x, Mathf.Sin(time * wobbleSpeed.y) * wobbleOffset.y);
    }

}
