using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class RuleManager : MonoBehaviour
{
    [Header("Rule Configuration")]
    public List<Rule> availableRules = new List<Rule>();
    public List<Rule> activeRules = new List<Rule>();

    [Header("Rule Referances")]
    public PlayerController playerController;

    public static RuleManager Instance { get; private set; } // We will be using singelton pattern for Rules Manager as we want to prevent any future instance of this object exsisting

    public System.Action OnRulesChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeRules();
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void Start()
    {
        //AddRule(availableRules.Find(r => r.ruleName == "Sprint"));
        //AddRule(availableRules.Find(r => r.ruleName == "Quantum Tunelling"));
        //AddRule(availableRules.Find(r => r.ruleName == "Mystical Sheild"));
        //AddRule(availableRules.Find(r => r.ruleName == "Cold Feet"));
    }

    void Update()
    {
        UpdateActiveRules();
    }

    private void InitializeRules()
    {
        Rule[] allRules = Resources.LoadAll<Rule>("Rules");
        availableRules.AddRange(allRules);

        Debug.Log($"Loaded {availableRules.Count} rules :)");
    }

    private void UpdateActiveRules()
    {
        foreach (Rule rule in activeRules)
        {
            if (rule != null && rule.isActive)
            {
                rule.UpdateRule(playerController, Time.deltaTime);
            }
        }
    }

    public bool AddRule(Rule rule)
    {
        if (activeRules.Count > 3)
        {
            Debug.LogError("Too many rules maxium is 3!");
            return false;
        }
        if (activeRules.Contains(rule))
        {
            Debug.LogError("Cannot duplicate rules");
            return false;
        }

        activeRules.Add(rule);
        rule.OnRuleSelected();
        rule.ActivateRule(playerController);
        rule.isActive = true;

        OnRulesChanged?.Invoke();
        return true;
    }
    // Remove rule from active set
    public bool RemoveRule(Rule rule)
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
    // Replace rule at specific index
    public bool ReplaceRule(int index, Rule newRule)
    {
        if (index < 0 || index >= activeRules.Count)
        {
            Debug.LogWarning("Invalid rule index");
            return false;
        }

        Rule oldRule = activeRules[index];
        oldRule.DeactivateRule(playerController);
        oldRule.OnRuleRemoved();

        activeRules[index] = newRule;
        newRule.OnRuleSelected();
        newRule.ActivateRule(playerController);

        OnRulesChanged?.Invoke();
        return true;
    }
    // Get active rules of specific type
    public List<Rule> GetActiveRulesByType(RuleType type)
    {
        return activeRules.Where(r => r.ruleType == type).ToList();
    }
    
    // Check if specific rule is active
    public bool IsRuleActive(string ruleName)
    {
        return activeRules.Any(r => r.ruleName == ruleName);
    }
    
    // Clear all active rules
    public void ClearAllRules()
    {
        foreach (Rule rule in activeRules.ToList())
        {
            RemoveRule(rule);
        }
    }
    
    // Get available rules for selection
    public List<Rule> GetAvailableRules()
    {
        return availableRules.Where(r => !activeRules.Contains(r)).ToList();
    }
}