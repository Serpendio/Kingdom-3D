

public class AStarNode : IHeapItem<AStarNode>
{
    public int value;
    public int gridX;
    public int gridY;
    public AStarNode parent;

    public int gCost; // cost from start
    public int hCost; // estimated cost to end
    int heapIndex;

    public AStarNode(int gridX, int gridY)
    {
        this.gridX = gridX;
        this.gridY = gridY;
        gCost = int.MaxValue;
        hCost = int.MaxValue;
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex { get => heapIndex; set => heapIndex = value; }

    public int CompareTo(AStarNode other)
    {
        int compare = fCost.CompareTo(other.fCost);

        if (compare == 0)
        {
            compare = hCost.CompareTo(other.hCost);
        }

        return -compare;
    }
}
