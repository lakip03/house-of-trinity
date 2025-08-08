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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGameFlow();
            EnsureLoadingScreen();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void EnsureLoadingScreen()
    {
        if (LoadingScreenManager.Instance == null)
        {
            GameObject loadingManagerObj = new GameObject("LoadingScreenManager");
            loadingManagerObj.AddComponent<LoadingScreenManager>();
            DontDestroyOnLoad(loadingManagerObj);
            Debug.Log("Created LoadingScreenManager");
        }
    }
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        
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
        currentLevelRules.Clear();
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
        
        if (LoadingScreenManager.Instance != null && LoadingScreenManager.Instance.IsTransitioning)
        {
            Debug.Log("Scene transition in progress, ignoring main menu request");
            return;
        }
        
        ChangeState(GameState.MainMenu);
        
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.LoadSceneWithTransition(mainMenuScene, LoadingType.Normal);
        }
        else
        {
            SceneManager.LoadScene(mainMenuScene);
        }
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
        
        StartCoroutine(ReturnToCardSelectorAfterDeath());
    }
    
    void HandleLevelComplete(string reason)
    {
        Debug.Log($"Level {gameFlowData.currentLevel} completed! (Level {gameFlowData.currentLevel} of {levelScenes.Count})");
        
        PlaySound(levelCompleteSound);
        gameFlowData.levelsCompleted++;
        
        bool isFinalLevel = gameFlowData.currentLevel >= levelScenes.Count;
        
        Debug.Log($"Is final level? {isFinalLevel} (Current: {gameFlowData.currentLevel}, Total: {levelScenes.Count})");
        
        if (isFinalLevel)
        {
            Debug.Log("GAME COMPLETE! Final level beaten! Loading end screen with transition...");
            OnGameCompleted?.Invoke();
            
            if (string.IsNullOrEmpty(endScreenScene))
            {
                Debug.LogError("End screen scene name is empty! Setting to 'EndScreen'");
                endScreenScene = "EndScreen";
            }
            
            if (LoadingScreenManager.Instance != null)
            {
                Debug.Log($"Using LoadingScreenManager to load end screen: {endScreenScene}");
                LoadingScreenManager.Instance.LoadSceneWithTransition(endScreenScene, LoadingType.GameComplete, 2f);
            }
            else
            {
                Debug.LogWarning("LoadingScreenManager not found, using direct load");
                StartCoroutine(LoadEndScreenAfterDelay());
            }
        }
        else
        {
            gameFlowData.currentLevel++;
            Debug.Log($"Moving to level {gameFlowData.currentLevel}");
            OnLevelChanged?.Invoke(gameFlowData.currentLevel);
            
            if (LoadingScreenManager.Instance != null)
            {
                StartCoroutine(LoadCardSelectorWithTransition());
            }
            else
            {
                StartCoroutine(LoadCardSelectorAfterLevelComplete());
            }
        }
    }
    
    IEnumerator LoadCardSelectorWithTransition()
    {
        yield return new WaitForSeconds(1f); // Small delay for victory effects
        LoadingScreenManager.Instance.LoadSceneWithTransition(cardSelectorScene, LoadingType.Victory, 1f);
    }
    
    void SetupCardSelector()
    {
        CardSelectionManager cardManager = FindAnyObjectByType<CardSelectionManager>();
        if (cardManager != null)
        {
            UpdateCardSelectorUI(cardManager);
        }
        else
        {
            Debug.LogWarning("CardSelectionManager not found in scene");
        }
    }
    
    void SetupLevel()
    {
        if (RuleManager.Instance == null)
        {
            Debug.LogWarning("RuleManager not found, creating one");
            GameObject ruleManagerObj = new GameObject("RuleManager");
            ruleManagerObj.AddComponent<RuleManager>();
        }
        
        GameStateManager gameStateManager = FindAnyObjectByType<GameStateManager>();
        if (gameStateManager == null)
        {
            Debug.LogWarning("GameStateManager not found in level scene");
        }
    }
    
    void SetupEndScreen()
    {
        EndScreenManager endScreen = FindAnyObjectByType<EndScreenManager>();
        if (endScreen != null)
        {
            endScreen.DisplayGameStats(gameFlowData);
        }
    }
    
    void UpdateCardSelectorUI(CardSelectionManager cardManager)
    {
        Debug.Log($"Setting up card selector for level {gameFlowData.currentLevel}");
    }
    
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
        yield return null;
        
        if (RuleManager.Instance != null)
        {
            RuleManager.Instance.ForcePlayerSearch();
            
            float timeout = 2f;
            float elapsed = 0f;
            while (!RuleManager.Instance.IsPlayerReady && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            
            if (currentLevelRules.Count > 0)
            {
                Debug.Log($"Reapplying {currentLevelRules.Count} rules to new level");
                
                RuleManager.Instance.ClearAllRules();
                
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
        Debug.Log("LoadEndScreenAfterDelay coroutine started - waiting 3 seconds...");
        yield return new WaitForSeconds(3f);
        
        Debug.Log($"Wait complete. Now loading end screen: {endScreenScene}");
        
        if (string.IsNullOrEmpty(endScreenScene))
        {
            Debug.LogError("endScreenScene is null or empty! Using 'EndScreen' as fallback");
            endScreenScene = "EndScreen";
        }
        
        Time.timeScale = 1f;
        
        try
        {
            Debug.Log($"Attempting to load scene: {endScreenScene}");
            LoadEndScreen();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load end screen: {e.Message}");
            Debug.LogError("Make sure the EndScreen scene is added to Build Settings!");
            
            // Fallback: try to return to main menu
            Debug.Log("Falling back to main menu...");
            SceneManager.LoadScene(mainMenuScene);
        }
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
    
    [ContextMenu("Debug - Load End Screen")]
    public void DebugLoadEndScreen()
    {
        Debug.Log("Debug: Forcing load of end screen");
        LoadEndScreen();
    }
    
    [ContextMenu("Debug - Complete Current Level")]
    public void DebugCompleteCurrentLevel()
    {
        Debug.Log($"Debug: Simulating completion of level {gameFlowData.currentLevel}");
        HandleLevelComplete("Debug completion");
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