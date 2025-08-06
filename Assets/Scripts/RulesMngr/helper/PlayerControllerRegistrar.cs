using UnityEngine;

public class PlayerControllerRegistrar : MonoBehaviour
{
    public PlayerController playerController;
    
    void Start()
    {
        // Auto-find if not assigned
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
            
        // Register with RuleManager
        if (playerController != null && RuleManager.Instance != null)
        {
            RuleManager.Instance.RegisterPlayerController(playerController);
        }
    }
}