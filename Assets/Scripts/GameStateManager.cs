using UnityEngine;
using System;

public class GameStateManager : MonoBehaviour
{
    [Header("Game Over Settings")]
    public CanvasGroup gameOverCanvasGroup;

    [Header("Game Win Settings")]
    public CanvasGroup gameWinCanvasGroup;

    public PlayerController player;
    
    [Header("Debug")]
    public bool freezeTimeOnGameOver = true;
    
    // Singleton pattern for easy access
    public static GameStateManager Instance { get; private set; }
    
    // Static events that can be accessed from anywhere
    public static event Action<string> OnGameOver;
    public static event Action<string> OnGameWon;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Don't destroy on load for level-specific GameStateManager
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (gameOverCanvasGroup == null)
        {
            // Try to find game over screen in scene
            GameObject gameOverScreen = GameObject.Find("GameOverScr");
            if (gameOverScreen != null)
            {
                gameOverCanvasGroup = gameOverScreen.GetComponent<CanvasGroup>();
            }
        }
        
        if (gameWinCanvasGroup == null)
        {
            // Try to find win screen in scene
            GameObject winScreen = GameObject.Find("WinScreen");
            if (winScreen != null)
            {
                gameWinCanvasGroup = winScreen.GetComponent<CanvasGroup>();
            }
        }
        
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>();
        }

        if (gameOverCanvasGroup != null)
        {
            HideGameOverScreen();
        }
        
        if (gameWinCanvasGroup != null)
        {
            HideGameWonScreen();
        }
    }

    private void HideGameWonScreen()
    {
         if (gameWinCanvasGroup != null)
        {
            gameWinCanvasGroup.alpha = 0f;
            gameWinCanvasGroup.interactable = false;
            gameWinCanvasGroup.blocksRaycasts = false;
        }
    }

    public void GameWon()
    {
        Debug.Log($"YOU WON YAY :)");

        if (player != null)
        {
            player.enabled = false;
        }
        
        ShowWinScreen();
        
        if (freezeTimeOnGameOver)
        {
            Time.timeScale = 0f;
        }
        
        // Trigger static event
        OnGameWon?.Invoke("Won");
    }

    private void ShowWinScreen()
    {
        if (gameWinCanvasGroup != null)
        {
            gameWinCanvasGroup.alpha = 1f;
            gameWinCanvasGroup.interactable = true;
            gameWinCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            Debug.LogError("GameWin CanvasGroup not found! Make sure WinScreen prefab has a CanvasGroup component.");
        }
    }

    public void GameOver(string reason)
    {
        Debug.Log($"GAME OVER: {reason}");

        if (player != null)
        {
            player.enabled = false;
        }
        
        ShowGameOverScreen();
        
        if (freezeTimeOnGameOver)
        {
            Time.timeScale = 0f;
        }
        
        // Trigger static event
        OnGameOver?.Invoke(reason);
    }
    
    private void ShowGameOverScreen()
    {
        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 1f;
            gameOverCanvasGroup.interactable = true;
            gameOverCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            Debug.LogError("GameOver CanvasGroup not found! Make sure GameOverScr prefab has a CanvasGroup component.");
        }
    }
    
    private void HideGameOverScreen()
    {
        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0f;
            gameOverCanvasGroup.interactable = false;
            gameOverCanvasGroup.blocksRaycasts = false;
        }
    }
    
    public void RestartGame()
    {
        Debug.Log("Restarting current level");
        
        // Reset time scale
        Time.timeScale = 1f;
        
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.RestartCurrentLevel();
        }
        else
        {
            Debug.LogError("GameFlowController not found! Cannot restart level.");
            // Fallback - reload current scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
    
    public void RetryWithNewRules()
    {
        Debug.Log("Going back to card selection");
        
        // Reset time scale
        Time.timeScale = 1f;
        
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.LoadCardSelector();
        }
        else
        {
            Debug.LogError("GameFlowController not found! Cannot return to card selector.");
            // Fallback - try to load card selector scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("CardSelector");
        }
    }
    
    public void ReturnToMainMenu()
    {
        Debug.Log("Returning to main menu");
        
        // Reset time scale
        Time.timeScale = 1f;
        
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.ReturnToMainMenu();
        }
        else
        {
            Debug.LogError("GameFlowController not found! Loading main menu directly.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
    
    public void NextLevel()
    {
        Debug.Log("Proceeding to next level");
        
        // Reset time scale
        Time.timeScale = 1f;
        
        if (GameFlowController.Instance != null)
        {
            // The GameFlowController will handle level progression automatically
            // when GameWon is called, so this method might not be needed
            GameFlowController.Instance.LoadCardSelector();
        }
        else
        {
            Debug.LogError("GameFlowController not found! Cannot proceed to next level.");
        }
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting Game");
        
        // Reset time scale before quitting
        Time.timeScale = 1f;
        
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.QuitGame();
        }
        else
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
    
    void OnDestroy()
    {
        // Reset time scale when destroyed to prevent issues
        Time.timeScale = 1f;
        
        // Clear singleton reference if this was the instance
        if (Instance == this)
        {
            Instance = null;
        }
    }
}