using UnityEngine;

/// <summary>
/// Bootstrap script to ensure GameFlowController exists in the game.
/// </summary>
public class GameFlowControllerBootstrap : MonoBehaviour
{
    [Header("GameFlowController Settings")]
    public GameObject gameFlowControllerPrefab;
    
    [Header("Level Configuration")]
    [Tooltip("List of level scene names in order")]
    public string[] levelScenes = new string[]
    {
        "Level1",
        "Level2", 
        "Level3"
    };
    
    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string cardSelectorScene = "CardSelector";
    public string endScreenScene = "EndScreen";
    
    void Awake()
    {
        // Only create if one doesn't already exist
        if (GameFlowController.Instance == null)
        {
            CreateGameFlowController();
        }
        
        Destroy(gameObject);
    }
    
    void CreateGameFlowController()
    {
        GameObject flowControllerObj;
        
        if (gameFlowControllerPrefab != null)
        {
            // Use the prefab if assigned
            flowControllerObj = Instantiate(gameFlowControllerPrefab);
            flowControllerObj.name = "GameFlowController";
            Debug.Log("GameFlowController created from prefab");
        }
        else
        {
            // Create basic GameFlowController
            flowControllerObj = new GameObject("GameFlowController");
            GameFlowController controller = flowControllerObj.AddComponent<GameFlowController>();
            
            // Configure the controller with our settings
            ConfigureGameFlowController(controller);
            Debug.Log("Basic GameFlowController created and configured");
        }
    }
    
    void ConfigureGameFlowController(GameFlowController controller)
    {
        // Set scene names
        controller.mainMenuScene = mainMenuScene;
        controller.cardSelectorScene = cardSelectorScene;
        controller.endScreenScene = endScreenScene;
        
        // Set level scenes
        controller.levelScenes.Clear();
        foreach (string levelScene in levelScenes)
        {
            if (!string.IsNullOrEmpty(levelScene))
            {
                controller.levelScenes.Add(levelScene);
            }
        }
        
        Debug.Log($"Configured GameFlowController with {controller.levelScenes.Count} levels");
    }
}