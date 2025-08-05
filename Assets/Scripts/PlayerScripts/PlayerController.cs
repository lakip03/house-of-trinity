using UnityEngine;
using System;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private InputAction moveAction;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        
    }

    void Update()
    {
        Vector2 movementInput = moveAction.ReadValue<Vector2>();
        rb.linearVelocity = movementInput * moveSpeed;
    }
}