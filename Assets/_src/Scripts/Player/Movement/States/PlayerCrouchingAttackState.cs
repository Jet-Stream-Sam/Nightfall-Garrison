﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouchingAttackState : PlayerAttackState
{
    public PlayerCrouchingAttackState(PlayerMainController controllerScript, MainStateMachine stateMachine, PlayerMoves playerAttackAsset) : base(controllerScript, stateMachine, playerAttackAsset)
    {

    }
    public override void Enter()
    {
        base.Enter();
    }
    public override void HandleUpdate()
    {
        base.HandleUpdate();

    }
    public override void HandleFixedUpdate()
    {
        base.HandleFixedUpdate();
    }
    public override void Exit()
    {
        base.Exit();
    }
}
