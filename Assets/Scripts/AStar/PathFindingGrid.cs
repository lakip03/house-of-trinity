using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    private void UpdateGridOrigin()
    {
        gridWorldOrigin = transform.position + new Vector3(gridOffset.x, gridOffset.y, 0);
    }

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

    private Vector3 GetWorldPositionFromGrid(int gridX, int gridY)
    {
        return gridWorldOrigin + new Vector3(gridX * nodeSize, gridY * nodeSize, 0);
    }

    public Node GetNode(Vector2Int gridPos)
    {
        if (gridPos.x >= 0 && gridPos.x < gridWidth && gridPos.y >= 0 && gridPos.y < gridHeight)
            return grid[gridPos.x, gridPos.y];
        return null;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        UpdateGridOrigin();
        Vector3 localPos = worldPos - gridWorldOrigin;
        int x = Mathf.RoundToInt(localPos.x / nodeSize);
        int y = Mathf.RoundToInt(localPos.y / nodeSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return GetWorldPositionFromGrid(gridPos.x, gridPos.y);
    }

    public List<Node> GetWalkableNeighbors(Node node)
    {
        List<Node> neighbors = GetNeighbors(node);
        List<Node> walkableNeighbors = new List<Node>();

        foreach (Node neighbor in neighbors)
        {
            if (neighbor.isWalkable) // ← This check is crucial!
            {
                walkableNeighbors.Add(neighbor);
            }
        }
        return walkableNeighbors;
    }

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

    public void SetCurrentPath(List<Vector3> path)
    {
        currentPath = path;
    }

    public void RefreshGrid()
    {
        if (Application.isPlaying)
            CreateGrid();
    }

    void OnDrawGizmos()
    {
        if (!showGrid || (!Application.isEditor && !showGridInPlayMode))
            return;

        UpdateGridOrigin();

        Gizmos.color = gridOriginColor;
        Gizmos.DrawWireSphere(gridWorldOrigin, nodeSize * 0.3f);

        Vector3 gridCenter = gridWorldOrigin + new Vector3((gridWidth - 1) * nodeSize * 0.5f, (gridHeight - 1) * nodeSize * 0.5f, 0);
        Vector3 gridSize = new Vector3(gridWidth * nodeSize, gridHeight * nodeSize, 0);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(gridCenter, gridSize);

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