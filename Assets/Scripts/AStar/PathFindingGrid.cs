using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Manages a 2D grid of nodes for pathfinding algorithms. Creates and maintains a grid
/// where each cell can be walkable or blocked, and provides coordinate conversion between
/// world space and grid space. Includes visualization tools for debugging pathfinding.
/// </summary>
public class PathFindingGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float nodeSize = 1f;
    public LayerMask obstacleLayer;

    [Header("Grid Offset")]
    [Tooltip("Offset from the GameObject's position")]
    public Vector2 gridOffset = Vector2.zero;

    [Header("Visualization")]
    public bool showGrid = true;
    public bool showGridInPlayMode = false;
    public Color walkableColor = Color.white;
    public Color obstacleColor = Color.red;
    public Color pathColor = Color.green;
    public Color gridOriginColor = Color.yellow;

    [Header("Debug Info")]
    [SerializeField] private Vector3 gridWorldOrigin;
    private Node[,] grid;
    private List<Vector3> currentPath = new List<Vector3>();

    void Start()
    {
        CreateGrid();
    }

    void OnValidate()
    {
        UpdateGridOrigin();
    }

    /// <summary>
    /// Calculates and updates the world position of the grid origin based on transform position and offset.
    /// </summary>
    private void UpdateGridOrigin()
    {
        gridWorldOrigin = transform.position + new Vector3(gridOffset.x, gridOffset.y, 0);
    }

    /// <summary>
    /// Creates the pathfinding grid by initializing all nodes and checking for obstacles.
    /// Each node's walkability is determined by checking for Physics2D overlaps with the obstacle layer.
    /// </summary>
    private void CreateGrid()
    {
        UpdateGridOrigin();
        grid = new Node[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPosition = GetWorldPositionFromGrid(x, y);
                bool walkable = !Physics2D.OverlapCircle(worldPosition, nodeSize * 0.4f, obstacleLayer);
                grid[x, y] = new Node(new Vector2Int(x, y), walkable);
            }
        }
    }

    /// <summary>
    /// Converts grid coordinates to world position.
    /// </summary>
    /// <param name="gridX">The X coordinate in the grid.</param>
    /// <param name="gridY">The Y coordinate in the grid.</param>
    /// <returns>The corresponding world position.</returns>
    private Vector3 GetWorldPositionFromGrid(int gridX, int gridY)
    {
        return gridWorldOrigin + new Vector3(gridX * nodeSize, gridY * nodeSize, 0);
    }

    /// <summary>
    /// Gets the node at the specified grid position.
    /// </summary>
    /// <param name="gridPos">The grid coordinates to retrieve the node from.</param>
    /// <returns>The node at the specified position, or null if coordinates are out of bounds.</returns>
    public Node GetNode(Vector2Int gridPos)
    {
        if (gridPos.x >= 0 && gridPos.x < gridWidth && gridPos.y >= 0 && gridPos.y < gridHeight)
            return grid[gridPos.x, gridPos.y];
        return null;
    }

    /// <summary>
    /// Converts a world position to grid coordinates.
    /// </summary>
    /// <param name="worldPos">The world position to convert.</param>
    /// <returns>The corresponding grid coordinates.</returns>
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        UpdateGridOrigin();
        Vector3 localPos = worldPos - gridWorldOrigin;
        int x = Mathf.RoundToInt(localPos.x / nodeSize);
        int y = Mathf.RoundToInt(localPos.y / nodeSize);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Converts grid coordinates to world position.
    /// </summary>
    /// <param name="gridPos">The grid coordinates to convert.</param>
    /// <returns>The corresponding world position.</returns>
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return GetWorldPositionFromGrid(gridPos.x, gridPos.y);
    }

    /// <summary>
    /// Gets all walkable neighboring nodes for a given node.
    /// Filters the results of GetNeighbors to only include walkable nodes.
    /// </summary>
    /// <param name="node">The node to find walkable neighbors for.</param>
    /// <returns>A list of walkable neighboring nodes.</returns>
    public List<Node> GetWalkableNeighbors(Node node)
    {
        List<Node> neighbors = GetNeighbors(node);
        List<Node> walkableNeighbors = new List<Node>();

        foreach (Node neighbor in neighbors)
        {
            if (neighbor.isWalkable)
            {
                walkableNeighbors.Add(neighbor);
            }
        }
        return walkableNeighbors;
    }

    /// <summary>
    /// Gets all neighboring nodes (including diagonal neighbors) for a given node.
    /// This includes both walkable and non-walkable neighbors within the grid bounds.
    /// </summary>
    /// <param name="node">The node to find neighbors for.</param>
    /// <returns>A list of all neighboring nodes within grid bounds.</returns>
    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector2Int neighborPos = node.position + new Vector2Int(x, y);
                Node neighbor = GetNode(neighborPos);

                if (neighbor != null)
                    neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }

    /// <summary>
    /// Sets the current path for visualization purposes.
    /// </summary>
    /// <param name="path">The path in world coordinates to display.</param>
    public void SetCurrentPath(List<Vector3> path)
    {
        currentPath = path;
    }

    /// <summary>
    /// Refreshes the grid by recreating all nodes and rechecking obstacles.
    /// Only works during play mode.
    /// </summary>
    public void RefreshGrid()
    {
        if (Application.isPlaying)
            CreateGrid();
    }

    /// <summary>
    /// Draws grid visualization gizmos in the Scene view.
    /// Shows the grid origin, grid bounds, individual nodes colored by walkability, and current path.
    /// </summary>
    void OnDrawGizmos()
    {
        if (!showGrid || (!Application.isEditor && !showGridInPlayMode))
            return;

        UpdateGridOrigin();

        // Draw grid origin
        Gizmos.color = gridOriginColor;
        Gizmos.DrawWireSphere(gridWorldOrigin, nodeSize * 0.3f);

        // Draw grid bounds
        Vector3 gridCenter = gridWorldOrigin + new Vector3((gridWidth - 1) * nodeSize * 0.5f, (gridHeight - 1) * nodeSize * 0.5f, 0);
        Vector3 gridSize = new Vector3(gridWidth * nodeSize, gridHeight * nodeSize, 0);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(gridCenter, gridSize);

        // Draw individual nodes
        if (grid != null)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3 worldPos = GetWorldPositionFromGrid(x, y);
                    Gizmos.color = grid[x, y].isWalkable ? walkableColor : obstacleColor;
                    Gizmos.DrawWireCube(worldPos, Vector3.one * nodeSize * 0.8f);
                }
            }
        }
        else
        {
            // Draw preview when grid hasn't been created yet
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3 worldPos = GetWorldPositionFromGrid(x, y);
                    bool walkable = !Physics2D.OverlapCircle(worldPos, nodeSize * 0.4f, obstacleLayer);
                    Gizmos.color = walkable ? walkableColor : obstacleColor;
                    Gizmos.DrawWireCube(worldPos, Vector3.one * nodeSize * 0.8f);
                }
            }
        }

        // Draw current path
        if (currentPath != null && currentPath.Count > 1)
        {
            Gizmos.color = pathColor;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
                Gizmos.DrawSphere(currentPath[i], nodeSize * 0.2f);
            }
            Gizmos.DrawSphere(currentPath[currentPath.Count - 1], nodeSize * 0.2f);
        }
    }

    /// <summary>
    /// Draws additional gizmos when this GameObject is selected in the editor.
    /// Shows grid coordinate labels for easier debugging.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (!showGrid) return;

        UpdateGridOrigin();

#if UNITY_EDITOR
        if (Application.isEditor)
        {
            Handles.color = Color.white;
            for (int x = 0; x < gridWidth; x += 5)
            {
                for (int y = 0; y < gridHeight; y += 5)
                {
                    Vector3 worldPos = GetWorldPositionFromGrid(x, y);
                    Handles.Label(worldPos, $"({x},{y})");
                }
            }
        }
#endif
    }
}