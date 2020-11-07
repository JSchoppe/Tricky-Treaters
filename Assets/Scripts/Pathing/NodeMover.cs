using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class NodeMover : MonoBehaviour
{
    [Tooltip("Speed of the mover in grid units per second.")]
    [SerializeField] private float speed = 1f;

    [HideInInspector] public WaveBatch parentBatch;
    [HideInInspector] public NodeGrid grid;

    private Vector2 position;
    private int pathIndex;

    public Vector2 Position { get { return position; } }
    public Vector2 Velocity { get; private set; }

    public Vector2Int[] Path { get; set; }

    protected virtual void OnRouteComplete() { }

    public virtual void StartRoute()
    {
        pathIndex = 0;
        position = Path[0];
        StartCoroutine(UpdateMovement());
        parentBatch.BatchPathRecalculated += OnPathRecalculated;
    }

    public void OnPathRecalculated(Vector2Int[] newRoute)
    {
        // If the mover is on the final joint of the path,
        // do not bother recalculating. This helps avoid
        // edge case checks below.
        if (pathIndex == Path.Length - 1)
            return;

        // Check to see if the new path intersects
        // the current path; in that case we can
        // truncate and use the new path.
        bool foundConvergence = false;
        for (int i = 0; i < newRoute.Length; i++)
        {
            // Look for the point we are on route for
            // in the new route.
            if (newRoute[i] == Path[pathIndex + 1])
            {
                foundConvergence = true;
                // Accumulate the remaining path.
                Path = new Vector2Int[newRoute.Length - i];
                for (int j = i; j < newRoute.Length; j++)
                {
                    Path[j - i] = newRoute[j];
                }
                pathIndex = 0;
                break;
            }
        }
        if (!foundConvergence)
        {
            // If the new route can not be immediately joined
            // then run A* path finding from current position.
            if (grid.TryFindRoute(Path[pathIndex + 1], newRoute[newRoute.Length - 1], out Vector2Int[] foundPath))
            {
                Path = foundPath;
                pathIndex = 0;
            }
        }
    }

    protected virtual void WhileUpdatingMovement() { }

    private IEnumerator UpdateMovement()
    {
        bool reachedPathEnd = false;
        while (true)
        {
            float movement = speed * Time.deltaTime;
            while (movement > 0f)
            {
                float distanceToNextPoint = Vector2.Distance(position, Path[pathIndex + 1]);
                if (movement > distanceToNextPoint)
                {
                    position = Path[pathIndex + 1];
                    movement -= distanceToNextPoint;
                    pathIndex++;
                    if (pathIndex >= Path.Length - 1)
                    {
                        reachedPathEnd = true;
                        break;
                    }
                }
                else
                {
                    Vector2 direction = (Path[pathIndex + 1] - position).normalized;
                    position += direction * movement;
                    movement = 0f;
                    // Update the velocity for external observers.
                    Velocity = direction * speed;
                }
            }
            transform.position = (Vector2)grid.transform.position + (position + Vector2.one * 0.5f) * grid.GridUnit;

            if (reachedPathEnd)
                break;
            else
            {
                WhileUpdatingMovement();
                yield return null;
            }
        }
        OnRouteComplete();
    }
}
