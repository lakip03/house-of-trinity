// Fixed CardSelectionManager.cs - Handles null RuleManager gracefully
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class CardSelectionManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform cardContainer;
    public GameObject cardPrefab;
    public Button nextButton;
    public Button backButton;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI selectionCountText;
    
    [Header("Fallback Rules (if RuleManager missing)")]
    public List<Rule> fallbackRules = new List<Rule>();
    
    [Header("Selected Rules Display")]
    public Transform selectedRulesContainer;
    public GameObject selectedRuleDisplayPrefab;
    
    [Header("Selection Settings")]
    public int totalPositiveRulesNeeded = 2;
    public int totalRestrictionRulesNeeded = 1;
    
    [Header("Scene Settings")]
    public string gameSceneName = "GameLevel";
    public string menuSceneName = "MainMenu";
    
    [Header("Audio")]
    public AudioClip cardSelectSound;
    public AudioClip cardDeselectSound;
    public AudioClip nextSound;
    public AudioClip errorSound;
    
    private List<SelectableCard> allCards = new List<SelectableCard>();
    private List<Rule> selectedPositiveRules = new List<Rule>();
    private List<Rule> selectedRestrictionRules = new List<Rule>();
    private List<GameObject> selectedRuleDisplays = new List<GameObject>();
    
    private SelectionPhase currentPhase = SelectionPhase.SelectingPositive;
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        EnsureRuleManagerExists();
        
        SetupUI();
        GenerateCards();
        UpdateUI();
    }
    
    void EnsureRuleManagerExists()
    {
        if (RuleManager.Instance == null)
        {
            Debug.LogWarning("RuleManager.Instance is null! Trying to find or create one...");
            
            RuleManager existingManager = FindFirstObjectByType<RuleManager>();
            
            if (existingManager == null)
            {
                GameObject managerObj = new GameObject("RuleManager");
                RuleManager newManager = managerObj.AddComponent<RuleManager>();
                
                if (fallbackRules.Count > 0)
                {
                    newManager.availableRules.AddRange(fallbackRules);
                    Debug.Log($"Created RuleManager with {fallbackRules.Count} fallback rules");
                }
                else
                {
                    Debug.LogError("No fallback rules assigned! Please assign rules in CardSelectionManager or ensure RuleManager exists in scene.");
                }
            }
        }
    }
    
    void SetupUI()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
            nextButton.interactable = false;
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBack);
        }
    }
    
    void GenerateCards()
    {
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
        allCards.Clear();
        
        List<Rule> availableRules = GetAvailableRules();
        
        if (availableRules.Count == 0)
        {
            Debug.LogError("No available rules found! Check RuleManager setup or fallback rules.");
            return;
        }
        
        foreach (Rule rule in availableRules)
        {
            if (rule == null)
            {
                Debug.LogWarning("Null rule found in available rules list, skipping...");
                continue;
            }
            
            GameObject cardObj = Instantiate(cardPrefab, cardContainer);
            SelectableCard card = cardObj.GetComponent<SelectableCard>();
            
            if (card != null)
            {
                card.Initialize(rule, this);
                allCards.Add(card);
            }
            else
            {
                Debug.LogError("SelectableCard component not found on card prefab!");
            }
        }
        
        Debug.Log($"Generated {allCards.Count} cards");
    }
    
    List<Rule> GetAvailableRules()
    {
        if (RuleManager.Instance != null && RuleManager.Instance.availableRules.Count > 0)
        {
            return RuleManager.Instance.availableRules.Where(r => r != null).ToList();
        }
        
        Debug.LogWarning("Using fallback rules as RuleManager is not available or has no rules");
        return fallbackRules.Where(r => r != null).ToList();
    }
    
    public void OnCardClicked(SelectableCard card)
    {
        switch (currentPhase)
        {
            case SelectionPhase.SelectingPositive:
                HandlePositiveRuleSelection(card);
                break;
            case SelectionPhase.SelectingRestriction:
                HandleRestrictionRuleSelection(card);
                break;
        }
        
        UpdateUI();
        UpdateSelectedRuleDisplays();
    }
    
    void HandlePositiveRuleSelection(SelectableCard card)
    {
        if (card.AssociatedRule.ruleType == RuleType.Restriction)
        {
            PlayErrorSound();
            return;
        }
        
        if (card.IsSelected)
        {
            card.SetSelected(false);
            selectedPositiveRules.Remove(card.AssociatedRule);
            PlayDeselectSound();
        }
        else
        {
            if (selectedPositiveRules.Count < totalPositiveRulesNeeded)
            {
                card.SetSelected(true);
                selectedPositiveRules.Add(card.AssociatedRule);
                PlaySelectSound();
            }
            else
            {
                PlayErrorSound();
            }
        }
    }
    
    void HandleRestrictionRuleSelection(SelectableCard card)
    {
        if (card.AssociatedRule.ruleType != RuleType.Restriction)
        {
            PlayErrorSound();
            return;
        }
        
        if (card.IsSelected)
        {
            card.SetSelected(false);
            selectedRestrictionRules.Remove(card.AssociatedRule);
            PlayDeselectSound();
        }
        else
        {
            if (selectedRestrictionRules.Count < totalRestrictionRulesNeeded)
            {
                card.SetSelected(true);
                selectedRestrictionRules.Add(card.AssociatedRule);
                PlaySelectSound();
            }
            else
            {
                PlayErrorSound();
            }
        }
    }
    
    void OnNextButtonClicked()
    {
        switch (currentPhase)
        {
            case SelectionPhase.SelectingPositive:
                if (selectedPositiveRules.Count == totalPositiveRulesNeeded)
                {
                    currentPhase = SelectionPhase.SelectingRestriction;
                    ClearAllSelections();
                    PlayNextSound();
                }
                break;
                
            case SelectionPhase.SelectingRestriction:
                if (selectedRestrictionRules.Count == totalRestrictionRulesNeeded)
                {
                    CompleteSelection();
                }
                break;
        }
        
        UpdateUI();
    }
    
    void ClearAllSelections()
    {
        foreach (SelectableCard card in allCards)
        {
            card.SetSelected(false);
        }
    }
    
    void UpdateUI()
    {
        UpdateInstructions();
        UpdateSelectionCount();
        UpdateCardInteractability();
        UpdateNextButton();
    }
    
    void UpdateInstructions()
    {
        if (instructionText != null)
        {
            switch (currentPhase)
            {
                case SelectionPhase.SelectingPositive:
                    instructionText.text = $"Select {totalPositiveRulesNeeded} Positive Rules\n(Movement, Health, or Temporal)";
                    break;
                case SelectionPhase.SelectingRestriction:
                    instructionText.text = $"Select {totalRestrictionRulesNeeded} Restriction Rule";
                    break;
            }
        }
    }
    
    void UpdateSelectionCount()
    {
        if (selectionCountText != null)
        {
            switch (currentPhase)
            {
                case SelectionPhase.SelectingPositive:
                    selectionCountText.text = $"Selected: {selectedPositiveRules.Count}/{totalPositiveRulesNeeded}";
                    break;
                case SelectionPhase.SelectingRestriction:
                    selectionCountText.text = $"Selected: {selectedRestrictionRules.Count}/{totalRestrictionRulesNeeded}";
                    break;
            }
        }
    }
    
    void UpdateCardInteractability()
    {
        foreach (SelectableCard card in allCards)
        {
            bool canInteract = false;
            
            switch (currentPhase)
            {
                case SelectionPhase.SelectingPositive:
                    canInteract = card.AssociatedRule.ruleType != RuleType.Restriction;
                    break;
                case SelectionPhase.SelectingRestriction:
                    canInteract = card.AssociatedRule.ruleType == RuleType.Restriction;
                    break;
            }
            
            card.SetInteractable(canInteract);
        }
    }
    
    void UpdateNextButton()
    {
        if (nextButton != null)
        {
            bool canProceed = false;
            string buttonText = "Next";
            
            switch (currentPhase)
            {
                case SelectionPhase.SelectingPositive:
                    canProceed = selectedPositiveRules.Count == totalPositiveRulesNeeded;
                    buttonText = "Next";
                    break;
                case SelectionPhase.SelectingRestriction:
                    canProceed = selectedRestrictionRules.Count == totalRestrictionRulesNeeded;
                    buttonText = "Start Game";
                    break;
            }
            
            nextButton.interactable = canProceed;
            
            TextMeshProUGUI buttonTextComponent = nextButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonTextComponent != null)
            {
                buttonTextComponent.text = buttonText;
            }
        }
    }
    
    void UpdateSelectedRuleDisplays()
    {
        if (selectedRulesContainer == null)
            return;
            
        foreach (GameObject display in selectedRuleDisplays)
        {
            if (display != null)
                Destroy(display);
        }
        selectedRuleDisplays.Clear();
        
        foreach (Rule rule in selectedPositiveRules)
        {
            CreateSelectedRuleDisplay(rule);
        }
        
        foreach (Rule rule in selectedRestrictionRules)
        {
            CreateSelectedRuleDisplay(rule);
        }
    }
    
    void CreateSelectedRuleDisplay(Rule rule)
    {
        if (selectedRuleDisplayPrefab == null || rule == null)
            return;
            
        GameObject display = Instantiate(selectedRuleDisplayPrefab, selectedRulesContainer);
        selectedRuleDisplays.Add(display);
        
        Image icon = display.GetComponentInChildren<Image>();
        TextMeshProUGUI nameText = display.GetComponentInChildren<TextMeshProUGUI>();
        
        if (icon != null && rule.ruleCard != null)
            icon.sprite = rule.ruleCard;
            
        if (nameText != null)
            nameText.text = rule.ruleName;
    }
    
    void CompleteSelection()
    {
        PlayNextSound();
        ApplySelectedRules();
        StartCoroutine(LoadGameSceneWithDelay(0.5f));
    }
    
    void ApplySelectedRules()
    {
        EnsureRuleManagerExists();
        
        if (RuleManager.Instance == null)
        {
            Debug.LogError("Cannot apply rules: RuleManager.Instance is still null!");
            return;
        }
        
        RuleManager.Instance.ClearAllRules();
        
        foreach (Rule rule in selectedPositiveRules)
        {
            if (rule != null)
            {
                bool success = RuleManager.Instance.AddRule(rule);
                Debug.Log($"Applied positive rule: {rule.ruleName} - Success: {success}");
            }
        }
        
        foreach (Rule rule in selectedRestrictionRules)
        {
            if (rule != null)
            {
                bool success = RuleManager.Instance.AddRule(rule);
                Debug.Log($"Applied restriction rule: {rule.ruleName} - Success: {success}");
            }
        }
    }
    
    System.Collections.IEnumerator LoadGameSceneWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(gameSceneName);
    }
    
    void GoBack()
    {
        SceneManager.LoadScene(menuSceneName);
    }
    
    // Audio methods
    void PlaySelectSound()
    {
        if (audioSource != null && cardSelectSound != null)
            audioSource.PlayOneShot(cardSelectSound);
    }
    
    void PlayDeselectSound()
    {
        if (audioSource != null && cardDeselectSound != null)
            audioSource.PlayOneShot(cardDeselectSound);
    }
    
    void PlayNextSound()
    {
        if (audioSource != null && nextSound != null)
            audioSource.PlayOneShot(nextSound);
    }
    
    void PlayErrorSound()
    {
        if (audioSource != null && errorSound != null)
            audioSource.PlayOneShot(errorSound);
    }
}
