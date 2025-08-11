using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Static utility class managing rule selection persistence between scenes.
/// </summary>
public static class RulePersistenceManager
{
    private static RuleSelectionData currentSelection = new RuleSelectionData();
    private const string PREFS_KEY = "SelectedRules";

    public static void SaveRuleSelection(List<Rule> rules)
    {
        currentSelection = new RuleSelectionData(rules);

        string json = JsonUtility.ToJson(currentSelection);
        PlayerPrefs.SetString(PREFS_KEY, json);
        PlayerPrefs.Save();

        Debug.Log($"Saved {rules.Count} rules to persistence manager");
        foreach (Rule rule in rules)
        {
            Debug.Log($"- {rule.ruleName}");
        }
    }

    public static List<string> GetSelectedRuleNames()
    {
        return new List<string>(currentSelection.selectedRuleNames);
    }

    public static List<string> LoadRuleSelectionFromPrefs()
    {
        if (PlayerPrefs.HasKey(PREFS_KEY))
        {
            string json = PlayerPrefs.GetString(PREFS_KEY);
            RuleSelectionData data = JsonUtility.FromJson<RuleSelectionData>(json);
            return data.selectedRuleNames;
        }
        return new List<string>();
    }

    public static void ClearSelection()
    {
        currentSelection = new RuleSelectionData();
        PlayerPrefs.DeleteKey(PREFS_KEY);
        Debug.Log("Cleared rule selection");
    }

    public static bool HasSelection()
    {
        return currentSelection.selectedRuleNames.Count > 0;
    }
}