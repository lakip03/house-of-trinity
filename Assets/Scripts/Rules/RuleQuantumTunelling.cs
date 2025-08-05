using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RuleQuantumTunelling", menuName = "Rules/Movement/Rule Quantum Tunelling")]
public class RuleQuantumTunelling : Rule
{
    private LayerMask wallLayerMask = -1;
    private float tunnelDistance = 1f;
    private float cooldownTime = 0.5f;
    private float lastTunnelTime = -1f;
    private bool wasCollidingLastFrame = false;

    public override void ActivateRule(PlayerController player)
    {
        lastTunnelTime = -1f;
        wasCollidingLastFrame = false;

    }
    public override void DeactivateRule(PlayerController player)
    {
        wasCollidingLastFrame = false;
        if (player.TryGetComponent<Collider2D>(out var collider))
        {
            collider.isTrigger = false;
        }
    }
    public override void UpdateRule(PlayerController player, float deltaTime)
    {
        if (!isActive) return;
        if (duration > 0)
        {
            duration -= deltaTime;
            if (duration <= 0)
            {
                OnRuleRemoved();
                return;
            }
        }
        CheckForTunneling(player);
    }

    private void CheckForTunneling(PlayerController player)
    {
        Vector2 inputDirection = GetPlayerInputDirection();
    }
    
     private Vector2 GetPlayerInputDirection(PlayerController player)
    {
        Vector2 input = Vector2.zero;
        
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        
        return input.normalized;
    }
    
    private bool IsPositionValid(Vector3 position)
    {
        Collider2D hit = Physics2D.OverlapPoint(position, wallLayerMask);
        return hit == null;
    }
}
