using UnityEngine;

/// <summary>
/// Persistent data container tracking player progress and statistics across the "Three Rules" game.
/// Stores level progression, performance metrics, and current game state.
/// </summary>
[System.Serializable]
public class GameFlowData : ScriptableObject
{
    [Header("Progression")]
    public int currentLevel = 1;
    public int levelsCompleted = 0;
    
    [Header("Statistics")]
    public int playerDeaths = 0;
    public float totalPlayTime = 0f;
    
    [Header("State")]
    public GameState currentState = GameState.MainMenu;
    
    public void Initialize()
    {
        currentLevel = 1;
        levelsCompleted = 0;
        playerDeaths = 0;
        totalPlayTime = 0f;
        currentState = GameState.MainMenu;
    }
}