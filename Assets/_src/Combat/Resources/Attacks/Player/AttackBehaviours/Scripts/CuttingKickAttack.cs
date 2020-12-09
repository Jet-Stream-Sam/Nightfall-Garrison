﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingKickAttack : MonoBehaviour, IMoveBehaviour
{
    private PlayerMainController controllerScript;
    private PlayerMoves attackAsset;

    [SerializeField] private float dashPower = 1;
    [SerializeField] private float easingRate = 0.8f;

    [SerializeField] private GameObject afterImageEffect;
    [SerializeField] private float allowedDistanceBtwImages = 0.8f;

    
    private float easingMovement;
    private float lastImagePos;

    private AfterImageEffectPool vfxPool;

    public void Init(IEntityController controllerScript, Moves attackAsset, IState state)
    {
        if (controllerScript is PlayerMainController controller)
        {
            this.controllerScript = controller;
        }
    }


    public void OnMoveEnter()
    {
        float playerDirection = controllerScript.playerSpriteTransform.localScale.x;

        controllerScript.playerRigidBody.velocity =
            new Vector2(playerDirection * dashPower + controllerScript.playerRigidBody.velocity.x,
            controllerScript.playerRigidBody.velocity.y);

        easingMovement = controllerScript.playerRigidBody.velocity.x;

        vfxPool = controllerScript.playerMainVFXManager.afterImageEffectPool;
        
        vfxPool.UpdatePool(afterImageEffect);
        vfxPool.GetFromPool();
    }

    public void OnMoveExit()
    {

    }

    public void OnMoveFixedUpdate()
    {
        easingMovement =
            Mathf.Lerp(easingMovement,
            0,
            easingRate);

        float tempSpeed = easingMovement;

        controllerScript.playerRigidBody.velocity += Vector2.up * Physics2D.gravity.y * controllerScript.fallMultiplier * Time.deltaTime;

        controllerScript.playerRigidBody.velocity =
            new Vector2(tempSpeed, controllerScript.playerRigidBody.velocity.y);

        if (Mathf.Abs(controllerScript.playerMainCollider.transform.position.x - lastImagePos) > allowedDistanceBtwImages)
        {
            vfxPool.GetFromPool();
            lastImagePos = controllerScript.playerMainCollider.transform.position.x;
        }
    }

    public void OnMoveUpdate()
    {

    }
}
