using UnityEngine;

[CreateAssetMenu(fileName = "RuleQuantumTunnelling", menuName = "Rules/Movement/Rule Quantum Tunnelling")]
public class RuleQuantumTunnelling : Rule
{
    
     [Header("Quantum Tunneling Settings")]
    [SerializeField] private string thinWallTag = "ThinWall"; // Use tags instead of layers for easier setup
    [SerializeField] private LayerMask solidObjectsLayer = -1; // Check against all layers by default
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float detectionDistance = 1.25f;
    [SerializeField] private float teleportCooldown = 0.5f;
    [SerializeField] private bool debugMode = true; // Enable detailed logging
    
    private float lastTeleportTime;
    private bool isSubscribedToCollision;
    
    public override void ActivateRule(PlayerController player)
    {
        if (!isSubscribedToCollision)
        {
            PlayerController.OnAnyPlayerCollision += HandlePlayerCollision;
            isSubscribedToCollision = true;
            
            if (debugMode) Debug.Log($"[QuantumTunneling] Rule activated and subscribed to collisions");
        }
    }
    
    public override void DeactivateRule(PlayerController player)
    {
        if (isSubscribedToCollision)
        {
            PlayerController.OnAnyPlayerCollision -= HandlePlayerCollision;
            isSubscribedToCollision = false;
            
            if (debugMode) Debug.Log($"[QuantumTunneling] Rule deactivated and unsubscribed");
        }
    }
    
    public override void UpdateRule(PlayerController player, float deltaTime)
    {
        if (duration > 0)
        {
            duration -= deltaTime;
            if (duration <= 0)
            {
                OnRuleRemoved();
            }
        }
    }
    
    private void HandlePlayerCollision(PlayerController player, Collision2D collision)
    {
        if (debugMode) Debug.Log($"[QuantumTunneling] Collision detected with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        
        // Check cooldown
        if (Time.time - lastTeleportTime < teleportCooldown)
        {
            if (debugMode) Debug.Log($"[QuantumTunneling] Cooldown active. Time since last: {Time.time - lastTeleportTime}");
            return;
        }
        
        // Check if collision is with a thin wall using tag
        if (!collision.gameObject.CompareTag(thinWallTag))
        {
            if (debugMode) Debug.Log($"[QuantumTunneling] Not a thin wall. Expected tag: {thinWallTag}, Got: {collision.gameObject.tag}");
            return;
        }
        
        if (debugMode) Debug.Log($"[QuantumTunneling] Thin wall detected! Attempting quantum tunnel...");
        AttemptQuantumTunnel(player, collision);
    }
    
    private void AttemptQuantumTunnel(PlayerController player, Collision2D collision)
    {
        Vector2 playerPos = player.transform.position;
        
        // Get movement direction from player's velocity or input
        Vector2 movementDirection = GetMovementDirection(player, collision);
        
        if (movementDirection == Vector2.zero)
        {
            if (debugMode) Debug.Log("[QuantumTunneling] No movement direction detected");
            return;
        }
        
        // Calculate target position
        Vector2 targetPosition = CalculateTargetPosition(playerPos, movementDirection);
        
        if (debugMode) Debug.Log($"[QuantumTunneling] Player at: {playerPos}, Moving: {movementDirection}, Target: {targetPosition}");
        
        // Check if target position is safe
        if (IsTargetPositionSafe(targetPosition, player))
        {
            PerformTeleport(player, targetPosition);
            lastTeleportTime = Time.time;
            
            if (debugMode) Debug.Log($"[QuantumTunneling] SUCCESS! Teleported to {targetPosition}");
        }
        else
        {
            if (debugMode) Debug.Log("[QuantumTunneling] Target position is not safe");
        }
    }
    
    private Vector2 GetMovementDirection(PlayerController player, Collision2D collision)
    {
        // Try to get direction from player's rigidbody velocity
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null && playerRb.linearVelocity.magnitude > 0.1f)
        {
            return playerRb.linearVelocity.normalized;
        }
        
        // Fallback: use collision normal (opposite direction)
        if (collision.contacts.Length > 0)
        {
            return -collision.contacts[0].normal.normalized;
        }
        
        // Last resort: try input direction (you might need to modify this based on your input system)
        Vector2 inputDir = player.GetPlayerDirection();
        if (inputDir.magnitude > 0.1f)
        {
            return inputDir.normalized;
        }
        
        return Vector2.zero;
    }
    
    private Vector2 CalculateTargetPosition(Vector2 playerPos, Vector2 direction)
    {
        // Move exactly one tile in the movement direction
        Vector2 targetPos = playerPos + (direction * tileSize);
        
        // Optional: Snap to grid (comment out if your game doesn't use grid alignment)
        // targetPos.x = Mathf.Round(targetPos.x / tileSize) * tileSize;
        // targetPos.y = Mathf.Round(targetPos.y / tileSize) * tileSize;
        
        return targetPos;
    }
    
    private bool IsTargetPositionSafe(Vector2 targetPosition, PlayerController player)
    {
        // Get player's collider for size reference
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            if (debugMode) Debug.LogWarning("[QuantumTunneling] Player has no collider!");
            return false;
        }
        
        Vector2 colliderSize = playerCollider.bounds.size;
        
        // Check for overlapping colliders at target position
        Collider2D[] overlapping = Physics2D.OverlapBoxAll(targetPosition, colliderSize * 0.9f, 0f);
        
        foreach (var col in overlapping)
        {
            // Skip the player's own collider and thin walls
            if (col == playerCollider || col.CompareTag(thinWallTag))
                continue;
                
            if (debugMode) Debug.Log($"[QuantumTunneling] Target blocked by: {col.name}");
            return false;
        }
        
        // Additional raycast check in movement direction
        RaycastHit2D hit = Physics2D.Raycast(targetPosition, Vector2.zero, 0.1f, solidObjectsLayer);
        if (hit.collider != null && !hit.collider.CompareTag(thinWallTag))
        {
            if (debugMode) Debug.Log($"[QuantumTunneling] Raycast hit: {hit.collider.name}");
            return false;
        }
        
        if (debugMode) Debug.Log("[QuantumTunneling] Target position is safe!");
        return true;
    }
    
    private void PerformTeleport(PlayerController player, Vector2 targetPosition)
    {
        // Store original physics state
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        Vector2 originalVelocity = Vector2.zero;
        
        if (playerRb != null)
        {
            originalVelocity = playerRb.linearVelocity;
            playerRb.linearVelocity = Vector2.zero; // Stop movement during teleport
        }
        
        // Teleport
        player.transform.position = targetPosition;
        
        // Restore some velocity to maintain momentum
        if (playerRb != null)
        {
            playerRb.linearVelocity = originalVelocity * 0.5f; // Reduce velocity by half
        }
        
        // Visual effect placeholder
        CreateTeleportEffect(targetPosition);
    }
    
    private void CreateTeleportEffect(Vector2 position)
    {
        // Add particle effects, screen shake, sound, etc. here
        if (debugMode) Debug.Log($"[QuantumTunneling] *TELEPORT EFFECT* at {position}");
    }
    
    private void OnDestroy()
    {
        if (isSubscribedToCollision)
        {
            PlayerController.OnAnyPlayerCollision -= HandlePlayerCollision;
        }
    }
}