﻿using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyMainController : MonoBehaviour, IDamageable, IEntityController
{
    public SoundManager SoundManager { get; protected set; }

    public float MovementX;
    public float MovementY;
    [HideInInspector] public bool isReversed = false;

    [FoldoutGroup("Dependencies", expanded: false)]
    public AnimationsState enemyAnimationsScript;
    [FoldoutGroup("Dependencies")]
    public Collider2D enemyCollider;
    [FoldoutGroup("Dependencies")]
    public Rigidbody2D enemyRigidBody;
    [FoldoutGroup("Dependencies")]
    public Transform enemySpriteTransform;
    [FoldoutGroup("Dependencies")]
    public SpriteRenderer enemySpriteRenderer;
    [FoldoutGroup("Dependencies")]
    public Transform groundCheck;
    [FoldoutGroup("Dependencies")]
    public Transform groundDetectionLeft;
    [FoldoutGroup("Dependencies")]
    public Transform groundDetectionRight;
    [FoldoutGroup("Dependencies")]
    public Transform wallDetectionLeft;
    [FoldoutGroup("Dependencies")]
    public Transform wallDetectionRight;
    [FoldoutGroup("Dependencies")]
    public AnimationClip idleAnimationClip;
    [FoldoutGroup("Dependencies")]
    public AnimationClip hitAnimationClip;
    [FoldoutGroup("Dependencies")]
    public AnimationClip fallAnimationClip;
    [FoldoutGroup("Dependencies")]
    public AnimationClip deathAnimationClip;
    [FoldoutGroup("Dependencies")]
    public CollectionSounds hitSound;
    [FoldoutGroup("Dependencies")]
    public CollectionSounds deathSound;
    [FoldoutGroup("Dependencies")]
    public EnemyAIBrain AIBrain;
    [FoldoutGroup("Dependencies")]
    public Transform enemyProjectileTransform;
    [FoldoutGroup("Dependencies")]
    public HitCheck hitBoxCheck;
    [FoldoutGroup("Dependencies")]
    public FlipSprite spriteFlip;
    [FoldoutGroup("Dependencies")]
    public MainVFXManager enemyVFXManager;

    [TitleGroup("Enemy", Alignment = TitleAlignments.Centered)]
    [TabGroup("Enemy/Tabs", "Movement Settings")]
    [Range(0, 1f)] public float groundedStunnedToIdleEasingRate = 0.6f;
    [TabGroup("Enemy/Tabs", "Movement Settings")]
    [Range(0, 1f)] public float airborneStunnedToIdleEasingRate = 0.6f;
    [TabGroup("Enemy/Tabs", "Movement Settings")]
    public float stunnedMaxTime = 0.4f;
    [TabGroup("Enemy/Tabs", "Movement Settings")]
    [SerializeField] public float enemySpeed;

    [TabGroup("Enemy/Tabs", "Collision Checks")]
    [TabGroup("Enemy/Tabs/Collision Checks/SubTabGroup", "Ground Check")]
    public float groundCheckRadius = 0.25f;
    [TabGroup("Enemy/Tabs", "Collision Checks")]
    [TabGroup("Enemy/Tabs/Collision Checks/SubTabGroup", "Ground Detection")]
    public float groundDetectionLRadius = 0.25f;
    [TabGroup("Enemy/Tabs/Collision Checks/SubTabGroup", "Ground Detection")]
    public float groundDetectionRRadius = 0.25f;
    [TabGroup("Enemy/Tabs", "Collision Checks")]
    [TabGroup("Enemy/Tabs/Collision Checks/SubTabGroup", "Wall Detection")]
    public float wallDetectionLRadius = 0.25f;
    [TabGroup("Enemy/Tabs/Collision Checks/SubTabGroup", "Wall Detection")]
    public float wallDetectionRRadius = 0.25f;

    [TabGroup("Enemy/Tabs", "Collision Checks")]
    public LayerMask groundMask;

    [TabGroup("Enemy/Tabs", "Combat")]
    public int maxHealth;
    [TabGroup("Enemy/Tabs", "Combat")]
    [ReadOnly]
    public int currentHealth;
    [TabGroup("Enemy/Tabs", "Combat")]
    [ColorUsage(true, true)]
    public Color hitColor;

    [TabGroup("Enemy/Tabs", "Debug")]
    [SerializeField] private bool debugActivated = true;
    public MainStateMachine StateMachine { get; protected set; }
    [TabGroup("Enemy/Tabs", "Debug")]
    [ShowIf("debugActivated")] [ReadOnly] public string currentStateOutput;
    [TabGroup("Enemy/Tabs", "Debug")]
    [ShowIf("debugActivated")] [ReadOnly] public bool isGrounded;

    #region Enemy Events
    public Action<ScriptableObject> AnimationEventWasCalled { get; set; }
    public Action hasDied;
    #endregion

    #region Enemy Coroutines
    private IEnumerator flashCoroutine;
    public IEnumerator stunnedCoroutine;
    #endregion

    #region Animation Event Exclusive Methods
    public void AnimationSendObject(ScriptableObject obj)
    {
        AnimationEventWasCalled?.Invoke(obj);
    }

    #endregion
    private void Start()
    {
        SoundManager = SoundManager.Instance;
        currentHealth = maxHealth;

        StateMachine = new MainStateMachine();

        StateMachine.onStateChanged += state => currentStateOutput = state;
        StateMachine.Init(new EnemyStandingState(this, StateMachine));

    }

    private void Update()
    {
        StateMachine.CurrentState.HandleUpdate();
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.HandleFixedUpdate(); 
    }

    private void OnDrawGizmosSelected()
    {
        if (debugActivated)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            Gizmos.DrawWireSphere(groundDetectionLeft.position, groundDetectionLRadius);
            Gizmos.DrawWireSphere(groundDetectionRight.position, groundDetectionRRadius);
            Gizmos.DrawWireSphere(wallDetectionLeft.position, wallDetectionLRadius);
            Gizmos.DrawWireSphere(wallDetectionRight.position, wallDetectionRRadius);
        }

    }
    public virtual void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
            return;
        currentHealth -= damage;

        
        if(currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = HitFlash(enemySpriteRenderer, 0.5f);
        StartCoroutine(flashCoroutine);

  
    }

    public virtual void TakeDamage(int damage, Vector2 forceDirection, float knockbackForce)
    {
        if (currentHealth <= 0)
            return;
        currentHealth -= damage;
        
        enemyRigidBody.AddForce(forceDirection * knockbackForce, ForceMode2D.Impulse);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }
        else
        {
            if (hitSound != null)
                hitSound.PlaySound(SoundManager, enemySpriteTransform.position);
        }

        if(flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = HitFlash(enemySpriteRenderer, 3.5f);
        StartCoroutine(flashCoroutine);

        StateMachine.ChangeState(new EnemyHitStunnedState(this, StateMachine));
   
    }

    protected virtual void Die()
    {
        if (deathSound != null)
            deathSound.PlaySound(SoundManager, enemySpriteTransform.position);
        hasDied?.Invoke();
        StateMachine.ChangeState(new EnemyDeathState(this, StateMachine));
    }

    IEnumerator HitFlash(Renderer renderer, float secondsToRecover)
    {
        float lerpRate = 0;
        Color previousMaterialColor = renderer.material.GetColor("_SpriteColor");
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_SpriteColor", hitColor);
        renderer.SetPropertyBlock(propertyBlock);

        
        yield return null;
        Color currentColor = hitColor;
        
        while (true)
        {
            if (lerpRate >= 1)
                break;

            lerpRate += Time.deltaTime / secondsToRecover;

            currentColor = Color.Lerp(currentColor, previousMaterialColor, lerpRate);
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_SpriteColor", currentColor);
            renderer.SetPropertyBlock(propertyBlock);

            yield return null;
        }



    }

    public bool Flip(Transform _transform, float movementValue)
    {
        bool isFacingRight = _transform.localScale.x > 0;
        bool willFlip = isFacingRight && movementValue < 0 || !isFacingRight && movementValue > 0;
        if (willFlip)
        {
            _transform.localScale =
                    new Vector2(_transform.localScale.x * -1,
                     _transform.localScale.y);

        }
        return isFacingRight;
    }

    public void SetMovement(Vector2 direction)
    {
        MovementX = direction.x;
        MovementY = direction.y;
    }
}
