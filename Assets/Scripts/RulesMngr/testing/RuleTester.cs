using UnityEditor;
using UnityEngine;

class RuleTester : MonoBehaviour
{
    public RuleManager ruleManager;
    public Rule rule;

    void Start()
    {
        ruleManager.AddRule(ruleManager.availableRules.Find(r => r == rule));
    }
}