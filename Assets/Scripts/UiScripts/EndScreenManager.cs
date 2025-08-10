using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Controls the end screen UI in a Unity game, displaying performance statistics,
/// managing button interactions, playing victory effects, and handling scene transitions.
/// </summary>
public class EndScreenManager : MonoBehaviour
{
    [Header("UI References")]
    /// <summary>
    /// Text element displaying a congratulatory message upon completing the game.
    /// </summary>
    public TextMeshProUGUI congratulationsText;

    /// <summary>
    /// Optional extra stats text (currently unused in logic).
    /// </summary>
    public TextMeshProUGUI statsText;

    /// <summary>
    /// Button for restarting the game from the beginning.
    /// </summary>
    public Button playAgainButton;

    /// <summary>
    /// Button for returning to the main menu.
    /// </summary>
    public Button mainMenuButton;

    /// <summary>
    /// Button for quitting the game entirely.
    /// </summary>
    public Button quitButton;

    [Header("Statistics Display")]
    /// <summary>
    /// UI element showing the number of levels completed.
    /// </summary>
    public TextMeshProUGUI levelsCompletedText;

    /// <summary>
    /// UI element showing the total number of player deaths.
    /// </summary>
    public TextMeshProUGUI totalDeathsText;

    /// <summary>
    /// UI element showing the total play time in mm:ss format.
    /// </summary>
    public TextMeshProUGUI totalPlayTimeText;

    /// <summary>
    /// UI element showing the final calculated score.
    /// </summary>
    public TextMeshProUGUI finalScoreText;

    [Header("Visual Effects")]
    /// <summary>
    /// Particle effect played on victory.
    /// </summary>
    public ParticleSystem celebrationParticles;

    /// <summary>
    /// CanvasGroup used to fade in the end screen UI.
    /// </summary>
    public CanvasGroup fadeGroup;

    /// <summary>
    /// Duration in seconds for the fade-in animation.
    /// </summary>
    public float fadeInDuration = 1f;

    [Header("Audio")]
    /// <summary>
    /// Music clip played during the victory screen.
    /// </summary>
    public AudioClip victoryMusic;

    /// <summary>
    /// Sound effect played when a button is clicked.
    /// </summary>
    public AudioClip buttonClickSound;

    [Header("Scene References")]
    /// <summary>
    /// Name of the main menu scene to load when returning to the main menu.
    /// </summary>
    public string mainMenuScene = "MainMenu";

    private AudioSource audioSource;
    private GameFlowData gameData;

    /// <summary>
    /// Unity lifecycle method. Initializes the audio source, configures UI listeners,
    /// starts the fade-in animation, plays music, and triggers celebration effects.
    /// </summary>
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        SetupUI();
        StartCoroutine(FadeInScreen());

        if (victoryMusic != null)
        {
            audioSource.clip = victoryMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (celebrationParticles != null)
        {
            celebrationParticles.Play();
        }
    }

    /// <summary>
    /// Sets up button event listeners and initializes fade group transparency.
    /// </summary>
    void SetupUI()
    {
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// Populates the end screen UI with game statistics and the calculated final score.
    /// </summary>
    /// <param name="data">The <see cref="GameFlowData"/> containing performance information.</param>
    public void DisplayGameStats(GameFlowData data)
    {
        gameData = data;

        if (congratulationsText != null)
        {
            congratulationsText.text = " CONGRATULATIONS! \nYou have completed all levels!";
        }

        if (levelsCompletedText != null)
        {
            levelsCompletedText.text = $"Levels Completed: {data.levelsCompleted}";
        }

        if (totalDeathsText != null)
        {
            totalDeathsText.text = $"Total Deaths: {data.playerDeaths}";
        }

        if (totalPlayTimeText != null)
        {
            string formattedTime = FormatTime(data.totalPlayTime);
            totalPlayTimeText.text = $"Total Play Time: {formattedTime}";
        }

        int finalScore = CalculateFinalScore(data);
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {finalScore}";
        }
    }

    /// <summary>
    /// Calculates the player's final score based on performance metrics.
    /// 
    /// Scoring breakdown:
    /// - Base Score: 1000 points per completed level.
    /// - Death Bonus: Starts at 500 points, reduced by 50 points per death (max penalty 500 points).
    /// - Time Bonus: Additional points for faster completion:
    ///     &lt; 5 min   → +300 points
    ///     &lt; 10 min  → +200 points
    ///     &lt; 15 min  → +100 points
    /// - Perfect Bonus: +1000 points if no deaths occurred.
    /// </summary>
    /// <param name="data">The <see cref="GameFlowData"/> containing performance information.</param>
    /// <returns>The total calculated score.</returns>
    int CalculateFinalScore(GameFlowData data)
    {
        int baseScore = data.levelsCompleted * 1000;

        int deathPenalty = Mathf.Min(data.playerDeaths * 50, 500);
        int deathBonus = 500 - deathPenalty;

        int timeBonus = 0;
        if (data.totalPlayTime < 300f)
            timeBonus = 300;
        else if (data.totalPlayTime < 600f)
            timeBonus = 200;
        else if (data.totalPlayTime < 900f)
            timeBonus = 100;

        int perfectBonus = data.playerDeaths == 0 ? 1000 : 0;

        return baseScore + deathBonus + timeBonus + perfectBonus;
    }

    /// <summary>
    /// Formats a time value from seconds into a mm:ss string.
    /// </summary>
    /// <param name="timeInSeconds">The time value in seconds.</param>
    /// <returns>A formatted string in mm:ss format.</returns>
    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// Coroutine to gradually fade in the end screen UI.
    /// </summary>
    IEnumerator FadeInScreen()
    {
        if (fadeGroup == null) yield break;

        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }

        fadeGroup.alpha = 1f;
    }

    /// <summary>
    /// Called when the "Play Again" button is clicked. Restarts the game or returns to the main menu if the GameFlowController is missing.
    /// </summary>
    void OnPlayAgainClicked()
    {
        PlayButtonSound();

        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.StartNewGame();
        }
        else
        {
            Debug.LogError("GameFlowController not found! Cannot restart game.");
            SceneManager.LoadScene(mainMenuScene);
        }
    }

    /// <summary>
    /// Called when the "Main Menu" button is clicked. Loads the main menu scene.
    /// </summary>
    void OnMainMenuClicked()
    {
        PlayButtonSound();

        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.ReturnToMainMenu();
        }
        else
        {
            SceneManager.LoadScene(mainMenuScene);
        }
    }

    /// <summary>
    /// Called when the "Quit" button is clicked. Quits the game or stops play mode in the Unity Editor.
    /// </summary>
    void OnQuitClicked()
    {
        PlayButtonSound();

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

    /// <summary>
    /// Plays the button click sound effect if available.
    /// </summary>
    void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    /// <summary>
    /// Test utility method that simulates displaying the end screen with sample data.
    /// Can be triggered from the Unity context menu.
    /// </summary>
    [ContextMenu("Test End Screen")]
    void TestEndScreen()
    {
        GameFlowData testData = ScriptableObject.CreateInstance<GameFlowData>();
        testData.levelsCompleted = 3;
        testData.playerDeaths = 5;
        testData.totalPlayTime = 420f;

        DisplayGameStats(testData);
    }
}
