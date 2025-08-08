using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameFlowController : MonoBehaviour
{
    [Header("Game Flow Settings")]
    public GameFlowData gameFlowData;
    
    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string cardSelectorScene = "CardSelector";
    public string endScreenScene = "EndScreen";
    
    [Header("Level Progression")]
    public List<string> levelScenes = new List<string>
    {
        "Level1",
        "Level2", 
        "Level3"
    };
    
    [Header("Audio")]
    public AudioClip levelCompleteSound;
    public AudioClip gameOverSound;
    public AudioClip gameCompleteSound;
    
    // Singleton pattern
    public static GameFlowController Instance { get; private set; }
    
    // Events
    public System.Action<int> OnLevelChanged;
    public System.Action<GameState> OnGameStateChanged;
    public System.Action OnGameCompleted;
    public System.Action OnPlayerDied;
    
    // Properties
    public int CurrentLevel => gameFlowData.currentLevel;
    public int TotalLevels => levelScenes.Count;
    public GameState CurrentState => gameFlowData.currentState;
    public int PlayerDeaths => gameFlowData.playerDeaths;
    public float TotalPlayTime => gameFlowData.totalPlayTime;
    
    private AudioSource audioSource;
    private List<Rule> currentLevelRules = new List<Rule>(); // Store rules between levels
    
    void Awake()
    {
        // Singleton setup with persistence
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGameFlow();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Subscribe to scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Subscribe to static game events
        GameStateManager.OnGameOver += HandlePlayerDeath;
        GameStateManager.OnGameWon += HandleLevelComplete;
    }
    
    void Update()
    {
        if (gameFlowData.currentState == GameState.InLevel)
        {
            gameFlowData.totalPlayTime += Time.deltaTime;
        }
    }
    
    void InitializeGameFlow()
    {
        if (gameFlowData == null)
        {
            gameFlowData = ScriptableObject.CreateInstance<GameFlowData>();
            gameFlowData.Initialize();
        }
        
        Debug.Log("GameFlowController initialized");
    }
    
    public void StartNewGame()
    {
        Debug.Log("Starting new game");
        gameFlowData.Initialize();
        currentLevelRules.Clear(); // Clear any stored rules
        ChangeState(GameState.CardSelection);
        LoadCardSelector();
    }
    
    public void ContinueGame()
    {
        Debug.Log($"Continuing game from level {gameFlowData.currentLevel}");
        ChangeState(GameState.CardSelection);
        LoadCardSelector();
    }
    
    public void ReturnToMainMenu()
    {
        Debug.Log("Returning to main menu");
        ChangeState(GameState.MainMenu);
        SceneManager.LoadScene(mainMenuScene);
    }
    
    public void LoadCardSelector()
    {
        Debug.Log($"Loading card selector for level {gameFlowData.currentLevel}");
        ChangeState(GameState.CardSelection);
        SceneManager.LoadScene(cardSelectorScene);
    }
    
    public void StartCurrentLevel()
    {
        if (gameFlowData.currentLevel <= 0 || gameFlowData.currentLevel > levelScenes.Count)
        {
            Debug.LogError($"Invalid level: {gameFlowData.currentLevel}");
            return;
        }
        
        // Store current rules before loading the level
        StoreCurrentRules();
        
        string levelScene = levelScenes[gameFlowData.currentLevel - 1];
        Debug.Log($"Starting level {gameFlowData.currentLevel}: {levelScene}");
        
        ChangeState(GameState.InLevel);
        SceneManager.LoadScene(levelScene);
    }
    
    public void RestartCurrentLevel()
    {
        Debug.Log($"Restarting level {gameFlowData.currentLevel}");
        StartCurrentLevel();
    }
    
    public void LoadEndScreen()
    {
        Debug.Log("Loading end screen - Game Complete!");
        ChangeState(GameState.GameComplete);
        PlaySound(gameCompleteSound);
        SceneManager.LoadScene(endScreenScene);
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        
        // Handle scene-specific setup
        if (scene.name == mainMenuScene)
        {
            ChangeState(GameState.MainMenu);
        }
        else if (scene.name == cardSelectorScene)
        {
            ChangeState(GameState.CardSelection);
            SetupCardSelector();
        }
        else if (levelScenes.Contains(scene.name))
        {
            ChangeState(GameState.InLevel);
            SetupLevel();
            // Reapply rules after level loads
            StartCoroutine(ReapplyRulesAfterSceneLoad());
        }
        else if (scene.name == endScreenScene)
        {
            ChangeState(GameState.GameComplete);
            SetupEndScreen();
        }
    }
    
    void HandlePlayerDeath(string reason)
    {
        Debug.Log($"Player died: {reason}");
        gameFlowData.playerDeaths++;
        
        PlaySound(gameOverSound);
        OnPlayerDied?.Invoke();
        
        // Return to card selector after a delay
        StartCoroutine(ReturnToCardSelectorAfterDeath());
    }
    
    void HandleLevelComplete(string reason)
    {
        Debug.Log($"Level {gameFlowData.currentLevel} completed!");
        
        PlaySound(levelCompleteSound);
        gameFlowData.levelsCompleted++;
        
        // FIX: Check if this is the final level BEFORE incrementing
        if (gameFlowData.currentLevel >= levelScenes.Count)
        {
            // Game completed!
            Debug.Log("Final level completed! Loading end screen...");
            OnGameCompleted?.Invoke();
            StartCoroutine(LoadEndScreenAfterDelay());
        }
        else
        {
            // Move to next level
            gameFlowData.currentLevel++;
            Debug.Log($"Moving to level {gameFlowData.currentLevel}");
            OnLevelChanged?.Invoke(gameFlowData.currentLevel);
            StartCoroutine(LoadCardSelectorAfterLevelComplete());
        }
    }
    
    void SetupCardSelector()
    {
        // Find and configure the CardSelectionManager
        CardSelectionManager cardManager = FindAnyObjectByType<CardSelectionManager>();
        if (cardManager != null)
        {
            // Update UI to show current level
            UpdateCardSelectorUI(cardManager);
        }
        else
        {
            Debug.LogWarning("CardSelectionManager not found in scene");
        }
    }
    
    void SetupLevel()
    {
        // Ensure RuleManager is properly set up
        if (RuleManager.Instance == null)
        {
            Debug.LogWarning("RuleManager not found, creating one");
            GameObject ruleManagerObj = new GameObject("RuleManager");
            ruleManagerObj.AddComponent<RuleManager>();
        }
        
        // Find GameStateManager for reference
        GameStateManager gameStateManager = FindAnyObjectByType<GameStateManager>();
        if (gameStateManager == null)
        {
            Debug.LogWarning("GameStateManager not found in level scene");
        }
    }
    
    void SetupEndScreen()
    {
        // Find and setup end screen with game statistics
        EndScreenManager endScreen = FindAnyObjectByType<EndScreenManager>();
        if (endScreen != null)
        {
            endScreen.DisplayGameStats(gameFlowData);
        }
    }
    
    void UpdateCardSelectorUI(CardSelectionManager cardManager)
    {
        // This could be enhanced to show level-specific information
        Debug.Log($"Setting up card selector for level {gameFlowData.currentLevel}");
    }
    
    // Store current rules before changing levels
    void StoreCurrentRules()
    {
        if (RuleManager.Instance != null)
        {
            currentLevelRules.Clear();
            currentLevelRules.AddRange(RuleManager.Instance.activeRules);
            Debug.Log($"Stored {currentLevelRules.Count} rules for next level");
        }
    }
    
    // Reapply rules after scene load
    IEnumerator ReapplyRulesAfterSceneLoad()
    {
        // Wait a frame to ensure PlayerController is initialized
        yield return null;
        
        // Force RuleManager to find the new PlayerController
        if (RuleManager.Instance != null)
        {
            RuleManager.Instance.ForcePlayerSearch();
            
            // Wait for player to be registered
            float timeout = 2f;
            float elapsed = 0f;
            while (!RuleManager.Instance.IsPlayerReady && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            
            // If we have stored rules, reapply them
            if (currentLevelRules.Count > 0)
            {
                Debug.Log($"Reapplying {currentLevelRules.Count} rules to new level");
                
                // Clear any existing rules first
                RuleManager.Instance.ClearAllRules();
                
                // Reapply stored rules
                foreach (Rule rule in currentLevelRules)
                {
                    if (rule != null)
                    {
                        RuleManager.Instance.AddRule(rule);
                        Debug.Log($"Reapplied rule: {rule.ruleName}");
                    }
                }
            }
        }
    }
    
    IEnumerator ReturnToCardSelectorAfterDeath()
    {
        yield return new WaitForSeconds(2f); // Wait for death animation/UI
        LoadCardSelector();
    }
    
    IEnumerator LoadCardSelectorAfterLevelComplete()
    {
        yield return new WaitForSeconds(2f); // Wait for victory animation/UI
        LoadCardSelector();
    }
    
    IEnumerator LoadEndScreenAfterDelay()
    {
        yield return new WaitForSeconds(3f); // Wait for final victory celebration
        LoadEndScreen();
    }
    
    void ChangeState(GameState newState)
    {
        if (gameFlowData.currentState != newState)
        {
            gameFlowData.currentState = newState;
            OnGameStateChanged?.Invoke(newState);
            Debug.Log($"Game state changed to: {newState}");
        }
    }
    
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // Public method for debugging/testing
    [ContextMenu("Debug - Next Level")]
    public void DebugNextLevel()
    {
        if (gameFlowData.currentLevel < levelScenes.Count)
        {
            gameFlowData.currentLevel++;
            Debug.Log($"Debug: Advanced to level {gameFlowData.currentLevel}");
        }
    }
    
    [ContextMenu("Debug - Reset Game")]
    public void DebugResetGame()
    {
        gameFlowData.Initialize();
        currentLevelRules.Clear();
        Debug.Log("Debug: Game data reset");
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameStateManager.OnGameOver -= HandlePlayerDeath;
        GameStateManager.OnGameWon -= HandleLevelComplete;
    }
}