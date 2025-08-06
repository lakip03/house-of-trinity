using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

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
            playerController = FindAnyObjectByType<PlayerController>();
        }
        
        if (playerController != null && !isPlayerReady)
        {
            RegisterPlayerController(playerController);
        }
    }
    
    public void RegisterPlayerController(PlayerController player)
    {
        if (isPlayerReady)
        {
            LogQueue("PlayerController already registered");
            return;
        }
        
        playerController = player;
        isPlayerReady = true;
        
        LogQueue($"PlayerController registered! Processing {ruleActionQueue.Count} queued actions");
        
        ProcessQueue();
        ProcessPersistentRules();
        OnPlayerReady?.Invoke();
        
        LogQueue("Rule management system active");
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
            ruleActionQueue.Enqueue(new QueuedRuleAction(RuleActionType.Clear));
            LogQueue($"Queued clear all rules (Queue: {ruleActionQueue.Count})");
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
        rule.ActivateRule(playerController);
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
        
        rule.DeactivateRule(playerController);
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
        oldRule.DeactivateRule(playerController);
        oldRule.OnRuleRemoved();
        oldRule.isActive = false;
        
        activeRules[index] = newRule;
        newRule.OnRuleSelected();
        newRule.ActivateRule(playerController);
        newRule.isActive = true;
        
        OnRulesChanged?.Invoke();
        return true;
    }
    
    private void ExecuteClearRules()
    {
        foreach (Rule rule in activeRules.ToList())
        {
            rule.DeactivateRule(playerController);
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
}