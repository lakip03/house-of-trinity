using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class RuleManager : MonoBehaviour
{
    [Header("Rule Configuration")]
    public List<Rule> availableRules = new List<Rule>();
    public List<Rule> activeRules = new List<Rule>();
    
    [Header("Rule References")]
    public PlayerController playerController;
    
    [Header("Debug Settings")]
    public bool debugQueueSystem = true;
    
    public static RuleManager Instance { get; private set; }
    public Action OnRulesChanged;
    public Action OnPlayerReady;
    
    private Queue<QueuedRuleAction> ruleActionQueue = new Queue<QueuedRuleAction>();
    private bool isPlayerReady = false;
    private bool hasProcessedPersistentRules = false;
    
    public bool IsPlayerReady => isPlayerReady;
    public int QueuedActionsCount => ruleActionQueue.Count;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeRules();
            LogQueue("RuleManager initialized, waiting for PlayerController");
            
            // Subscribe to scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        TryFindAndRegisterPlayer();
        ProcessPersistentRules();
    }
    
    void Update()
    {
        if (!isPlayerReady)
        {
            TryFindAndRegisterPlayer();
        }
    }
    
    // Called when a new scene is loaded
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LogQueue($"Scene loaded: {scene.name}");
        
        // Reset player ready state when entering a new scene
        if (IsLevelScene(scene.name))
        {
            LogQueue("Level scene detected, resetting player state and searching for PlayerController");
            ResetPlayerState();
            
            // Give the scene a frame to initialize, then search for player
            StartCoroutine(DelayedPlayerSearch());
        }
    }
    
    // Check if this is a level scene (not menu or card selector)
    bool IsLevelScene(string sceneName)
    {
        // Check if it's a level scene (customize this based on your scene naming)
        return sceneName.Contains("Level") && 
               !sceneName.Contains("Menu") && 
               !sceneName.Contains("Card") &&
               !sceneName.Contains("End");
    }
    
    System.Collections.IEnumerator DelayedPlayerSearch()
    {
        // Wait one frame for scene objects to initialize
        yield return null;
        
        LogQueue("Performing delayed player search...");
        TryFindAndRegisterPlayer();
        
        // If still not found, keep trying for a few seconds
        float timeout = 3f;
        float elapsed = 0f;
        
        while (!isPlayerReady && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
            TryFindAndRegisterPlayer();
        }
        
        if (!isPlayerReady)
        {
            Debug.LogError("Failed to find PlayerController after timeout!");
        }
    }
    
    void ResetPlayerState()
    {
        isPlayerReady = false;
        playerController = null;
        LogQueue("Player state reset for new scene");
    }
    
    private void InitializeRules()
    {
        Rule[] allRules = Resources.LoadAll<Rule>("Rules");
        availableRules.AddRange(allRules);
        LogQueue($"Loaded {availableRules.Count} rules");
    }
    
    private void TryFindAndRegisterPlayer()
    {
        if (isPlayerReady) return;
        
        if (playerController == null)
        {
            // Try multiple methods to find the player
            playerController = FindAnyObjectByType<PlayerController>();
            
            if (playerController == null)
            {
                // Try finding by tag
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                {
                    playerController = playerObj.GetComponent<PlayerController>();
                }
            }
            
            if (playerController == null)
            {
                // Try finding by name
                GameObject playerObj = GameObject.Find("Player");
                if (playerObj != null)
                {
                    playerController = playerObj.GetComponent<PlayerController>();
                }
            }
        }
        
        if (playerController != null && !isPlayerReady)
        {
            RegisterPlayerController(playerController);
        }
    }
    
    public void RegisterPlayerController(PlayerController player)
    {
        if (player == null)
        {
            Debug.LogWarning("Attempted to register null PlayerController");
            return;
        }
        
        playerController = player;
        isPlayerReady = true;
        
        LogQueue($"PlayerController registered! Processing {ruleActionQueue.Count} queued actions");
        
        // Re-activate all currently active rules for the new player
        ReactivateRulesForNewPlayer();
        
        ProcessQueue();
        ProcessPersistentRules();
        OnPlayerReady?.Invoke();
        
        LogQueue("Rule management system active with player");
    }
    
    // Reactivate rules when a new player is found (for scene transitions)
    void ReactivateRulesForNewPlayer()
    {
        if (activeRules.Count > 0 && playerController != null)
        {
            LogQueue($"Reactivating {activeRules.Count} active rules for new player");
            
            foreach (Rule rule in activeRules.ToList())
            {
                if (rule != null)
                {
                    rule.ActivateRule(playerController);
                    LogQueue($"Reactivated rule: {rule.ruleName}");
                }
            }
        }
    }
    
    private void ProcessQueue()
    {
        LogQueue($"Processing {ruleActionQueue.Count} queued actions");
        
        while (ruleActionQueue.Count > 0)
        {
            QueuedRuleAction action = ruleActionQueue.Dequeue();
            ExecuteRuleAction(action);
        }
        
        LogQueue("Queue processing complete");
    }
    
    private void ExecuteRuleAction(QueuedRuleAction action)
    {
        switch (action.ActionType)
        {
            case RuleActionType.Add:
                ExecuteAddRule(action.Rule);
                break;
            case RuleActionType.Remove:
                ExecuteRemoveRule(action.Rule);
                break;
            case RuleActionType.Replace:
                ExecuteReplaceRule(action.Index, action.Rule);
                break;
            case RuleActionType.Clear:
                ExecuteClearRules();
                break;
        }
    }
    
    private void ProcessPersistentRules()
    {
        if (hasProcessedPersistentRules) return;
        
        if (RulePersistenceManager.HasSelection())
        {
            List<string> selectedRuleNames = RulePersistenceManager.GetSelectedRuleNames();
            LogQueue($"Applying {selectedRuleNames.Count} persistent rules");
            
            ClearAllRules();
            
            foreach (string ruleName in selectedRuleNames)
            {
                Rule rule = availableRules.FirstOrDefault(r => r.ruleName == ruleName);
                if (rule != null)
                {
                    AddRule(rule);
                    LogQueue($"Applied persistent rule: {ruleName}");
                }
                else
                {
                    Debug.LogWarning($"Persistent rule not found: {ruleName}");
                }
            }
            
            hasProcessedPersistentRules = true;
            RulePersistenceManager.ClearSelection();
            LogQueue("Persistent rules applied");
        }
    }
    
    public bool AddRule(Rule rule)
    {
        if (!isPlayerReady)
        {
            ruleActionQueue.Enqueue(new QueuedRuleAction(RuleActionType.Add, rule));
            LogQueue($"Queued add rule: {rule.ruleName} (Queue: {ruleActionQueue.Count})");
            return true;
        }
        
        return ExecuteAddRule(rule);
    }
    
    public bool RemoveRule(Rule rule)
    {
        if (!isPlayerReady)
        {
            ruleActionQueue.Enqueue(new QueuedRuleAction(RuleActionType.Remove, rule));
            LogQueue($"Queued remove rule: {rule.ruleName} (Queue: {ruleActionQueue.Count})");
            return true;
        }
        
        return ExecuteRemoveRule(rule);
    }
    
    public bool ReplaceRule(int index, Rule newRule)
    {
        if (!isPlayerReady)
        {
            ruleActionQueue.Enqueue(new QueuedRuleAction(RuleActionType.Replace, newRule, index));
            LogQueue($"Queued replace rule at {index}: {newRule.ruleName} (Queue: {ruleActionQueue.Count})");
            return true;
        }
        
        return ExecuteReplaceRule(index, newRule);
    }
    
    public void ClearAllRules()
    {
        if (!isPlayerReady)
        {
            // Clear the active rules list immediately even if player isn't ready
            activeRules.Clear();
            
            // Also queue a clear action for when player becomes ready
            ruleActionQueue.Enqueue(new QueuedRuleAction(RuleActionType.Clear));
            LogQueue($"Cleared active rules and queued clear action (Queue: {ruleActionQueue.Count})");
            return;
        }
        
        ExecuteClearRules();
    }
        
    private bool ExecuteAddRule(Rule rule)
    {
        if (activeRules.Count >= 3)
        {
            Debug.LogError("Maximum of 3 rules allowed");
            return false;
        }
        
        if (activeRules.Contains(rule))
        {
            Debug.LogError("Rule already active - duplicates not allowed");
            return false;
        }
        
        activeRules.Add(rule);
        rule.OnRuleSelected();
        
        if (playerController != null)
        {
            rule.ActivateRule(playerController);
        }
        else
        {
            Debug.LogWarning($"Added rule {rule.ruleName} but no player controller available yet");
        }
        
        rule.isActive = true;
        
        OnRulesChanged?.Invoke();
        return true;
    }
    
    private bool ExecuteRemoveRule(Rule rule)
    {
        if (!activeRules.Contains(rule))
        {
            Debug.LogWarning("Rule not in active set");
            return false;
        }
        
        if (playerController != null)
        {
            rule.DeactivateRule(playerController);
        }
        
        rule.OnRuleRemoved();
        activeRules.Remove(rule);
        rule.isActive = false;
        
        OnRulesChanged?.Invoke();
        return true;
    }
    
    private bool ExecuteReplaceRule(int index, Rule newRule)
    {
        if (index < 0 || index >= activeRules.Count)
        {
            Debug.LogWarning("Invalid rule index for replacement");
            return false;
        }
        
        Rule oldRule = activeRules[index];
        
        if (playerController != null)
        {
            oldRule.DeactivateRule(playerController);
        }
        
        oldRule.OnRuleRemoved();
        oldRule.isActive = false;
        
        activeRules[index] = newRule;
        newRule.OnRuleSelected();
        
        if (playerController != null)
        {
            newRule.ActivateRule(playerController);
        }
        
        newRule.isActive = true;
        
        OnRulesChanged?.Invoke();
        return true;
    }
    
    private void ExecuteClearRules()
    {
        foreach (Rule rule in activeRules.ToList())
        {
            if (playerController != null)
            {
                rule.DeactivateRule(playerController);
            }
            rule.OnRuleRemoved();
            rule.isActive = false;
        }
        
        activeRules.Clear();
        OnRulesChanged?.Invoke();
        LogQueue("Cleared all active rules");
    }
    
    private void UpdateActiveRules()
    {
        if (!isPlayerReady || playerController == null) return;
        
        foreach (Rule rule in activeRules)
        {
            if (rule != null && rule.isActive)
            {
                rule.UpdateRule(playerController, Time.deltaTime);
            }
        }
    }
    
    void LateUpdate()
    {
        UpdateActiveRules();
    }
        
    public List<Rule> GetActiveRulesByType(RuleType type)
    {
        return activeRules.Where(r => r.ruleType == type).ToList();
    }
    
    public bool IsRuleActive(string ruleName)
    {
        return activeRules.Any(r => r.ruleName == ruleName);
    }
    
    public List<Rule> GetAvailableRules()
    {
        return availableRules.Where(r => !activeRules.Contains(r)).ToList();
    }
    
    public void ForcePlayerSearch()
    {
        isPlayerReady = false;
        playerController = null;
        TryFindAndRegisterPlayer();
    }
    
    private void LogQueue(string message)
    {
        if (debugQueueSystem)
        {
            Debug.Log($"[RuleManager] {message}");
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}