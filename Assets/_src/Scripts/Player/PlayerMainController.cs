﻿using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[HideMonoScript]
public class PlayerMainController : MonoBehaviour, IDamageable, IEntityController
{
    public SoundManager SoundManager { get; private set; }
    private ControlManager controlManager;
    public InputMaster Controls { get; private set; }
    [HideInInspector] public bool isHittingHead;
    [HideInInspector] public int attacksInTheAir = 0;
    [HideInInspector] public bool isReversed = false;
    public float MovementX { get; private set; }
    public float MovementY { get; private set; }
    public bool IsHoldingJumpButton { get; private set; }

    [FoldoutGroup("Dependencies", expanded: false)]
    public Transform groundCheck;
    [FoldoutGroup("Dependencies")]
    public Transform ceilingCheck;
    [FoldoutGroup("Dependencies")]
    public Rigidbody2D playerRigidBody;
    [FoldoutGroup("Dependencies")]
    public CapsuleCollider2D playerMainCollider;
    [FoldoutGroup("Dependencies")]
    public AnimationsState playerAnimationsScript;
    [FoldoutGroup("Dependencies")]
    public CollectionSounds hitSound;
    [FoldoutGroup("Dependencies")]
    public CollectionSounds deathSound;
    [FoldoutGroup("Dependencies")]
    public Transform playerSpriteTransform;
    [FoldoutGroup("Dependencies")]
    public SpriteRenderer playerSpriteRenderer;
    [FoldoutGroup("Dependencies")]
    public HitCheck hitBoxCheck;
    [FoldoutGroup("Dependencies")]
    public PlayerInputHandler playerInputHandler;
    [FoldoutGroup("Dependencies")]
    public PlayerMoveList playerMoveList;
    [FoldoutGroup("Dependencies")]
    public MainVFXManager playerMainVFXManager;
    [FoldoutGroup("Dependencies")]
    public Transform playerProjectileTransform;

    [TitleGroup("Player", Alignment = TitleAlignments.Centered)]
    [TabGroup("Player/Tabs", "Movement Settings")]
    public float standingMoveSpeed = 10;
    [TabGroup("Player/Tabs", "Movement Settings")]
    public float crouchingMoveSpeed = 5;
    [TabGroup("Player/Tabs", "Movement Settings")]
    [Range(0.01f, 1)] public float standingEasingRate = 0.6f;
    [TabGroup("Player/Tabs", "Movement Settings")]
    [Range(0.01f, 1)] public float airborneEasingRate = 0.6f;
    [TabGroup("Player/Tabs", "Movement Settings")]
    public float stunnedMaxTime = 0.4f;
    [TabGroup("Player/Tabs", "Movement Settings")]
    public float jumpSpeed = 2;
    [TabGroup("Player/Tabs", "Movement Settings")]
    public float jumpHeight = 5;
    [TabGroup("Player/Tabs", "Movement Settings")]
    public float airborneJumpDelay = 0.2f;
    [TabGroup("Player/Tabs", "Movement Settings")]
    public float groundedJumpDelay = 0.2f;
    [TabGroup("Player/Tabs", "Movement Settings")]
    [HideInInspector] public float airborneJumpTimer;
    [TabGroup("Player/Tabs", "Movement Settings")]
    [HideInInspector] public float groundedJumpTimer;
    [TabGroup("Player/Tabs", "Movement Settings")]
    public float fallMultiplier = 1.5f;
    [TabGroup("Player/Tabs", "Movement Settings")]
    [Range(0, 1f)] public float groundedStunnedToIdleEasingRate = 0.6f;
    [TabGroup("Player/Tabs", "Movement Settings")]
    [Range(0, 1f)] public float airborneStunnedToIdleEasingRate = 0.6f;


    [TabGroup("Player/Tabs", "Collision Checks")]
    [TabGroup("Player/Tabs/Collision Checks/SubTabGroup", "Ground Check")]
    public float groundCheckRadius = 1f;
    [TabGroup("Player/Tabs/Collision Checks/SubTabGroup", "Ground Check")]
    public LayerMask groundMask;

    [TabGroup("Player/Tabs", "Collision Checks")]
    [TabGroup("Player/Tabs/Collision Checks/SubTabGroup", "Ceiling Check")]
    public float ceilingCheckRadius = 1f;
    [TabGroup("Player/Tabs/Collision Checks/SubTabGroup", "Ceiling Check")]
    public LayerMask ceilingMask;

    [TabGroup("Player/Tabs", "Combat")]
    public int maxHealth;
    [TabGroup("Player/Tabs", "Combat")]
    [ReadOnly]
    public int currentHealth;
    public const int ORB_CAPACITY = 100;
    [TabGroup("Player/Tabs", "Combat")]
    public int maxPowerOrbs;
    [TabGroup("Player/Tabs", "Combat")]
    [ReadOnly]
    public int currentPowerOrbMeter;
    [TabGroup("Player/Tabs", "Combat")]
    public int maxVitalityOrbs;
    [TabGroup("Player/Tabs", "Combat")]
    [ReadOnly]
    public int currentVitalityOrbMeter;
    [TabGroup("Player/Tabs", "Combat")]
    [ColorUsage(true, true)]
    public Color hitColor;

    [TabGroup("Player/Tabs", "Debug")]
    [SerializeField] private bool debugActivated = true;
    [TabGroup("Player/Tabs", "Debug")]
    [ShowIf("debugActivated")] [ReadOnly] public bool isGrounded;

    public MainStateMachine StateMachine { get; private set; }
    [TabGroup("Player/Tabs", "Debug")]
    [ShowIf("debugActivated")]
    [ReadOnly]
    public string currentStateOutput;

    #region Input Events
    public Action<InputAction.CallbackContext> kickAction;
    public Action<InputAction.CallbackContext> punchAction;
    #endregion
    #region Player Events
    public Action<ScriptableObject> AnimationEventWasCalled { get; set; }
    public Action hasPerformedJump;
    public Action hasShotAProjectile;
    public Action<int> hasDamaged;
    public Action<int> hasChangedPowerOrbMeter;
    public Action<int> hasChangedVitalityOrbMeter;
    public Action hasDied;
    #endregion
    #region Odin Inspector Conditions
    private bool areControlsEnabled;
    #endregion
    #region Player Coroutines
    private IEnumerator flashCoroutine;
    public IEnumerator stunnedCoroutine;
    #endregion

    #region Animation Event Exclusive Methods
    public void AnimationSendObject(ScriptableObject obj)
    {
        AnimationEventWasCalled?.Invoke(obj);
    }

    #endregion

    private void Awake()
    {
        currentHealth = maxHealth;
        currentPowerOrbMeter = maxPowerOrbs * ORB_CAPACITY;
        currentVitalityOrbMeter = maxVitalityOrbs * ORB_CAPACITY;
    }
    private void Start()
    {
        
        SoundManager = SoundManager.Instance;
        controlManager = ControlManager.Instance;
        Controls = controlManager.controls;

        #region Input Handling
        Controls.Player.Jump.performed += _ => airborneJumpTimer = Time.time + airborneJumpDelay;
        Controls.Player.Jump.started += _ => IsHoldingJumpButton = true;
        Controls.Player.Jump.canceled += _ => IsHoldingJumpButton = false;

        Controls.Player.Movement.performed += ctx =>
        {
            MovementX = ctx.ReadValue<Vector2>().x;
            MovementY = ctx.ReadValue<Vector2>().y;
        };
        Controls.Player.Movement.canceled += _ =>
        {
            MovementX = 0;
            MovementY = 0;
        };


        #endregion

        StateMachine = new MainStateMachine();
        
        StateMachine.onStateChanged += state => currentStateOutput = state;
        StateMachine.Init(new PlayerStandingState(this, StateMachine));

        controlManager.ClearObjects();
        EnableControls();
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
            Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);
        }
        
    }

    private void OnDestroy()
    {
        //TODO: These don't actually unsubscribe themselves
        Controls.Player.Jump.performed -= _ => airborneJumpTimer = Time.time + airborneJumpDelay;
        Controls.Player.Jump.started -= _ => IsHoldingJumpButton = true;
        Controls.Player.Jump.canceled -= _ => IsHoldingJumpButton = false;

        Controls.Player.Movement.performed -= ctx => MovementX = ctx.ReadValue<Vector2>().x;
        Controls.Player.Movement.canceled -= _ => MovementX = 0;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
            return;


        currentHealth -= damage;
        hasDamaged?.Invoke(damage);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();

            return;
        }


        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = HitFlash(playerSpriteRenderer, 1f);
        StartCoroutine(flashCoroutine);
    }
    public void TakeDamage(int damage, Vector2 forceDirection, float knockbackForce)
    {
        if (currentHealth <= 0)
            return;
        currentHealth -= damage;
        hasDamaged?.Invoke(damage);
        float originalYVelocity = playerRigidBody.velocity.y;
        playerRigidBody.AddForce(forceDirection * knockbackForce, ForceMode2D.Impulse);
        playerRigidBody.velocity = new Vector2(playerRigidBody.velocity.x, originalYVelocity);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }
        else
        {
            if (hitSound != null)
                hitSound.PlaySound(SoundManager, playerSpriteTransform.position);
        }

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = HitFlash(playerSpriteRenderer, 4f);
        StartCoroutine(flashCoroutine);

        StateMachine.ChangeState(new PlayerHitStunnedState(this, StateMachine));
        

        
    }

    private void Die()
    {
        if (deathSound != null)
            deathSound.PlaySound(SoundManager, playerSpriteTransform.position);
        hasDied?.Invoke();
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
    public void DepletePowerOrbMeter(int amount)
    {
        currentPowerOrbMeter -= amount;
        if (currentPowerOrbMeter < 0)
            currentPowerOrbMeter = 0;
        hasChangedPowerOrbMeter?.Invoke(currentPowerOrbMeter);
    }

    public void FillPowerOrbMeter(int amount)
    {
        currentPowerOrbMeter += amount;
        if (currentPowerOrbMeter > maxPowerOrbs * ORB_CAPACITY)
            currentPowerOrbMeter = maxPowerOrbs * ORB_CAPACITY;
        hasChangedPowerOrbMeter?.Invoke(currentPowerOrbMeter);
    }
    public void DepleteVitalityOrbMeter(int amount)
    {
        currentVitalityOrbMeter -= amount;
        if (currentVitalityOrbMeter < 0)
            currentVitalityOrbMeter = 0;
        hasChangedVitalityOrbMeter?.Invoke(currentVitalityOrbMeter);
    }

    public void FillVitalityOrbMeter(int amount)
    {
        currentVitalityOrbMeter += amount;
        if (currentVitalityOrbMeter > maxVitalityOrbs * ORB_CAPACITY)
            currentVitalityOrbMeter = maxVitalityOrbs * ORB_CAPACITY;
        hasChangedVitalityOrbMeter?.Invoke(currentVitalityOrbMeter);
    }

    [HideIf("areControlsEnabled")]
    [Button("Enable Player Controls")]
    public void EnableControls()
    {
        controlManager.EnablePlayerControls(gameObject);
        areControlsEnabled = true;
    }

    [ShowIf("areControlsEnabled")]
    [Button("Disable Player Controls")]
    public void DisableControls()
    {
        controlManager.DisablePlayerControls(gameObject);
        areControlsEnabled = false;
    }

    [Button("Freeze Horizontal Movement")]
    public void FreezeHorizontalMovement()
    {
        StateMachine.ChangeState(new PlayerStandingState(this, StateMachine));
        playerRigidBody.velocity = new Vector2(0, playerRigidBody.velocity.y);
    }

}
