using UnityEngine;

public class PlayerControllerRegistrar : MonoBehaviour
{
    public PlayerController playerController;
    
    void Start()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
            
        if (playerController != null && RuleManager.Instance != null)
        {
            RuleManager.Instance.RegisterPlayerController(playerController);
        }
    }
}