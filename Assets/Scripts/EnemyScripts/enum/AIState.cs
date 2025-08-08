/// <summary>
/// Defines the different behavioral states for AI entities in the game.
/// Used to control AI decision-making and behavior patterns.
/// </summary>
public enum AIState
{
    /// <summary>
    /// The AI is in a passive patrol state, rotating and scanning for targets.
    /// This is the default state when no player has been detected.
    /// </summary>
    Patrolling,
    
    /// <summary>
    /// The AI has detected the player and is actively pursuing them.
    /// The AI will move directly toward the player's current position.
    /// </summary>
    Chasing,
    
    /// <summary>
    /// The AI has lost sight of the player but is actively searching for them.
    /// The AI will investigate the last known position and search the surrounding area.
    /// </summary>
    Searching
}