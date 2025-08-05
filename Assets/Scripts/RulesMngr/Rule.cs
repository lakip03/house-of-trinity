using UnityEngine;
using System;

[System.Serializable]
public abstract class Rule : ScriptableObject
{
    [Header("Rule Info")]
    public string ruleName;
    public string ruleDescription;
    public RuleType ruleType;
    public Sprite ruleCard;

    [Header("Rule Settings")]
    public bool isActive = false; //Probably bad solution; can't think of a better one right now probably will change latter
    public float duration = -1f; // -1 means it is allwas active ; >0 means that it is timed

    //Events
    public static event Action<Rule> OnRuleActivated;
    public static event Action<Rule> OnRuleDeactivated;

    //Rule Methods
    public abstract void ActivateRule(PlayerController player);
    public abstract void DeactivateRule(PlayerController player);
    public abstract void UpdateRule(PlayerController player, float deltaTime);

    // State updates
    public virtual void OnRuleSelected()
    {
        isActive = true;
        OnRuleActivated?.Invoke(this);
        Debug.Log($"Rule activated - {ruleName}");
    }
    public virtual void OnRuleRemoved()
    {
        isActive = false;
        OnRuleDeactivated?.Invoke(this);
        Debug.Log($"Rule deactivated - {ruleName}");
    }
}
