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

    private void InitializeRules()
    {
        throw new NotImplementedException();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
    
}