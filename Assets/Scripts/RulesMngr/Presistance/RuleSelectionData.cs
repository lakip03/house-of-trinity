using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RuleSelectionData
{
    public List<string> selectedRuleNames = new List<string>();
    
    public RuleSelectionData()
    {
        selectedRuleNames = new List<string>();
    }
    
    public RuleSelectionData(List<Rule> rules)
    {
        selectedRuleNames = new List<string>();
        foreach (Rule rule in rules)
        {
            if (rule != null)
                selectedRuleNames.Add(rule.ruleName);
        }
    }
}
