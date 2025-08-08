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
    
    public static GameStateManager Instance { get; private set; }
    
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
            GameObject gameOverScreen = GameObject.Find("GameOverScr");
            if (gameOverScreen != null)
            {
                gameOverCanvasGroup = gameOverScreen.GetComponent<CanvasGroup>();
            }
        }
        
        if (gameWinCanvasGroup == null)
        {
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
        
        bool isFinalLevel = false;
        if (GameFlowController.Instance != null)
        {
            isFinalLevel = GameFlowController.Instance.CurrentLevel >= GameFlowController.Instance.TotalLevels;
            Debug.Log($"GameWon - Is final level? {isFinalLevel} (Level {GameFlowController.Instance.CurrentLevel} of {GameFlowController.Instance.TotalLevels})");
        }
        
        if (!isFinalLevel)
        {
            ShowWinScreen();
            
            if (freezeTimeOnGameOver)
            {
                Time.timeScale = 0f;
            }
        }
        else
        {
            Debug.Log("Final level completed - skipping win screen, letting GameFlowController handle end screen transition");
        }
        
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
        
        Time.timeScale = 1f;
        
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.RestartCurrentLevel();
        }
        else
        {
            Debug.LogError("GameFlowController not found! Cannot restart level.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
    
    public void RetryWithNewRules()
    {
        Debug.Log("Going back to card selection");
        
        Time.timeScale = 1f;
        
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.LoadCardSelector();
        }
        else
        {
            Debug.LogError("GameFlowController not found! Cannot return to card selector.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("CardSelector");
        }
    }
    
    public void ReturnToMainMenu()
    {
        Debug.Log("Returning to main menu");
        
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
        
        Time.timeScale = 1f;
        
        if (GameFlowController.Instance != null)
        {
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
        Time.timeScale = 1f;
        
        if (Instance == this)
        {
            Instance = null;
        }
    }
}