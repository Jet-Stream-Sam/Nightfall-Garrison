﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStandingState : PlayerGroundedState
{
    private float toCrouchDelay = 0.18f;
    private float toCrouchTimer;
    public PlayerStandingState(PlayerMainController controllerScript, MainStateMachine stateMachine) : base(controllerScript, stateMachine)
    {

    }

    public override void Enter()
    {
        base.Enter();
        
        if (controllerScript.MovementX != 0)
            controllerScript.playerAnimationsScript.ChangeAnimationState("player_walk");
        else
            controllerScript.playerAnimationsScript.ChangeAnimationState("player_idle");

        toCrouchTimer = toCrouchDelay;
    }

    public override void HandleUpdate()
    {
        if (controllerScript.MovementX != 0)
            controllerScript.playerAnimationsScript.ChangeAnimationState("player_walk");
        else
            controllerScript.playerAnimationsScript.ChangeAnimationState("player_idle");

        base.HandleUpdate();



        if (controllerScript.MovementY < -deadzoneMin)
        {
            toCrouchTimer -= Time.deltaTime;
            
        }
        else if(toCrouchTimer != toCrouchDelay)
        {
            toCrouchTimer = toCrouchDelay;
        }

        if(toCrouchTimer < 0)
        {
            stateMachine.ChangeState(new PlayerCrouchingState(controllerScript, stateMachine));
        }
        if (controllerScript.airborneJumpTimer > Time.time)
            stateMachine.ChangeState(new PlayerJumpingState(controllerScript, stateMachine));

    }

    public override void HandleFixedUpdate()
    {
        base.HandleFixedUpdate();

        float tempSpeed = easingMovementX * controllerScript.standingMoveSpeed;

        controllerScript.playerRigidBody.velocity =
            new Vector2(tempSpeed, controllerScript.playerRigidBody.velocity.y);

        
    }

    public override void Exit()
    {
        base.Exit();


    }

    
    
    
}