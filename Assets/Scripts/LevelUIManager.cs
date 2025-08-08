using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages UI elements within level scenes, including game over and win screens
/// Works with GameFlowController to provide appropriate button actions
/// </summary>
public class LevelUIManager : MonoBehaviour
{
    [Header("Game Over UI")]
    public Button restartLevelButton;
    public Button retryWithNewRulesButton;
    public Button gameOverMainMenuButton;
    public Button gameOverQuitButton;
    public TextMeshProUGUI gameOverText;
    
    [Header("Win Screen UI")]
    public Button nextLevelButton;
    public Button winMainMenuButton;
    public Button winQuitButton;
    public TextMeshProUGUI winText;
    
    [Header("HUD Elements")]
    public TextMeshProUGUI levelInfoText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI timeText;
    
    [Header("Audio")]
    public AudioClip buttonClickSound;
    
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        SetupButtons();
        UpdateLevelInfo();
    }
    
    void SetupButtons()
    {
        if (restartLevelButton != null)
        {
            restartLevelButton.onClick.AddListener(() => {
                PlayButtonSound();
                RestartLevel();
            });
        }
        
        if (retryWithNewRulesButton != null)
        {
            retryWithNewRulesButton.onClick.AddListener(() => {
                PlayButtonSound();
                RetryWithNewRules();
            });
        }
        
        if (gameOverMainMenuButton != null)
        {
            gameOverMainMenuButton.onClick.AddListener(() => {
                PlayButtonSound();
                ReturnToMainMenu();
            });
        }
        
        if (gameOverQuitButton != null)
        {
            gameOverQuitButton.onClick.AddListener(() => {
                PlayButtonSound();
                QuitGame();
            });
        }
        
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(() => {
                PlayButtonSound();
                NextLevel();
            });
            
            if (GameFlowController.Instance != null)
            {
                bool isLastLevel = GameFlowController.Instance.CurrentLevel >= GameFlowController.Instance.TotalLevels;
                nextLevelButton.gameObject.SetActive(!isLastLevel);
            }
        }
        
        if (winMainMenuButton != null)
        {
            winMainMenuButton.onClick.AddListener(() => {
                PlayButtonSound();
                ReturnToMainMenu();
            });
        }
        
        if (winQuitButton != null)
        {
            winQuitButton.onClick.AddListener(() => {
                PlayButtonSound();
                QuitGame();
            });
        }
    }
    
    void UpdateLevelInfo()
    {
        if (levelInfoText != null && GameFlowController.Instance != null)
        {
            int currentLevel = GameFlowController.Instance.CurrentLevel;
            int totalLevels = GameFlowController.Instance.TotalLevels;
            levelInfoText.text = $"Level {currentLevel}/{totalLevels}";
        }
    }
    
    void Update()
    {
        UpdateTimeDisplay();
    }
    
    void UpdateTimeDisplay()
    {
        if (timeText != null && GameFlowController.Instance != null)
        {
            float playTime = GameFlowController.Instance.TotalPlayTime;
            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);
            timeText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }
    
    #region Button Actions
    
    void RestartLevel()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RestartGame();
        }
        else if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.RestartCurrentLevel();
        }
        else
        {
            Debug.LogError("No GameStateManager or GameFlowController found!");
        }
    }
    
    void RetryWithNewRules()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RetryWithNewRules();
        }
        else if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.LoadCardSelector();
        }
        else
        {
            Debug.LogError("No GameStateManager or GameFlowController found!");
        }
    }
    
    void NextLevel()
    {
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.LoadCardSelector();
        }
        else
        {
            Debug.LogError("GameFlowController not found!");
        }
    }
    
    void ReturnToMainMenu()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ReturnToMainMenu();
        }
        else if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.ReturnToMainMenu();
        }
        else
        {
            Debug.LogError("No GameStateManager or GameFlowController found!");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
    
    void QuitGame()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.QuitGame();
        }
        else if (GameFlowController.Instance != null)
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
    
    #endregion
    
    void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
    
    public void SetGameOverText(string reason)
    {
        if (gameOverText != null)
        {
            gameOverText.text = $"GAME OVER\n{reason}";
        }
    }
    


}