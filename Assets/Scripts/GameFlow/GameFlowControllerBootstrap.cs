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
            flowControllerObj = Instantiate(gameFlowControllerPrefab);
            flowControllerObj.name = "GameFlowController";
            Debug.Log("GameFlowController created from prefab");
        }
        else
        {
            flowControllerObj = new GameObject("GameFlowController");
            GameFlowController controller = flowControllerObj.AddComponent<GameFlowController>();
            
            ConfigureGameFlowController(controller);
            Debug.Log("Basic GameFlowController created and configured");
        }
    }
    
    void ConfigureGameFlowController(GameFlowController controller)
    {
        controller.mainMenuScene = mainMenuScene;
        controller.cardSelectorScene = cardSelectorScene;
        controller.endScreenScene = endScreenScene;
        
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