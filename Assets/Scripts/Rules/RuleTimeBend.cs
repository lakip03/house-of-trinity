using UnityEngine;

/// <summary>
/// Restriction rule implementing a countdown timer that triggers game over when expired.
/// Uses events to communicate with UI systems for timer display and updates.
/// </summary>
[CreateAssetMenu(fileName = "TimeBend", menuName = "Rules/Restrictions/Time Bend")]
public class TimeBend : Rule
{
    [Header("Time Limit Settings")]
    public float timeLimit = 40f;
    
    private float timeRemaining;
    private bool isTimerActive = false;
    
    public static event System.Action<float, float> OnTimeRemainingUpdated; // (timeRemaining, totalTime)
    public static event System.Action OnTimeBendStarted;
    public static event System.Action OnTimeBendEnded;
    
    public float TimeRemaining => timeRemaining;
    public float TimeLimit => timeLimit;
    public bool IsTimerActive => isTimerActive;

    public override void ActivateRule(PlayerController player)
    {

        if (player == null)
        {
            Debug.LogWarning($"[{ruleName}] ActivateRule called with null player - rule will be activated when player is available");
            return;
        }
        

        timeRemaining = timeLimit;
        isTimerActive = true;
        
        OnTimeBendStarted?.Invoke();
        
        Debug.Log($"Time Bend restriction activated! Player has {timeLimit} seconds to complete the level!");
    }
    
    public override void DeactivateRule(PlayerController player)
    {
         if (player == null)
        {
            Debug.LogWarning($"[{ruleName}] DeactivateRule called with null player - skipping deactivation");
            return;
        }
        isTimerActive = false;
        timeRemaining = 0f;
        
        OnTimeBendEnded?.Invoke();
        
        Debug.Log("Time Bend restriction deactivated.");
    }
    
    public override void UpdateRule(PlayerController player, float deltaTime)
    {
          if (player == null) return;
        if (!isTimerActive) return;
        
        timeRemaining -= deltaTime;
        
        OnTimeRemainingUpdated?.Invoke(timeRemaining, timeLimit);
        
        if (timeRemaining <= 0f)
        {
            Debug.Log("GAME OVER - Time Bend! Time limit exceeded!");
            TriggerGameOver(player);
        }
    }
    
    private void TriggerGameOver(PlayerController player)
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.GameOver("Time Bend - Time limit exceeded! You ran out of time!");
        }
        else
        {
            Debug.LogError("GameStateManager not found! Make sure it's in the scene.");
        }
        
        OnRuleRemoved();
    }
    
    public string GetFormattedTimeRemaining()
    {
        if (timeRemaining <= 0f) return "00:00";
        
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}