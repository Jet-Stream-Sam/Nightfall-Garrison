﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerState : IState
{
    protected PlayerMainController controllerScript;
    protected MainStateMachine stateMachine;
    protected float deadzoneMin;
    public bool OverridingState { get; set; }

    public PlayerState(PlayerMainController controllerScript, MainStateMachine stateMachine)
    {
        this.controllerScript = controllerScript;
    }
    public virtual void Enter()
    {
        OverridingState = false;
        stateMachine = controllerScript.StateMachine;
        deadzoneMin = InputSystem.settings.defaultDeadzoneMin;
    }
    public virtual void HandleUpdate()
    {
 
        controllerScript.isGrounded = Physics2D.OverlapCircle(
            controllerScript.groundCheck.position, 
            controllerScript.groundCheckRadius,
            controllerScript.groundMask);

        controllerScript.isHittingHead = Physics2D.OverlapCircle(
            controllerScript.ceilingCheck.position,
            controllerScript.ceilingCheckRadius,
            controllerScript.ceilingMask);


        if (controllerScript.MovementX > 0)
        {
            controllerScript.playerSpriteTransform.localScale = 
                new Vector2(1, 1);
            controllerScript.isReversed = false;

        }
        else if (controllerScript.MovementX < 0)
        {
            controllerScript.playerSpriteTransform.localScale = 
                new Vector2(-1, 1);
            controllerScript.isReversed = true;
        }

        
        
    }

    public virtual void HandleFixedUpdate()
    {
 
    }
    public virtual void Exit()
    {

    }


    protected float ClampMovement(float value)
    {
        if (value < 0.01f && value > -0.01f)
        {
            value = 0;
        }
        if (value > 0.99f)
        {
            value = 1;
        }
        if (value < -0.99f)
        {
            value = -1;
        }
        return value;
    }
}
