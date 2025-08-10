using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages UI elements within level scenes, including game over and win screens.
/// Works with GameFlowController to provide appropriate button actions.
/// This class handles all user interface interactions during gameplay, including
/// HUD updates, button click handling, and screen transitions.
/// </summary>
public class LevelUIManager : MonoBehaviour
{
    #region UI References
    
    /// <summary>
    /// Button reference for restarting the current level with the same rules.
    /// </summary>
    [Header("Game Over UI")]
    public Button restartLevelButton;
    
    /// <summary>
    /// Button reference for retrying the level with new randomly generated rules.
    /// </summary>
    public Button retryWithNewRulesButton;
    
    /// <summary>
    /// Button reference for returning to the main menu from game over screen.
    /// </summary>
    public Button gameOverMainMenuButton;
    
    /// <summary>
    /// Button reference for quitting the application from game over screen.
    /// </summary>
    public Button gameOverQuitButton;
    
    /// <summary>
    /// Text component displaying the game over message and reason for failure.
    /// </summary>
    public TextMeshProUGUI gameOverText;
    
    /// <summary>
    /// Button reference for proceeding to the next level after winning.
    /// Automatically hidden if the current level is the last level.
    /// </summary>
    [Header("Win Screen UI")]
    public Button nextLevelButton;
    
    /// <summary>
    /// Button reference for returning to the main menu from win screen.
    /// </summary>
    public Button winMainMenuButton;
    
    /// <summary>
    /// Button reference for quitting the application from win screen.
    /// </summary>
    public Button winQuitButton;
    
    /// <summary>
    /// Text component displaying the victory message.
    /// </summary>
    public TextMeshProUGUI winText;
    
    /// <summary>
    /// Text component displaying current level information (e.g., "Level 3/10").
    /// </summary>
    [Header("HUD Elements")]
    public TextMeshProUGUI levelInfoText;
    
    /// <summary>
    /// Text component displaying the current number of lives remaining.
    /// </summary>
    public TextMeshProUGUI livesText;
    
    /// <summary>
    /// Text component displaying the elapsed play time in MM:SS format.
    /// </summary>
    public TextMeshProUGUI timeText;
    
    /// <summary>
    /// Audio clip played when any button is clicked for auditory feedback.
    /// </summary>
    [Header("Audio")]
    public AudioClip buttonClickSound;
    
    #endregion
    
    /// <summary>
    /// AudioSource component for playing button click sounds.
    /// Created automatically if not present on the GameObject.
    /// </summary>
    private AudioSource audioSource;
    
    /// <summary>
    /// Initializes the UI manager by setting up audio, button listeners, and updating level information.
    /// Called automatically by Unity when the GameObject becomes active.
    /// </summary>
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
    
    /// <summary>
    /// Configures all button onClick listeners and sets up appropriate visibility states.
    /// Handles null checks for all button references and configures next level button
    /// visibility based on whether the current level is the final level.
    /// </summary>
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
    
    /// <summary>
    /// Updates the level information display with current level and total level count.
    /// Retrieves information from GameFlowController if available.
    /// </summary>
    void UpdateLevelInfo()
    {
        if (levelInfoText != null && GameFlowController.Instance != null)
        {
            int currentLevel = GameFlowController.Instance.CurrentLevel;
            int totalLevels = GameFlowController.Instance.TotalLevels;
            levelInfoText.text = $"Level {currentLevel}/{totalLevels}";
        }
    }
    
    /// <summary>
    /// Unity Update method called once per frame.
    /// Currently only handles time display updates.
    /// </summary>
    void Update()
    {
        UpdateTimeDisplay();
    }
    
    /// <summary>
    /// Updates the time display with the current total play time in MM:SS format.
    /// Retrieves play time from GameFlowController and formats it for display.
    /// </summary>
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
    
    /// <summary>
    /// Restarts the current level with the same rules and conditions.
    /// Attempts to use GameStateManager first, then falls back to GameFlowController.
    /// Logs an error if neither manager is available.
    /// </summary>
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
    
    /// <summary>
    /// Retries the current level with newly generated rules or conditions.
    /// Typically loads the card selector screen for rule generation.
    /// Attempts to use GameStateManager first, then falls back to GameFlowController.
    /// </summary>
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
    
    /// <summary>
    /// Proceeds to the next level in the sequence.
    /// Loads the card selector screen for the next level's rule generation.
    /// Requires GameFlowController to be available.
    /// </summary>
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
    
    /// <summary>
    /// Returns the player to the main menu scene.
    /// Attempts to use GameStateManager first, then GameFlowController.
    /// Falls back to direct scene loading if no managers are available.
    /// </summary>
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
    
    /// <summary>
    /// Quits the application entirely.
    /// Attempts to use GameStateManager first, then GameFlowController.
    /// Falls back to direct application quit if no managers are available.
    /// In editor mode, stops play mode instead of quitting.
    /// </summary>
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
    
    /// <summary>
    /// Plays the button click sound effect for auditory feedback.
    /// Only plays if both AudioSource and button click sound are assigned.
    /// </summary>
    void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
    
    /// <summary>
    /// Sets the game over text display with a custom reason for failure.
    /// Formats the text as "GAME OVER" followed by the provided reason.
    /// </summary>
    /// <param name="reason">The specific reason why the game ended (e.g., "Out of Time", "No Lives Remaining")</param>
    public void SetGameOverText(string reason)
    {
        if (gameOverText != null)
        {
            gameOverText.text = $"GAME OVER\n{reason}";
        }
    }
}