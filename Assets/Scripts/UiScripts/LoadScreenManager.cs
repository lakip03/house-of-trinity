using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Manages scene loading transitions with fade effects, progress display, and randomized messages.
/// 
/// Features:
/// - Singleton pattern to ensure a single loading manager persists across scenes.
/// - Dynamically creates a loading screen UI if none is assigned in the Inspector.
/// - Displays different message sets depending on loading type:
///     • Normal loading
///     • Victory loading
///     • Game complete loading
/// - Fade-in and fade-out animations with customizable durations.
/// 
/// Typical usage:
/// Call <see cref="LoadSceneWithTransition"/> with a target scene name and optional 
/// <see cref="LoadingType"/> to initiate a loading sequence.
/// </summary>
/// <remarks>
/// Attach this script to a persistent GameObject in your starting scene.
/// The script will survive scene loads due to <see cref="Object.DontDestroyOnLoad"/>.
/// </remarks>

public class LoadingScreenManager : MonoBehaviour
{
    [Header("Loading Screen UI")]
    public GameObject loadingScreenPanel;
    public Image fadeImage;
    public TextMeshProUGUI loadingText;
    public Image loadingProgressBar;
    public TextMeshProUGUI tipsText;

    [Header("Fade Settings")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;
    public float displayDuration = 2f;

    [Header("Loading Messages")]
    public string[] loadingMessages = new string[]
    {
        "Loading...",
        "Preparing next challenge...",
        "Get ready...",
        "Almost there..."
    };

    public string[] victoryMessages = new string[]
    {
        "Victory! Preparing next level...",
        "Well done! Loading...",
        "Excellent! Get ready for more..."
    };

    public string[] gameCompleteMessages = new string[]
    {
        "Congratulations! You've completed all levels!",
        "Amazing! Preparing your results...",
        "Victory is yours! Loading final screen..."
    };

    // Singleton
    public static LoadingScreenManager Instance { get; private set; }

    private bool isTransitioning = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (loadingScreenPanel == null)
            {
                CreateLoadingScreenUI();
            }

            if (loadingScreenPanel != null)
            {
                loadingScreenPanel.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void CreateLoadingScreenUI()
    {
        GameObject canvasObj = new GameObject("LoadingCanvas");
        canvasObj.transform.SetParent(transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        loadingScreenPanel = new GameObject("LoadingPanel");
        loadingScreenPanel.transform.SetParent(canvasObj.transform, false);

        fadeImage = loadingScreenPanel.AddComponent<Image>();
        fadeImage.color = Color.black;
        RectTransform panelRect = loadingScreenPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        GameObject textObj = new GameObject("LoadingText");
        textObj.transform.SetParent(loadingScreenPanel.transform, false);
        loadingText = textObj.AddComponent<TextMeshProUGUI>();
        loadingText.text = "Loading...";
        loadingText.fontSize = 48;
        loadingText.color = Color.white;
        loadingText.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(800, 100);
        textRect.anchoredPosition = new Vector2(0, 0);
    }

    public void LoadSceneWithTransition(string sceneName, LoadingType loadingType = LoadingType.Normal, float extraDelay = 0f)
    {
        if (!isTransitioning)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName, loadingType, extraDelay));
        }
    }

    IEnumerator LoadSceneCoroutine(string sceneName, LoadingType loadingType, float extraDelay)
    {
        isTransitioning = true;

        string message = GetLoadingMessage(loadingType);
        if (loadingText != null)
        {
            loadingText.text = message;
        }

        yield return StartCoroutine(FadeIn());

        yield return new WaitForSecondsRealtime(displayDuration + extraDelay);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            if (loadingProgressBar != null)
            {
                loadingProgressBar.fillAmount = asyncLoad.progress;
            }
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        yield return null;

        yield return StartCoroutine(FadeOut());

        isTransitioning = false;
    }

    IEnumerator FadeIn()
    {
        if (loadingScreenPanel != null)
        {
            loadingScreenPanel.SetActive(true);
        }

        if (fadeImage != null)
        {
            float elapsedTime = 0f;
            Color color = fadeImage.color;

            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                fadeImage.color = color;
                yield return null;
            }

            color.a = 1f;
            fadeImage.color = color;
        }
    }

    IEnumerator FadeOut()
    {
        if (fadeImage != null)
        {
            float elapsedTime = 0f;
            Color color = fadeImage.color;

            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
                fadeImage.color = color;
                yield return null;
            }

            color.a = 0f;
            fadeImage.color = color;
        }

        if (loadingScreenPanel != null)
        {
            loadingScreenPanel.SetActive(false);
        }
    }

    string GetLoadingMessage(LoadingType type)
    {
        string[] messages = type switch
        {
            LoadingType.Victory => victoryMessages,
            LoadingType.GameComplete => gameCompleteMessages,
            _ => loadingMessages
        };

        if (messages.Length > 0)
        {
            return messages[Random.Range(0, messages.Length)];
        }

        return "Loading...";
    }

    public bool IsTransitioning => isTransitioning;
}

public enum LoadingType
{
    Normal,
    Victory,
    GameComplete
}