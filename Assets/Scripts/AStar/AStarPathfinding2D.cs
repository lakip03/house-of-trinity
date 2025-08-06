using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class AStarPathfinding2D : MonoBehaviour
{
    private PathFindingGrid grid;

    void Start()
    {
        grid = FindAnyObjectByType<PathFindingGrid>();
        if (grid == null)
        {
            Debug.LogError("Pathfinding grid nto found... Please add it to the scene");
        }
    }
    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        if (grid == null) return new List<Vector3>();

        Vector2Int startGrid = grid.WorldToGrid(startPos);
        Vector2Int targetGrid = grid.WorldToGrid(targetPos);

        Node startNode = grid.GetNode(startGrid);
        Node targetNode = grid.GetNode(targetGrid);

        if (startNode == null || targetNode == null || !targetNode.isWalkable)
            return new List<Vector3>();

        List<Node> openSet = new List<Node>();
        List<Node> closedSet = new List<Node>();

        ResetNodes();

        openSet.Add(startNode);
        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);

        while (openSet.Count > 0)
        {
            Node currentNode = GetLowestFCostNode(openSet);

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                List<Vector3> path = RetracePath(startNode, targetNode);
                grid.SetCurrentPath(path);
                return path;
            }
            foreach (Node neighbor in grid.GetWalkableNeighbors(currentNode))
            {
                if (closedSet.Contains(neighbor)) continue;

                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        return new List<Vector3>();
    }

    private List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();

        List<Vector3> worldPath = new List<Vector3>();
        foreach (Node node in path)
        {
            worldPath.Add(grid.GridToWorld(node.position));
        }

        return worldPath;
    }

    Node GetLowestFCostNode(List<Node> nodeList)
    {
        Node lowestFCostNode = nodeList[0];
        for (int i = 1; i < nodeList.Count; i++)
        {
            if (nodeList[i].fCost < lowestFCostNode.fCost ||
                (nodeList[i].fCost == lowestFCostNode.fCost && nodeList[i].hCost < lowestFCostNode.hCost))
            {
                lowestFCostNode = nodeList[i];
            }
        }
        return lowestFCostNode;
    }
    void ResetNodes()
    {
        for (int x = 0; x < grid.gridWidth; x++)
        {
            for (int y = 0; y < grid.gridHeight; y++)
            {
                Node node = grid.GetNode(new Vector2Int(x, y));
                if (node != null)
                {
                    node.gCost = int.MaxValue;
                    node.hCost = 0;
                    node.parent = null;
                }
            }
        }
    }
    int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
        int distY = Mathf.Abs(nodeA.position.y - nodeB.position.y);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }
}