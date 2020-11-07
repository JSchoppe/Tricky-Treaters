using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tower : MonoBehaviour
{
    protected NodeGrid grid;

    protected Vector2 location;

    public virtual void OnPlacement(NodeGrid inGrid)
    {
        grid = inGrid;
        location = transform.position;
    }

}
