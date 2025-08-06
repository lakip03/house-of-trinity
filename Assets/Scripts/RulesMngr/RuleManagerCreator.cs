using UnityEngine;

public class RuleManagerBootstrap : MonoBehaviour
{
    [Header("RuleManager Settings")]
    public GameObject ruleManagerPrefab;
    
    void Awake()
    {
        if (RuleManager.Instance == null)
        {
            RuleManager existingManager = FindAnyObjectByType<RuleManager>();
            
            if (existingManager == null)
            {
                if (ruleManagerPrefab != null)
                {
                    GameObject managerObj = Instantiate(ruleManagerPrefab);
                    managerObj.name = "RuleManager";
                    Debug.Log("RuleManager created by Bootstrap");
                }
                else
                {
                    GameObject managerObj = new GameObject("RuleManager");
                    managerObj.AddComponent<RuleManager>();
                    Debug.Log("Basic RuleManager created by Bootstrap");
                }
            }
        }
        
        Destroy(gameObject);
    }
}