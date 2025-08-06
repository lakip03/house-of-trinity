using UnityEngine;
using System;
using System.Collections;
using UnityEngine.InputSystem;

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

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private SpriteRenderer plyr;
    private Coroutine invincibilityCoroutine;
    private Coroutine blinkCoroutine;

    // Events for better decoupling
    public static event Action<PlayerController, Collision2D> OnAnyPlayerCollision;
    public static event Action<PlayerController> OnInvincibilityStart;
    public static event Action<PlayerController> OnInvincibilityEnd;

    // Properties for external access
    public bool IsInvincible => isInvincible;
    public float InvincibilityTimeRemaining { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        plyr = GetComponent<SpriteRenderer>();

        SetPlayerColor(normalColor);
        moveAction = playerInput.actions["Move"];

        if (RuleManager.Instance != null)
        {
            RuleManager.Instance.RegisterPlayerController(this);
            Debug.Log("PlayerController registered with RuleManager");
        }
        else
        {
            Debug.LogError("RuleManager.Instance is null when PlayerController started!");
        }
    }

    void Update()
    {
        HandleMovement();
        UpdateInvincibilityTimer();
    }

    private void HandleMovement()
    {
        Vector2 movementInput = moveAction.ReadValue<Vector2>();
        rb.linearVelocity = movementInput * moveSpeed;
    }

    private void UpdateInvincibilityTimer()
    {
        if (isInvincible && InvincibilityTimeRemaining > 0)
        {
            InvincibilityTimeRemaining -= Time.deltaTime;
        }
    }

    public Vector2 GetPlayerDirection()
    {
        return moveAction.ReadValue<Vector2>();
    }

    // Public methods for controlling invincibility
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
        OnInvincibilityStart?.Invoke(this);
    }

    private void DisableInvincibility()
    {
        if (!isInvincible) return;

        isInvincible = false;
        InvincibilityTimeRemaining = 0f;
        StopBlinking();
        SetPlayerColor(normalColor);
        OnInvincibilityEnd?.Invoke(this);
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
        if (plyr != null)
        {
            plyr.color = color;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Always trigger the event, let subscribers decide what to do based on invincibility state
        OnAnyPlayerCollision?.Invoke(this, collision);
    }

    // Debug methods (remove in production)
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Finish"))
        {
            Debug.Log("YAY YOU WON:) COLLIDED WITH FINISH");
            GameStateManager gameStateManager = FindAnyObjectByType<GameStateManager>();
            gameStateManager.GameWon();
        }
        if (other.CompareTag("enemy"))
        {
            Debug.Log("OH NO YOU DIED");
            GameStateManager gameStateManager = FindAnyObjectByType<GameStateManager>();
            if (isInvincible)
            {
                return;
            }
            gameStateManager.GameOver("Death by enemy");
        }
    }
}