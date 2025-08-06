[System.Serializable]
public class QueuedRuleAction
{
    public RuleActionType ActionType;
    public Rule Rule;
    public int Index;
    
    public QueuedRuleAction(RuleActionType actionType, Rule rule = null, int index = -1)
    {
        ActionType = actionType;
        Rule = rule;
        Index = index;
    }
}