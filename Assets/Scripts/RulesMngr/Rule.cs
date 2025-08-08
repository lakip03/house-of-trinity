using UnityEngine;
using System;

/// <summary>
/// Abstract base class for all gameplay rules.
/// Rules are implemented as modular ScriptableObjects and managed via the RuleManager.
/// </summary>
[System.Serializable]
public abstract class Rule : ScriptableObject
{
    [Header("Rule Info")]
    [Tooltip("Display name of the rule.")]
    public string ruleName;

    [Tooltip("Text description of what the rule does.")]
    public string ruleDescription;

    [Tooltip("The type/category of this rule.")]
    public RuleType ruleType;

    [Tooltip("Visual card/sprite used for UI representation.")]
    public Sprite ruleCard;

    [Header("Rule Settings")]
    [Tooltip("Indicates whether the rule is currently active. Used by RuleManager.")]
    public bool isActive = false;  // TODO: Consider removing this state and moving to manager-controlled flags.

    [Tooltip("Duration this rule stays active. -1 means permanent.")]
    public float duration = -1f;

    /// <summary>
    /// Invoked globally when any rule is activated.
    /// </summary>
    public static event Action<Rule> OnRuleActivated;

    /// <summary>
    /// Invoked globally when any rule is deactivated.
    /// </summary>
    public static event Action<Rule> OnRuleDeactivated;

    /// <summary>
    /// Called once when the rule is activated by the RuleManager.
    /// </summary>
    /// <param name="player">The player affected by the rule.</param>
    public abstract void ActivateRule(PlayerController player);

    /// <summary>
    /// Called once when the rule is removed or expired.
    /// </summary>
    /// <param name="player">The player affected by the rule.</param>
    public abstract void DeactivateRule(PlayerController player);

    /// <summary>
    /// Called every frame while the rule is active.
    /// </summary>
    /// <param name="player">The player affected by the rule.</param>
    /// <param name="deltaTime">Time since last update.</param>
    public abstract void UpdateRule(PlayerController player, float deltaTime);

    /// <summary>
    /// Called when the rule is manually selected or activated in the UI.
    /// Triggers global activation event.
    /// </summary>
    public virtual void OnRuleSelected()
    {
        isActive = true;
        OnRuleActivated?.Invoke(this);
        Debug.Log($"Rule activated - {ruleName}");
    }

    /// <summary>
    /// Called when the rule is removed or deselected.
    /// Triggers global deactivation event.
    /// </summary>
    public virtual void OnRuleRemoved()
    {
        isActive = false;
        OnRuleDeactivated?.Invoke(this);
        Debug.Log($"Rule deactivated - {ruleName}");
    }
}
