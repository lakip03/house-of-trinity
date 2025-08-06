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
    
    // Events
    public static event Action<string> OnGameOver;
    public static event Action<string> OnGameWon;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
            gameOverCanvasGroup = FindAnyObjectByType<CanvasGroup>();
        }
        
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>();
        }

        if (gameOverCanvasGroup != null)
        {
            HideGameOverScreen();
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
            Debug.LogError("GameWin CanvasGroup not found! Make sure GameWin prefab has a GameWin component.");
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
        Time.timeScale = 1f;
        
        HideGameOverScreen();
        
        if (player != null)
        {
            player.enabled = true;
        }
        
        Debug.Log("Game Restarted");
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting Game");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}