using UnityEngine;

/// <summary>
/// Represents a single node in a 2D pathfinding grid used by A* algorithm.
/// Contains position information, walkability status, and pathfinding costs.
/// </summary>
public class Node
{
    /// <summary>
    /// The grid coordinates of this node in 2D space.
    /// </summary>
    public Vector2Int position;
    
    /// <summary>
    /// Indicates whether this node can be traversed during pathfinding.
    /// Set to false for walls
    /// </summary>
    public bool isWalkable;
    
    /// <summary>
    /// The movement cost from the starting node to this node.
    /// In A* pathfinding, this represents the actual distance traveled so far.
    /// </summary>
    public int gCost;
    
    /// <summary>
    /// The heuristic cost (estimated distance) from this node to the target node.
    /// </summary>
    public int hCost;
    
    /// <summary>
    /// Reference to the previous node in the optimal path.
    /// Used to reconstruct the final path once the target is reached by backtracking through parent nodes.
    /// </summary>
    public Node parent;
    
    /// <summary>
    /// Gets the total pathfinding cost for this node (gCost + hCost).
    /// Used by A* algorithm to determine the most promising node to explore next.
    /// </summary>
    public int fCost => gCost + hCost;

    /// <summary>
    /// Initializes a new instance of the Node class.
    /// </summary>
    /// <param name="pos">The grid position for this node.</param>
    /// <param name="_isWalkable">Whether this node can be traversed.</param>
    public Node(Vector2Int pos, bool _isWalkable)
    {
        position = pos;
        isWalkable = _isWalkable;
    }
}