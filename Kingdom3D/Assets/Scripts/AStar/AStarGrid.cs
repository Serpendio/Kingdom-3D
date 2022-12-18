using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class AStarGrid : MonoBehaviour
{
    private AStarNode[,] grid;

    [SerializeField] Vector2Int gridSize;
    [SerializeField] float cellSize;

    private void Awake()
    { 
        grid = new AStarNode[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                grid[x, y] = new AStarNode(x, y);
            }
        }
    }
    public int Length { get { return grid.Length; } }

    public void UpdateValue(int x, int y, int newValue)
    {
        grid[x, y].value = newValue;
    }

    public AStarNode GetNodeAt(int x, int y)
    {
        if (x < 0 || x >= grid.GetLength(0) || y < 0 || y >= grid.GetLength(1))
        {
            x = gridSize.x / 2;
            y = gridSize.y / 2;
            Debug.LogWarning("WARN: Requested grid position out of bounds");
        }

        return grid[x, y];
    }
    public List<AStarNode> GetNeighborsOctagonal(AStarNode node)
    {
        List<AStarNode> neighbors = new();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX > 0 && checkX < grid.GetLength(0) - 1 && checkY > 0 && checkY < grid.GetLength(1) - 1 && grid[checkX, checkY].value > 0)
                {
                    if (!(x == 0 || y == 0))
                    {
                        if (!(grid[node.gridX, checkY].value > 0 && grid[checkX, node.gridY].value > 0)) continue;
                    }

                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbors;
    }

    public static int GetDistanceOctagonal(AStarNode a, AStarNode b)
    {
        int distX = Mathf.Abs(a.gridX - b.gridX);
        int distY = Mathf.Abs(a.gridY - b.gridY);

        return distX > distY ? 14 * distY + 10 * (distX - distY) : 14 * distX + 10 * (distY - distX); // taking cardinal cost as 10, though 70 and 99 is probably even better
    }

    private void OnDrawGizmosSelected()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Gizmos.color = Color.white;

                Gizmos.DrawCube(new Vector3(
                    transform.position.x + (x - (gridSize.x - cellSize) / 2f) * cellSize,
                    transform.position.y,
                    transform.position.z + (y - (gridSize.y - cellSize) / 2f) * cellSize),
                    0.8f * cellSize * Vector3.one);
            }
        }
    }
}
