using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main menu UI controller with dynamic continue button visibility based on game progress.
/// Handles navigation to game flow and includes fallback system creation for missing controllers.
/// </summary>
public class MainMenuScript : MonoBehaviour
{
    [Header("Menu Buttons")]
    public Button startButton;
    public Button continueButton;
    public Button exitButton;

    
    [Header("Audio")]
    public AudioClip buttonClickSound;
    public AudioClip menuMusic;
    
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        SetupUI();
        PlayMenuMusic();
    }
    
    void SetupUI()
    {
        // Setup button listeners
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartClicked);
        }
        
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
            UpdateContinueButtonVisibility();
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitClicked);
        }
        
    }
    
    void UpdateContinueButtonVisibility()
    {
        if (continueButton == null) return;
        
        bool hasGameInProgress = false;
        
        if (GameFlowController.Instance != null)
        {
            hasGameInProgress = GameFlowController.Instance.CurrentLevel > 1;
        }
        
        continueButton.gameObject.SetActive(hasGameInProgress);
    }
    
    void PlayMenuMusic()
    {
        if (audioSource != null && menuMusic != null)
        {
            audioSource.clip = menuMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    
    public void OnStartClicked()
    {
        PlayButtonSound();
        
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.StartNewGame();
        }
        else
        {
            Debug.LogError("GameFlowController not found! Creating one...");
            CreateGameFlowController();
            GameFlowController.Instance.StartNewGame();
        }
    }
    
    public void OnContinueClicked()
    {
        PlayButtonSound();
        
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.ContinueGame();
        }
        else
        {
            Debug.LogWarning("GameFlowController not found! Starting new game instead.");
            OnStartClicked();
        }
    }
    
    public void OnExitClicked()
    {
        PlayButtonSound();
        
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.QuitGame();
        }
        else
        {
            QuitGame();
        }
    }
    
    void CreateGameFlowController()
    {
        GameObject flowControllerObj = new GameObject("GameFlowController");
        GameFlowController controller = flowControllerObj.AddComponent<GameFlowController>();
        
        Debug.Log("Created GameFlowController from MainMenu");
    }
    
    void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
    
    void QuitGame()
    {
        Debug.Log("Quitting Game");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void OnGameFlowStateChanged(GameState newState)
    {
        if (newState == GameState.MainMenu)
        {
            UpdateContinueButtonVisibility();
        }
    }
    
    void OnEnable()
    {
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.OnGameStateChanged += OnGameFlowStateChanged;
        }
    }
    
    void OnDisable()
    {
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.OnGameStateChanged -= OnGameFlowStateChanged;
        }
    }
}