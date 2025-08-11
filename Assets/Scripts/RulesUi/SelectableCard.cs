using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual rule card UI component with visual feedback for selection states.
/// Used to be color coded, but had to remove that feature becacue it made cards look worse and didn't have time
/// for a new implementation.
/// </summary>
public class SelectableCard : MonoBehaviour
{
    [Header("UI References")]
    public Image cardImage;
    public Image cardBorder;
    public Image ruleIcon;
    public TextMeshProUGUI ruleName;
    public TextMeshProUGUI ruleDescription;
    public Button cardButton;
    public GameObject selectedIndicator;

    [Header("Card Type Colors")]
    public Color movementColor = Color.blue;
    public Color healthColor = Color.green;
    public Color temporalColor = Color.purple;
    public Color restrictionColor = Color.red;

    [Header("Selection Colors")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color disabledColor = Color.gray;

    private Rule associatedRule;
    private bool isSelected = false;
    private CardSelectionManager selectionManager;

    public Rule AssociatedRule => associatedRule;
    public bool IsSelected => isSelected;

    void Awake()
    {
        if (cardButton == null)
            cardButton = GetComponent<Button>();

        if (cardButton != null)
        {
            cardButton.onClick.AddListener(OnCardClicked);
        }
        else
        {
            Debug.LogError($"Button component not found on {gameObject.name}! Make sure SelectableCard has a Button component.");
        }
    }

    public void Initialize(Rule rule, CardSelectionManager manager)
    {
        associatedRule = rule;
        selectionManager = manager;

        if (ruleIcon != null && rule.ruleCard != null)
            ruleIcon.sprite = rule.ruleCard;

        if (ruleName != null)
            ruleName.text = rule.ruleName;

        if (ruleDescription != null)
            ruleDescription.text = rule.ruleDescription;

        SetCardTypeColor();
        UpdateVisualState();
    }

    void SetCardTypeColor()
    {
        Color typeColor = Color.white;

        switch (associatedRule.ruleType)
        {
            case RuleType.Movement:
                typeColor = movementColor;
                break;
            case RuleType.Health:
                typeColor = healthColor;
                break;
            case RuleType.Temporal:
                typeColor = temporalColor;
                break;
            case RuleType.Restriction:
                typeColor = restrictionColor;
                break;
        }

        if (cardBorder != null)
            cardBorder.color = typeColor;
    }

    void OnCardClicked()
    {
        if (cardButton.interactable && selectionManager != null)
        {
            selectionManager.OnCardClicked(this);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisualState();
    }

    public void SetInteractable(bool interactable)
    {
        cardButton.interactable = interactable;
        UpdateVisualState();
    }

    void UpdateVisualState()
    {
        if (!cardButton.interactable)
        {
            cardImage.color = disabledColor;
        }
        else if (isSelected)
        {
            cardImage.color = selectedColor;
        }
        else
        {
            cardImage.color = normalColor;
        }

        if (selectedIndicator != null)
            selectedIndicator.SetActive(isSelected);
    }
}