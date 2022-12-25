using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;


public class AStarGrid : MonoBehaviour
{
    private AStarNode[,] grid;

    [SerializeField] Vector2 navMeshSize;
    Vector2Int gridSize;
    [SerializeField] float cellSize;

    [SerializeField] bool showGrid;

    private void OnValidate()
    {
        if (showGrid && !Application.isPlaying)
            gridSize = new Vector2Int(Mathf.FloorToInt(navMeshSize.x / cellSize), Mathf.FloorToInt(navMeshSize.y / cellSize));
    }

    private void Awake()
    {
        gridSize = new Vector2Int(Mathf.FloorToInt(navMeshSize.x / cellSize), Mathf.FloorToInt(navMeshSize.y / cellSize));
        grid = new AStarNode[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                grid[x, y] = new AStarNode(x, y);
                //Physics.OverlapSphere(GridToWorld(new Vector2Int(x, y)), cellSize / 2f, LayerMask.NameToLayer("Obstacles") | LayerMask.NameToLayer("Walls"));
                grid[x, y].value = Physics.CheckSphere(GridToWorld(new Vector2Int(x, y)), cellSize / 2f, LayerMask.NameToLayer("Obstacles") | LayerMask.NameToLayer("Walls")) ? 0 : 1;
            }
        }
    }

    public int Length { get { return grid.Length; } }

    public void UpdateValue(Vector2Int pos, int newValue)
    {
        grid[pos.x, pos.y].value = newValue;
    }

    public Vector3 GridToWorld(Vector2Int pos)
    {
        return new Vector3(
                        transform.position.x + (pos.x + (1 - gridSize.x) / 2f) * cellSize,
                        transform.position.y,
                        transform.position.z + (pos.y + (1 - gridSize.y) / 2f) * cellSize);
    }

    public Vector2Int WorldToGrid(Vector3 pos)
    {
        pos -= transform.position;
        pos /= cellSize;
        return new Vector2Int(
                        Mathf.FloorToInt(pos.x + gridSize.x / 2),
                        Mathf.FloorToInt(pos.z + gridSize.y / 2));
    }

    public AStarNode GetNodeAt(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= grid.GetLength(0) || pos.y < 0 || pos.y >= grid.GetLength(1))
        {
            pos.x = gridSize.x / 2;
            pos.y = gridSize.y / 2;
            Debug.LogWarning("WARN: Requested grid position out of bounds");
        }

        return grid[pos.x, pos.y];
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

                if (checkX > 0 && checkX < grid.GetLength(0) - 1 && checkY > 0 && checkY < grid.GetLength(1) - 1 && grid[checkX, checkY].value != 0) // if the coord exists & isn't impassible
                {
                    if (!(x == 0 || y == 0)) // if it is diagonal
                    {
                        if (grid[node.gridX, checkY].value == 0 || grid[checkX, node.gridY].value == 0) // if the horizontals are impassible
                            continue;
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
        Gizmos.DrawWireCube(transform.position, new Vector3(gridSize.x, 0, gridSize.y) * cellSize);
    }

    private void OnDrawGizmos()
    {
        if (showGrid)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Gizmos.color = Color.white;

                    Gizmos.DrawCube(GridToWorld(new Vector2Int(x, y)),
                        0.8f * cellSize * Vector3.one);
                }
            }
        }
    }
}
