﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class PlayerFallingState : PlayerState
{
    public PlayerFallingState(PlayerMainController controllerScript, MainStateMachine stateMachine) : base(controllerScript, stateMachine)
    {

    }

    public override void Enter()
    {
        base.Enter();
        controllerScript.playerAnimationsScript.ChangeAnimationState("player_fall");

 
    }
    public override void HandleUpdate()
    {
        base.HandleUpdate();


        bool hasLanded = controllerScript.isGrounded;
        if (hasLanded)
        {
            stateMachine.ChangeState(new PlayerStandingState(controllerScript, stateMachine));
            ParticleSystem landingDust = controllerScript.playerMainVFXManager.playerDustParticles.dustParticles["LandingDust"];
            landingDust.Play();
        }
        if (controllerScript.airborneJumpTimer > Time.time && 
            controllerScript.groundedJumpTimer > Time.time)
        {
            stateMachine.ChangeState(new PlayerJumpingState(controllerScript, stateMachine));
            Debug.Log("SPECIAL JUMP");
        }
            
        

    }
    public override void HandleFixedUpdate()
    {
        base.HandleFixedUpdate();
        controllerScript.playerRigidBody.velocity += Vector2.up * Physics2D.gravity.y * controllerScript.fallMultiplier * Time.deltaTime;

        float tempSpeed = easingMovementX * controllerScript.standingMoveSpeed;

        controllerScript.playerRigidBody.velocity =
            new Vector2(tempSpeed, controllerScript.playerRigidBody.velocity.y);
    }
    public override void Exit()
    {
        base.Exit();
        controllerScript.attacksInTheAir = 0;

    }

}