using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeBendUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject timerPanel;
    public TextMeshProUGUI timerText;
    public Image timerFillBar;
    public Animation panelAnimation;
    
    [Header("Display Settings")]
    public string timerFormat = "Time Left: {0}";
    public Color normalColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;
    public float warningThreshold = 15f; // Warning when less than 15 seconds
    public float criticalThreshold = 5f; // Critical when less than 5 seconds
    
    [Header("Effects")]
    public bool enablePulseEffect = true;
    public float pulseSpeed = 2f;
    
    private float totalTime = 0f;
    private bool isDisplaying = false;
    private bool isInCriticalTime = false;
    
    void OnEnable()
    {
        TimeBend.OnTimeBendStarted += ShowTimerDisplay;
        TimeBend.OnTimeBendEnded += HideTimerDisplay;
        TimeBend.OnTimeRemainingUpdated += UpdateTimerDisplay;
    }
    
    void OnDisable()
    {
        TimeBend.OnTimeBendStarted -= ShowTimerDisplay;
        TimeBend.OnTimeBendEnded -= HideTimerDisplay;
        TimeBend.OnTimeRemainingUpdated -= UpdateTimerDisplay;
    }
    
    void Start()
    {
        if (timerPanel != null)
        {
            timerPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        if (isDisplaying && isInCriticalTime && enablePulseEffect)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
            float scale = Mathf.Lerp(0.9f, 1.1f, pulse);
            
            if (timerText != null)
            {
                timerText.transform.localScale = Vector3.one * scale;
            }
        }
    }
    
    private void ShowTimerDisplay()
    {
        if (timerPanel == null) return;
        
        TimeBend activeTimeBend = GetActiveTimeBendRule();
        if (activeTimeBend != null)
        {
            totalTime = activeTimeBend.TimeLimit;
        }
        
        isDisplaying = true;
        timerPanel.SetActive(true);
        
        if (panelAnimation != null)
        {
            panelAnimation.Play("TimerShow");
        }
        
        Debug.Log("TimeBend timer UI activated");
    }
    
    private void HideTimerDisplay()
    {
        if (timerPanel == null) return;
        
        isDisplaying = false;
        isInCriticalTime = false;
        
        if (timerText != null)
        {
            timerText.transform.localScale = Vector3.one;
        }
        
        if (panelAnimation != null)
        {
            panelAnimation.Play("TimerHide");
            Invoke(nameof(HidePanel), 0.5f);
        }
        else
        {
            HidePanel();
        }
        
        Debug.Log("TimeBend timer UI deactivated");
    }
    
    private void HidePanel()
    {
        if (timerPanel != null)
        {
            timerPanel.SetActive(false);
        }
    }
    
    private void UpdateTimerDisplay(float timeRemaining, float totalTime)
    {
        if (!isDisplaying) return;
        
        this.totalTime = totalTime; 
        
        string formattedTime = FormatTime(timeRemaining);
        
        if (timerText != null)
        {
            timerText.text = string.Format(timerFormat, formattedTime);
            
            Color textColor = GetColorForTime(timeRemaining);
            timerText.color = textColor;
        }
        
        if (timerFillBar != null && totalTime > 0)
        {
            float fillAmount = timeRemaining / totalTime;
            timerFillBar.fillAmount = fillAmount;
            
            Color barColor = GetColorForTime(timeRemaining);
            timerFillBar.color = barColor;
        }
        
        isInCriticalTime = timeRemaining <= criticalThreshold;
    }
    
    private Color GetColorForTime(float timeRemaining)
    {
        if (timeRemaining <= criticalThreshold)
        {
            return criticalColor;
        }
        else if (timeRemaining <= warningThreshold)
        {
            return warningColor;
        }
        else
        {
            return normalColor;
        }
    }
    
    private string FormatTime(float timeInSeconds)
    {
        if (timeInSeconds <= 0f) return "00:00";
        
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    
    private TimeBend GetActiveTimeBendRule()
    {
        if (RuleManager.Instance == null) return null;
        
        foreach (Rule rule in RuleManager.Instance.activeRules)
        {
            if (rule is TimeBend timeBend)
            {
                return timeBend;
            }
        }
        
        return null;
    }
    
    [ContextMenu("Test Timer Display")]
    private void TestTimerDisplay()
    {
        TimeBend activeTimeBend = GetActiveTimeBendRule();
        if (activeTimeBend != null)
        {
            Debug.Log($"TimeBend active - Time remaining: {activeTimeBend.GetFormattedTimeRemaining()}");
        }
        else
        {
            Debug.Log("No active TimeBend rule found");
        }
    }
}