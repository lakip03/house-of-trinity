using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages End Screen UI and statistics
/// </summary>
public class EndScreenManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI congratulationsText;
    public TextMeshProUGUI statsText;
    public Button playAgainButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Statistics Display")]
    public TextMeshProUGUI levelsCompletedText;
    public TextMeshProUGUI totalDeathsText;
    public TextMeshProUGUI totalPlayTimeText;
    public TextMeshProUGUI finalScoreText;

    [Header("Visual Effects")]
    public ParticleSystem celebrationParticles;
    public CanvasGroup fadeGroup;
    public float fadeInDuration = 1f;

    [Header("Audio")]
    public AudioClip victoryMusic;
    public AudioClip buttonClickSound;

    [Header("Scene References")]
    public string mainMenuScene = "MainMenu";

    private AudioSource audioSource;
    private GameFlowData gameData;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        SetupUI();
        StartCoroutine(FadeInScreen());

        // Play victory music
        if (victoryMusic != null)
        {
            audioSource.clip = victoryMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Start celebration effects
        if (celebrationParticles != null)
        {
            celebrationParticles.Play();
        }
    }

    void SetupUI()
    {
        // Setup button listeners
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

        // Initially hide the screen for fade-in effect
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
        }
    }

    public void DisplayGameStats(GameFlowData data)
    {
        gameData = data;

        // Display congratulations message
        if (congratulationsText != null)
        {
            congratulationsText.text = "🎉 CONGRATULATIONS! 🎉\nYou have completed all levels!";
        }

        // Display individual stats
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

        // Calculate and display final score
        int finalScore = CalculateFinalScore(data);
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {finalScore}";
        }

        // Display comprehensive stats
        if (statsText != null)
        {
            statsText.text = GenerateStatsText(data, finalScore);
        }
    }

    string GenerateStatsText(GameFlowData data, int finalScore)
    {
        string formattedTime = FormatTime(data.totalPlayTime);
        float averageDeathsPerLevel = data.levelsCompleted > 0 ? (float)data.playerDeaths / data.levelsCompleted : 0f;

        string performanceRating = GetPerformanceRating(data.playerDeaths, data.totalPlayTime);

        return $@"🏆 GAME COMPLETE! 🏆

📊 Your Statistics:
━━━━━━━━━━━━━━━━━━━━
• Levels Completed: {data.levelsCompleted}
• Total Deaths: {data.playerDeaths}
• Play Time: {formattedTime}
• Avg Deaths/Level: {averageDeathsPerLevel:F1}

🎯 Performance: {performanceRating}
🏅 Final Score: {finalScore} points

Thank you for playing!";
    }

    string GetPerformanceRating(int deaths, float playTime)
    {
        if (deaths == 0)
            return "PERFECT! 🌟";
        else if (deaths <= 3)
            return "EXCELLENT! 🥇";
        else if (deaths <= 6)
            return "GREAT! 🥈";
        else if (deaths <= 10)
            return "GOOD! 🥉";
        else
            return "COMPLETED! 👍";
    }

    int CalculateFinalScore(GameFlowData data)
    {
        // Base score for completion
        int baseScore = data.levelsCompleted * 1000;

        // Bonus for low deaths (max 500 bonus)
        int deathPenalty = Mathf.Min(data.playerDeaths * 50, 500);
        int deathBonus = 500 - deathPenalty;

        // Time bonus (faster completion = higher bonus, max 300)
        int timeBonus = 0;
        if (data.totalPlayTime < 300f) // Under 5 minutes
            timeBonus = 300;
        else if (data.totalPlayTime < 600f) // Under 10 minutes
            timeBonus = 200;
        else if (data.totalPlayTime < 900f) // Under 15 minutes
            timeBonus = 100;

        // Perfect run bonus (no deaths)
        int perfectBonus = data.playerDeaths == 0 ? 1000 : 0;

        return baseScore + deathBonus + timeBonus + perfectBonus;
    }

    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

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
            // Fallback - load main menu
            SceneManager.LoadScene(mainMenuScene);
        }
    }

    void OnMainMenuClicked()
    {
        PlayButtonSound();

        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.ReturnToMainMenu();
        }
        else
        {
            // Fallback
            SceneManager.LoadScene(mainMenuScene);
        }
    }

    void OnQuitClicked()
    {
        PlayButtonSound();

        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.QuitGame();
        }
        else
        {
            // Fallback quit
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }
    }

    void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    // Debug method for testing
    [ContextMenu("Test End Screen")]
    void TestEndScreen()
    {
        GameFlowData testData = ScriptableObject.CreateInstance<GameFlowData>();
        testData.levelsCompleted = 3;
        testData.playerDeaths = 5;
        testData.totalPlayTime = 420f; // 7 minutes

        DisplayGameStats(testData);
    }
}