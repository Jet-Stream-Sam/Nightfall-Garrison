﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class FlyingEnemyTargetedAttackState : EnemyState
{
    private MainVFXManager VFXManager;
    protected CancellationTokenSource tokenSource;
    private Vector3 initialEnemyScale;
    private bool lockAsyncMethod;
    public Vector2 directionToFollow { get; private set; }
    public Transform focusedTargetTransform { get; private set; }
    private Transform enemyTransform;
    private EnemyMoves attackAsset;
    private float attackTimer;
    protected IMoveBehaviour attackBehaviour;
    protected bool attackAndProjectile;
    protected bool wasFlipped;
    public FlyingEnemyTargetedAttackState(EnemyMainController controllerScript, MainStateMachine stateMachine,
        EnemyMoves enemyAttackAsset, Transform target) : base(controllerScript, stateMachine)
    {
        attackAsset = enemyAttackAsset;
        focusedTargetTransform = target;
        if(attackAsset.moveType == Moves.MoveType.Projectile)
        {
            attackAndProjectile = enemyAttackAsset.attackAndProjectile;
            controllerScript.AnimationEventWasCalled += ShootProjectile;
        }

        
        if (enemyAttackAsset.moveBehaviour is IMoveBehaviour attack)
        {
            if (attack is MonoBehaviour attackComponent)
            {
                attackBehaviour = (IMoveBehaviour)UnityEngine.Object.Instantiate(attackComponent, controllerScript.transform);
            }
            
        }
    }

    public override void Enter()
    {
        
        base.Enter();

        VFXManager = controllerScript.enemyVFXManager;
        attackBehaviour?.Init(controllerScript, attackAsset, this);

        enemyTransform = controllerScript.enemySpriteTransform;
        directionToFollow = ((Vector2)focusedTargetTransform.position - (Vector2)enemyTransform.position).normalized;

        initialEnemyScale = enemyTransform.localScale;

        controllerScript.enemyAnimationsScript.ChangeAnimationState(attackAsset.animationClip.name, false);

        if (attackAsset.lockVelocity)
            LockVelocity();

        SoundManager soundManager = SoundManager.Instance;

        if(attackAsset.moveSoundEffect != null)
            attackAsset.moveSoundEffect.PlaySound(soundManager, enemyTransform.position);

        if (attackAsset.crySoundEffect != null)
            attackAsset.crySoundEffect.PlaySound(soundManager, enemyTransform.position);

        controllerScript.hitBoxCheck.HitProperties = attackAsset.hitProperties;

        if (attackAsset.moveType == Moves.MoveType.Projectile && !attackAndProjectile)
            attackAsset.hitProperties = null;
        if (!lockAsyncMethod)
            AttackLoop();

        attackBehaviour?.OnMoveEnter();
    }

    public override void HandleUpdate()
    {
        base.HandleUpdate();

        if (attackAsset.lockSideSwitch)
            LockSideSwitch(initialEnemyScale);
        else
        {
            if(focusedTargetTransform != null)
            {
                directionToFollow = ((Vector2)focusedTargetTransform.position - (Vector2)enemyTransform.position).normalized;
                
            }
            
        }
            
        attackBehaviour?.OnMoveUpdate();
    }

    public override void HandleFixedUpdate()
    {
        base.HandleFixedUpdate();
        attackBehaviour?.OnMoveFixedUpdate();
    }

    public override void Exit()
    {
        base.Exit();

        if(attackAsset.moveType == Moves.MoveType.Projectile)
        {
            controllerScript.AnimationEventWasCalled -= ShootProjectile;
        }
        tokenSource.Cancel();
        controllerScript.hitBoxCheck.ResetProperties();
        controllerScript.AIBrain.StateReset();
        attackBehaviour?.OnMoveExit();

        if(attackBehaviour != null)
            UnityEngine.Object.Destroy(((MonoBehaviour)attackBehaviour).gameObject);
    }

    private async void AttackLoop()
    {
        tokenSource = new CancellationTokenSource();
        var token = tokenSource.Token;

        lockAsyncMethod = true;

        float attackDuration = attackAsset.animationClip.length;
        attackTimer = attackDuration;

        while (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
            await Task.Yield();
        }

        if (token.IsCancellationRequested)
            return;

        stateMachine.ChangeState(new FlyingEnemyIdleState(controllerScript, stateMachine));
    }

    private void LockVelocity()
    {
        controllerScript.enemyRigidBody.velocity = new Vector2(0, 0);
    }
    private void LockSideSwitch(Vector3 initialScale)
    {
        controllerScript.enemySpriteTransform.localScale = initialScale;
    }

    private void ShootProjectile(ScriptableObject obj)
    {
        if (!(obj is ProjectileTriggerEvent projEvent))
            return;

        var instantiatedObj = Object.Instantiate(projEvent.fireballPrefab, controllerScript.enemyProjectileTransform.position, Quaternion.identity, VFXManager.transform);
        var fireball = instantiatedObj.GetComponent<FireballBehaviour>();
        var projectileHitBox = instantiatedObj.GetComponent<ProjectileHitCheck>();
        fireball.target = focusedTargetTransform;
        projectileHitBox.hitInstanceException = controllerScript.GetComponentInChildren<EnemyMainTrigger>();
        
    }

}
