using UnityEngine;

public class Node
{
    public Vector2Int position;
    public bool isWalkable;
    public int gCost;
    public int hCost;
    public Node parent;
    public int fCost => gCost + hCost;

    public Node(Vector2Int pos, bool _isWalkable)
    {
        position = pos;
        isWalkable = _isWalkable;
    }
}
