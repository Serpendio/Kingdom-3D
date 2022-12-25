using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarAlgo : MonoBehaviour
{
    public static AStarAlgo instance;

    [SerializeField] AStarGrid grid;

    private void Awake()
    {
        instance = this;
    }

    public List<Vector2Int> FindInitialCorridor(Vector2Int startPos, Vector2Int endPos)
    {
        AStarNode startNode = grid.GetNodeAt(startPos);
        AStarNode targetNode = grid.GetNodeAt(endPos);

        Heap<AStarNode> openSet = new(grid.Length);
        HashSet<AStarNode> closedSet = new();

        openSet.Add(startNode);

        List<Vector2Int> path = new();

        while (openSet.Count > 0)
        {
            AStarNode currentNode = openSet.RemoveFirst();

            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                currentNode = targetNode;
                while (currentNode.parent != null)
                {
                    path.Add(new Vector2Int(currentNode.gridX, currentNode.gridY));
                    currentNode = currentNode.parent;
                }
                return path;
            }

            foreach (AStarNode neighbor in grid.GetNeighborsOctagonal(currentNode))
            {
                if (closedSet.Contains(neighbor)) continue;

                int moveCost = currentNode.gCost + AStarGrid.GetDistanceOctagonal(currentNode, neighbor) * neighbor.value;

                if (moveCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = moveCost;
                    neighbor.hCost = AStarGrid.GetDistanceOctagonal(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                    else
                    {
                        openSet.UpdateItem(neighbor);
                    }
                }
            }
        }

        path.Add(startPos);
        return path;
    }
}
