﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiveKickAttack : MonoBehaviour, IAttackBehaviour
{
    private PlayerMainController controllerScript;
    private PlayerAttack attackAsset;
    [SerializeField] private float diveHorizontalPower = 3;
    [SerializeField] private float diveVerticalPower = 1;

    private AfterImageEffectPool vfxPool;
    [SerializeField] private GameObject afterImageEffect;
    [SerializeField] private float allowedDistanceBtwImages = 0.8f;
    private float lastImagePos;

    public void Init(PlayerMainController controllerScript, PlayerAttack attackAsset)
    {
        this.controllerScript = controllerScript;
        this.attackAsset = attackAsset;
    }
    public void OnAttackEnter()
    {
        float playerDirection = controllerScript.playerSpriteTransform.localScale.x;
        
        controllerScript.playerRigidBody.velocity =
            new Vector2(playerDirection * diveHorizontalPower + controllerScript.playerRigidBody.velocity.x, 
            -diveVerticalPower + controllerScript.playerRigidBody.velocity.y);


        vfxPool = controllerScript.VFXTransform.GetComponentInChildren<AfterImageEffectPool>();
        vfxPool.UpdatePool(afterImageEffect);
        vfxPool.GetFromPool();
        lastImagePos = controllerScript.playerMainCollider.transform.position.x;
        

    }

    public void OnAttackUpdate()
    {
        
    }

    public void OnAttackFixedUpdate()
    {
        if (Mathf.Abs(controllerScript.playerMainCollider.transform.position.x - lastImagePos) > allowedDistanceBtwImages)
        {
            vfxPool.GetFromPool();
            lastImagePos = controllerScript.playerMainCollider.transform.position.x;
        }
    }

    public void OnAttackExit()
    {

    }
}
