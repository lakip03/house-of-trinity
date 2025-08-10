using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "RuleQuantumTunnelling", menuName = "Rules/Movement/Rule Quantum Tunnelling")]
public class RuleQuantumTunnelling : Rule
{
    [Header("Rule Settings")]
    [SerializeField] private float raycastLength = 5f;
    [SerializeField] private Color raycastColor = Color.cyan;
    [SerializeField] private float raycastOffset = 0.5f;
    [SerializeField] private float tunnelDistance = 1.1f;
    [SerializeField] private LayerMask wallLayerMask = -1;
    [SerializeField] private float tunnelCooldown = 0.7f;

    [Header("Screen Shake Settings")]
    [SerializeField] private float shakeIntensity = 0.3f;
    [SerializeField] private float shakeDuration = 0.2f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip tunnelSound;

    private float lastTunnelTime = -1f;
    private static Camera mainCamera;
    private static Coroutine shakeCoroutine;
    Vector3 originalCameraPosition;

    public override void ActivateRule(PlayerController player)
    {
        if (player == null)
        {
            Debug.LogWarning($"[{ruleName}] ActivateRule called with null player - rule will be activated when player is available");
            return;
        }
        lastTunnelTime = -tunnelCooldown;
    }

    public override void DeactivateRule(PlayerController player)
    {
        if (shakeCoroutine != null && mainCamera != null)
        {
            MonoBehaviour cameraComponent = mainCamera.GetComponent<MonoBehaviour>();
            if (cameraComponent != null)
            {
                cameraComponent.StopCoroutine(shakeCoroutine);
                mainCamera.transform.position = originalCameraPosition;
                shakeCoroutine = null;
            }
        }
    }

    public override void UpdateRule(PlayerController player, float deltaTime)
    {
        if (player == null) return;
        Vector2 movementDirection = player.GetPlayerDirection();
        if (movementDirection == Vector2.zero) return;

        if (Time.time - lastTunnelTime < tunnelCooldown) return;

        if (CheckForCollisionTunneling(player, movementDirection))
            return;

        CheckForPreemptiveTunneling(player, movementDirection);
    }

    private bool CheckForCollisionTunneling(PlayerController player, Vector2 movementDirection)
    {
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider == null) return false;

        Collider2D[] overlapping = new Collider2D[10];
        int count = playerCollider.Overlap(new ContactFilter2D().NoFilter(), overlapping);

        for (int i = 0; i < count; i++)
        {
            if (overlapping[i] != null && overlapping[i] != playerCollider)
            {
                if (IsWallObject(overlapping[i]))
                {
                    return AttemptTunnelThroughWall(player, movementDirection.normalized, player.transform.position);
                }
            }
        }

        return false;
    }

    private void CheckForPreemptiveTunneling(PlayerController player, Vector2 movementDirection)
    {
        Vector2 rayDirection = movementDirection.normalized;
        Vector2 playerPos = player.transform.position;
        Vector2 rayOrigin = playerPos + rayDirection * raycastOffset;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, raycastLength, wallLayerMask);

        if (hit.collider != null && IsWallObject(hit.collider))
        {
            AttemptTunnelThroughWall(player, rayDirection, hit.point);
        }

        DrawDebugRaycast(rayOrigin, rayDirection, hit);
    }

    private bool AttemptTunnelThroughWall(PlayerController player, Vector2 direction, Vector2 fromPosition)
    {
        if (!CanTunnelThroughWall(fromPosition, direction))
        {
            return false;
        }

        Vector2 tunnelDestination = fromPosition + direction * tunnelDistance;

        if (IsDestinationClear(tunnelDestination))
        {
            ExecuteTunnel(player, tunnelDestination);
            return true;
        }

        return false;
    }

    private bool CanTunnelThroughWall(Vector2 wallHitPoint, Vector2 direction)
    {
        Vector2 rayStart = wallHitPoint + direction * tunnelDistance;
        Vector2 rayDirection = -direction;

        RaycastHit2D[] hits = Physics2D.RaycastAll(rayStart, rayDirection, tunnelDistance + 1f, wallLayerMask);

        bool hitWall = false;
        for (int i = 0; i < hits.Length; i++)
        {
            if (IsWallObject(hits[i].collider))
            {
                hitWall = true;
                break;
            }
        }

        Debug.DrawLine(rayStart, rayStart + rayDirection * (tunnelDistance + 1f),
                      hitWall ? Color.green : Color.red, 0.1f);

        return hitWall;
    }

    private bool IsDestinationClear(Vector2 destination)
    {
        Collider2D obstruction = Physics2D.OverlapCircle(destination, 0.3f, wallLayerMask);
        return obstruction == null;
    }

    private void ExecuteTunnel(PlayerController player, Vector2 destination)
    {
        Vector3 newPosition = new Vector3(destination.x, destination.y, player.transform.position.z);
        player.transform.position = newPosition;

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        lastTunnelTime = Time.time;

        TriggerScreenShake();
        PlayTunnelSound(player.transform.position);

        Debug.DrawLine(player.transform.position, destination, Color.magenta, 2f);
        Debug.Log($"Tunneled! Next tunnel available in {tunnelCooldown} seconds");
    }

    private void TriggerScreenShake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (shakeCoroutine == null)
        {
            originalCameraPosition = mainCamera.transform.position;
        }

        MonoBehaviour cameraComponent = mainCamera.GetComponent<MonoBehaviour>();
        if (cameraComponent != null)
        {
            if (shakeCoroutine != null)
            {
                cameraComponent.StopCoroutine(shakeCoroutine);
            }
            shakeCoroutine = cameraComponent.StartCoroutine(ScreenShakeCoroutine());
        }
    }
    private IEnumerator ScreenShakeCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            Vector3 randomOffset = new Vector3(
                UnityEngine.Random.Range(-shakeIntensity, shakeIntensity),
                UnityEngine.Random.Range(-shakeIntensity, shakeIntensity),
                0f
            );

            float diminishFactor = 1f - (elapsed / shakeDuration);
            mainCamera.transform.position = originalCameraPosition + randomOffset * diminishFactor;

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = originalCameraPosition;
        shakeCoroutine = null;
    }
    private void PlayTunnelSound(Vector3 position)
    {
        if (tunnelSound != null)
        {
            AudioSource.PlayClipAtPoint(tunnelSound, position, 500f);
        }
    }

    private bool IsWallObject(Collider2D collider)
    {
        return collider.CompareTag("Wall") ||
               collider.name.Contains("Wall") ||
               collider.gameObject.layer == LayerMask.NameToLayer("Wall");
    }

    private void DrawDebugRaycast(Vector2 origin, Vector2 direction, RaycastHit2D hit)
    {
        if (hit.collider != null)
        {
            Debug.DrawLine(origin, hit.point, raycastColor);
            Debug.DrawLine(hit.point, origin + direction * raycastLength, Color.red);
            DrawHitMarker(hit.point);
        }
        else
        {
            Debug.DrawRay(origin, direction * raycastLength, raycastColor);
        }
    }

    private void DrawHitMarker(Vector2 hitPoint)
    {
        float crossSize = 0.2f;
        Debug.DrawLine(hitPoint + Vector2.up * crossSize, hitPoint + Vector2.down * crossSize, Color.yellow);
        Debug.DrawLine(hitPoint + Vector2.left * crossSize, hitPoint + Vector2.right * crossSize, Color.yellow);
    }
}