using UnityEngine;
using System;
using System.Collections;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Invincibility Settings")]
    [SerializeField] private bool isInvincible = false;
    [SerializeField] private float invincibilityDuration = 2f;
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField] private Color invincibleColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    [Header("Audio")]
    public AudioClip deathSound;
    public AudioClip victorySound;
    public AudioClip invincibilitySound;

    [Header("Visual Effects")]
    public ParticleSystem deathParticles;
    public ParticleSystem victoryParticles;
    public ParticleSystem invincibilityParticles;

    // Components
    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Animator animator;

    // Invincibility system
    private Coroutine invincibilityCoroutine;
    private Coroutine blinkCoroutine;

    // Events for better decoupling
    public static event Action<PlayerController, Collision2D> OnAnyPlayerCollision;
    public static event Action<PlayerController> OnInvincibilityStart;
    public static event Action<PlayerController> OnInvincibilityEnd;
    public static event Action<PlayerController> OnPlayerDeath;
    public static event Action<PlayerController> OnPlayerVictory;

    // Properties for external access
    public bool IsInvincible => isInvincible;
    public float InvincibilityTimeRemaining { get; private set; }
    public bool IsAlive { get; private set; } = true;
    public Vector2 MovementInput { get; private set; }


    private Vector2 lastMoveDirection = Vector2.down;

    void Start()
    {
        InitializeComponents();
        RegisterWithSystems();
        SetPlayerColor(normalColor);
        animator = GetComponent<Animator>();
    }

    void InitializeComponents()
    {
        // Get required components
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("PlayerController requires a Rigidbody2D component!");
            enabled = false;
            return;
        }

        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PlayerController requires a PlayerInput component!");
            enabled = false;
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("PlayerController requires a SpriteRenderer component!");
            enabled = false;
            return;
        }

        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Setup input action
        moveAction = playerInput.actions["Move"];
        if (moveAction == null)
        {
            Debug.LogError("Move action not found in PlayerInput! Check input action asset.");
            enabled = false;
            return;
        }
    }

    void RegisterWithSystems()
    {
        // Register with RuleManager
        if (RuleManager.Instance != null)
        {
            RuleManager.Instance.RegisterPlayerController(this);
            Debug.Log("PlayerController registered with RuleManager");
        }
        else
        {
            Debug.LogWarning("RuleManager.Instance is null when PlayerController started!");
            // Try to find and register later
            StartCoroutine(TryRegisterWithRuleManagerLater());
        }

        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.OnGameStateChanged += OnGameStateChanged;
        }
    }

    IEnumerator TryRegisterWithRuleManagerLater()
    {
        float timeout = 5f;
        float elapsed = 0f;

        while (RuleManager.Instance == null && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        if (RuleManager.Instance != null)
        {
            RuleManager.Instance.RegisterPlayerController(this);
            Debug.Log("PlayerController registered with RuleManager (delayed)");
        }
        else
        {
            Debug.LogError("Failed to find RuleManager after timeout!");
        }
    }

    void Update()
    {
        if (!IsAlive) return;

        HandleMovement();
        UpdateInvincibilityTimer();
    }

    private void HandleMovement()
    {
        MovementInput = moveAction.ReadValue<Vector2>();
        rb.linearVelocity = MovementInput * moveSpeed;

        bool isMoving = MovementInput.magnitude > 0.1f;

        animator.SetBool("isWalking", isMoving);

        if (isMoving)
        {
            Vector2 normalizedInput = MovementInput.normalized;

            animator.SetFloat("inputX", normalizedInput.x);
            animator.SetFloat("inputY", normalizedInput.y);

            lastMoveDirection = normalizedInput;
        }
        else
        {
            animator.SetFloat("lastInputX", lastMoveDirection.x);
            animator.SetFloat("lastInputY", lastMoveDirection.y);
        }
    }


    private void UpdateInvincibilityTimer()
    {
        if (isInvincible && InvincibilityTimeRemaining > 0)
        {
            InvincibilityTimeRemaining -= Time.deltaTime;
        }
    }

    #region Public Methods

    public Vector2 GetPlayerDirection()
    {
        return MovementInput;
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public void ResetMoveSpeed(float originalSpeed)
    {
        moveSpeed = originalSpeed;
    }

    public void DisablePlayer()
    {
        IsAlive = false;
        enabled = false;
        rb.linearVelocity = Vector2.zero;
    }

    public void EnablePlayer()
    {
        IsAlive = true;
        enabled = true;
    }

    #endregion

    #region Invincibility System

    public void SetInvincible(float duration)
    {
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
        }

        invincibilityCoroutine = StartCoroutine(InvincibilityCoroutine(duration));
    }

    public void SetInvincibleIndefinitely()
    {
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = null;
        }

        EnableInvincibility();
    }

    public void RemoveInvincibility()
    {
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = null;
        }

        DisableInvincibility();
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        EnableInvincibility();
        InvincibilityTimeRemaining = duration;

        yield return new WaitForSeconds(duration);

        DisableInvincibility();
        invincibilityCoroutine = null;
    }

    private void EnableInvincibility()
    {
        if (isInvincible) return;

        isInvincible = true;
        StartBlinking();
        PlayInvincibilityEffects();
        OnInvincibilityStart?.Invoke(this);

        Debug.Log("Player invincibility enabled");
    }

    private void DisableInvincibility()
    {
        if (!isInvincible) return;

        isInvincible = false;
        InvincibilityTimeRemaining = 0f;
        StopBlinking();
        StopInvincibilityEffects();
        SetPlayerColor(normalColor);
        OnInvincibilityEnd?.Invoke(this);

        Debug.Log("Player invincibility disabled");
    }

    private void StartBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }

    private void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
    }

    private IEnumerator BlinkCoroutine()
    {
        bool useInvincibleColor = true;

        while (isInvincible)
        {
            SetPlayerColor(useInvincibleColor ? invincibleColor : normalColor);
            useInvincibleColor = !useInvincibleColor;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private void SetPlayerColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    #endregion

    #region Audio and Visual Effects

    private void PlayInvincibilityEffects()
    {
        PlaySound(invincibilitySound);
        PlayParticles(invincibilityParticles);
    }

    private void StopInvincibilityEffects()
    {
        StopParticles(invincibilityParticles);
    }

    private void PlayDeathEffects()
    {
        PlaySound(deathSound);
        PlayParticles(deathParticles);
        SetPlayerColor(Color.red); // Death color
    }

    private void PlayVictoryEffects()
    {
        PlaySound(victorySound);
        PlayParticles(victoryParticles);
        SetPlayerColor(Color.green); // Victory color
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void PlayParticles(ParticleSystem particles)
    {
        if (particles != null)
        {
            particles.Play();
        }
    }

    private void StopParticles(ParticleSystem particles)
    {
        if (particles != null)
        {
            particles.Stop();
        }
    }

    #endregion

    #region Collision Handling

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnAnyPlayerCollision?.Invoke(this, collision);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAlive) return;

        if (other.CompareTag("Finish"))
        {
            HandleVictory();
        }
        else if (other.CompareTag("enemy"))
        {
            HandleEnemyCollision();
        }
    }

    private void HandleVictory()
    {
        Debug.Log("Player reached finish line!");

        DisablePlayer();
        PlayVictoryEffects();
        OnPlayerVictory?.Invoke(this);

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.GameWon();
        }
        else
        {
            Debug.LogError("GameStateManager not found! Cannot trigger game win.");
        }
    }

    private void HandleEnemyCollision()
    {
        if (isInvincible)
        {
            Debug.Log("Player hit enemy but is invincible!");
            return;
        }

        Debug.Log("Player killed by enemy!");
        HandleDeath("Caught by enemy");
    }

    private void HandleDeath(string reason)
    {
        if (!IsAlive) return;

        Debug.Log($"Player died: {reason}");

        DisablePlayer();
        PlayDeathEffects();
        OnPlayerDeath?.Invoke(this);

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.GameOver(reason);
        }
        else
        {
            Debug.LogError("GameStateManager not found! Cannot trigger game over.");
        }
    }

    #endregion

    #region Event Handlers

    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.InLevel:
                EnablePlayer();
                break;
            case GameState.GameComplete:
            case GameState.MainMenu:
            case GameState.CardSelection:
                DisablePlayer();
                break;
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Test Invincibility")]
    private void TestInvincibility()
    {
        SetInvincible(invincibilityDuration);
    }

    [ContextMenu("Toggle Indefinite Invincibility")]
    private void ToggleIndefiniteInvincibility()
    {
        if (isInvincible)
            RemoveInvincibility();
        else
            SetInvincibleIndefinitely();
    }

    [ContextMenu("Test Death")]
    private void TestDeath()
    {
        HandleDeath("Debug test");
    }

    [ContextMenu("Test Victory")]
    private void TestVictory()
    {
        HandleVictory();
    }

    #endregion

    #region Cleanup

    void OnDestroy()
    {
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
        }
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.OnGameStateChanged -= OnGameStateChanged;
        }

        StopParticles(deathParticles);
        StopParticles(victoryParticles);
        StopParticles(invincibilityParticles);
    }

    void OnDisable()
    {
        RemoveInvincibility();
        rb.linearVelocity = Vector2.zero;
    }

    #endregion
}