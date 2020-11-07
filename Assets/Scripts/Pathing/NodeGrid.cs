using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class NodeGrid : MonoBehaviour
{
    [SerializeField] private WaveManager waveManager = null;


    public float GridUnit { get { return gridUnit; } }

    public Vector2 OriginTile { get { return (Vector2)transform.position + 0.5f * GridUnit * Vector2.one; } }

    private EnemyPath[] enemyPaths;

    public int Width { get { return width; } }
    public int Height { get { return height; } }

    public bool IsTileBuildLegal(int x, int y)
    {
        return !illegalTiles[x, y];
    }

    public void MarkTileOccupied(int x, int y)
    {
        tileIsBlocked[x, y] = true;
        illegalTiles[x, y] = true;
        MarkTilesIllegal();
        GridUpdated?.Invoke();
    }

    [SerializeField] private float gridUnit = 1f;
    [SerializeField] private int width = 3;
    [SerializeField] private int height = 3;
    [SerializeField] private List<Vector2Int> blockedTiles = null;
    private void OnValidate()
    {
        if (gridUnit < float.Epsilon)
            gridUnit = float.Epsilon;
        if (width < 1)
            width = 1;
        if (height < 1)
            height = 1;
        InitializePathing();
        if (waveManager != null)
            waveManager.OnValidateWaves();
    }
    private void OnDrawGizmos()
    {
        int x, y;
        Gizmos.color = Color.blue;
        for (x = 0; x <= width; x++)
            Gizmos.DrawRay(transform.position + x * gridUnit * Vector3.right,
                Vector3.up * gridUnit * height);
        for (y = 0; y <= height; y++)
            Gizmos.DrawRay(transform.position + y * gridUnit * Vector3.up,
                Vector3.right * gridUnit * width);
        Gizmos.color = Color.red;
        if (blockedTiles != null)
        {
            foreach (Vector2Int tile in blockedTiles)
            {
                float inset = gridUnit / 4f;
                Vector2 bottomLeft = (Vector2)transform.position +
                    new Vector2(tile.x * gridUnit, tile.y * gridUnit);
                Vector2 topLeft = bottomLeft + Vector2.up * gridUnit;
                Vector2 bottomRight = bottomLeft + Vector2.right * gridUnit;
                Vector2 topRight = new Vector2(bottomRight.x, topLeft.y);

                bottomLeft += inset * new Vector2(1, 1);
                topRight += inset * new Vector2(-1, -1);
                topLeft += inset * new Vector2(1, -1);
                bottomRight += inset * new Vector2(-1, 1);

                Gizmos.DrawLine(bottomLeft, topLeft);
                Gizmos.DrawLine(topLeft, topRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(bottomRight, bottomLeft);
            }
        }
        Gizmos.color = Color.yellow;
        for (x = 0; x < width; x++)
        {
            for (y = 0; y < height; y++)
            {
                if (illegalTiles[x, y])
                {
                    float inset = gridUnit / 8f;
                    Vector2 bottomLeft = (Vector2)transform.position +
                        new Vector2(x * gridUnit, y * gridUnit);
                    Vector2 topLeft = bottomLeft + Vector2.up * gridUnit;
                    Vector2 bottomRight = bottomLeft + Vector2.right * gridUnit;
                    Vector2 topRight = new Vector2(bottomRight.x, topLeft.y);

                    bottomLeft += inset * new Vector2(1, 1);
                    topRight += inset * new Vector2(-1, -1);
                    topLeft += inset * new Vector2(1, -1);
                    bottomRight += inset * new Vector2(-1, 1);

                    Gizmos.DrawLine(bottomLeft, topLeft);
                    Gizmos.DrawLine(topLeft, topRight);
                    Gizmos.DrawLine(topRight, bottomRight);
                    Gizmos.DrawLine(bottomRight, bottomLeft);
                }
            }
        }
    }

    private bool[,] tileIsBlocked;

    private bool[,] illegalTiles;

    /// <summary>
    /// Bind to this if your code needs to react to a change in available paths.
    /// </summary>
    public event Action GridUpdated;


    // Start is called before the first frame update
    void Start()
    {
        InitializePathing();
    }

    private void InitializePathing()
    {
        tileIsBlocked = new bool[width, height];
        illegalTiles = new bool[width, height];
        foreach (Vector2Int tile in blockedTiles)
        {
            tileIsBlocked[tile.x, tile.y] = true;
            illegalTiles[tile.x, tile.y] = true;
        }
        routeData = new RouteNodeData[width, height];
        enemyPaths = waveManager.GetAllEnemyPaths();
        MarkTilesIllegal();
    }

    private void MarkTilesIllegal()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!illegalTiles[x, y])
                {
                    tileIsBlocked[x, y] = true;
                    foreach (EnemyPath path in enemyPaths)
                        if (!TryFindRoute(path.start, path.end, out _))
                            illegalTiles[x, y] = true;
                    tileIsBlocked[x, y] = false;
                }
            }
        }
    }

    private RouteNodeData[,] routeData;

    public bool TryFindRoute(Vector2Int start, Vector2Int end, out Vector2Int[] path)
    {
        if (routeData == null)
            InitializePathing();

        // If at the end, go directly there.
        if (start.x == end.x && start.y == end.y)
        {
            path = new Vector2Int[] { start };
            return true;
        }

        // Clear previous pathfinding state.	
        ResetNodeData();
        // Initialize the collection for A*.	
        List<Vector2Int> openNodes = new List<Vector2Int>();
        // Initialize the starting node.	
        openNodes.Add(start);
        routeData[start.x, start.y].parent = new Vector2Int(-1, -1);
        routeData[start.x, start.y].gScore = 0f;
        routeData[start.x, start.y].hScore = CalculateHeuristic(start, end);

        // Start the A* algorithm.	
        while (openNodes.Count > 0)
        {
            // Find the best f score in the open nodes.	
            Vector2Int current = openNodes[0];
            for (int i = 1; i < openNodes.Count; i++)
                if (routeData[openNodes[i].x, openNodes[i].y].gScore + routeData[openNodes[i].x, openNodes[i].y].hScore 
                    < routeData[current.x, current.y].gScore + routeData[current.x, current.y].hScore)
                    current = openNodes[i];

            // Return the path if the end has been found.	
            if (current == end)
            {
                path = UnwindPath(current);
                return true;
            }

            openNodes.RemoveAll(node => node.x == current.x && node.y == current.y);
            // Try moving right.
            if (current.x + 1 < width
                && !tileIsBlocked[current.x + 1, current.y])
            {
                float newGScore = routeData[current.x, current.y].gScore + 1;
                if (newGScore < routeData[current.x + 1, current.y].gScore)
                {
                    // If this is a new best path to this node,	
                    // add it to the open nodes and calculate the heuristic.	
                    routeData[current.x + 1, current.y].gScore = newGScore;
                    routeData[current.x + 1, current.y].parent = current;
                    // If this is a new node, calculate its heuristic.
                    bool isNew = true;
                    foreach (Vector2Int node in openNodes)
                    {
                        if (node.x == current.x + 1 && node.y == current.y)
                        {
                            isNew = false;
                            break;
                        }
                    }
                    if (isNew)
                    {
                        Vector2Int newNode = new Vector2Int(current.x + 1, current.y);
                        routeData[current.x + 1, current.y].hScore =
                            CalculateHeuristic(newNode, end);
                        openNodes.Add(newNode);
                    }
                }
            }
            // Try moving left.
            if (current.x - 1 >= 0
                && !tileIsBlocked[current.x - 1, current.y])
            {
                float newGScore = routeData[current.x, current.y].gScore + 1;
                if (newGScore < routeData[current.x - 1, current.y].gScore)
                {
                    // If this is a new best path to this node,	
                    // add it to the open nodes and calculate the heuristic.	
                    routeData[current.x - 1, current.y].gScore = newGScore;
                    routeData[current.x - 1, current.y].parent = current;
                    // If this is a new node, calculate its heuristic.
                    bool isNew = true;
                    foreach (Vector2Int node in openNodes)
                    {
                        if (node.x == current.x - 1 && node.y == current.y)
                        {
                            isNew = false;
                            break;
                        }
                    }
                    if (isNew)
                    {
                        Vector2Int newNode = new Vector2Int(current.x - 1, current.y);
                        routeData[current.x - 1, current.y].hScore =
                            CalculateHeuristic(newNode, end);
                        openNodes.Add(newNode);
                    }
                }
            }
            // Try moving up.
            if (current.y + 1 < height
                && !tileIsBlocked[current.x, current.y + 1])
            {
                float newGScore = routeData[current.x, current.y].gScore + 1;
                if (newGScore < routeData[current.x, current.y + 1].gScore)
                {
                    // If this is a new best path to this node,	
                    // add it to the open nodes and calculate the heuristic.	
                    routeData[current.x, current.y + 1].gScore = newGScore;
                    routeData[current.x, current.y + 1].parent = current;
                    // If this is a new node, calculate its heuristic.
                    bool isNew = true;
                    foreach (Vector2Int node in openNodes)
                    {
                        if (node.x == current.x && node.y == current.y + 1)
                        {
                            isNew = false;
                            break;
                        }
                    }
                    if (isNew)
                    {
                        Vector2Int newNode = new Vector2Int(current.x, current.y + 1);
                        routeData[current.x, current.y + 1].hScore =
                            CalculateHeuristic(newNode, end);
                        openNodes.Add(newNode);
                    }
                }
            }
            // Try moving down.
            if (current.y - 1 >= 0
                && !tileIsBlocked[current.x, current.y - 1])
            {
                float newGScore = routeData[current.x, current.y].gScore + 1;
                if (newGScore < routeData[current.x, current.y - 1].gScore)
                {
                    // If this is a new best path to this node,	
                    // add it to the open nodes and calculate the heuristic.	
                    routeData[current.x, current.y - 1].gScore = newGScore;
                    routeData[current.x, current.y - 1].parent = current;
                    // If this is a new node, calculate its heuristic.
                    bool isNew = true;
                    foreach (Vector2Int node in openNodes)
                    {
                        if (node.x == current.x && node.y == current.y - 1)
                        {
                            isNew = false;
                            break;
                        }
                    }
                    if (isNew)
                    {
                        Vector2Int newNode = new Vector2Int(current.x, current.y - 1);
                        routeData[current.x, current.y - 1].hScore =
                            CalculateHeuristic(newNode, end);
                        openNodes.Add(newNode);
                    }
                }
            }
        }
        // A* pathfinding failed to find a path.	
        path = new Vector2Int[0];
        return false;
    }

    private float CalculateHeuristic(Vector2Int node, Vector2Int end)
    {
        // The heuristic is square magnitude.
        return node.x * node.x + node.y * node.y;
    }

    private Vector2Int[] UnwindPath(Vector2Int node)
    {
        // Unwind the path using the parent field.
        Stack<Vector2Int> path = new Stack<Vector2Int>();
        path.Push(node);
        // Watch for sentinel to exit.
        while (routeData[node.x, node.y].parent.x != -1)
        {
            node = routeData[node.x, node.y].parent;
            path.Push(node);
        }
        // Return the path found.	
        return path.ToArray();
    }

    private void ResetNodeData()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                routeData[x, y].hScore = float.MaxValue / 2f - float.Epsilon;
                routeData[x, y].gScore = routeData[x, y].hScore;
            }
        }
    }
    private struct RouteNodeData
    {
        public Vector2Int parent;
        public float gScore;
        public float hScore;
    }
}
