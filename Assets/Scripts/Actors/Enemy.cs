using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : NodeMover
{
    // TODO: this should not be a static member,
    // should be associated with the grid itself.
    /// <summary>
    /// All enemies currently on the grid.
    /// </summary>
    public static List<Enemy> AllEnemies { get; private set; }

    public event Action ReachedDestination;

    static Enemy()
    {
        AllEnemies = new List<Enemy>();
    }

    [SerializeField] private int hitPoints = 5;
    [SerializeField] private SpriteRenderer spriteRenderer = null;

    private void OnValidate()
    {
        hitPoints = Mathf.Clamp(hitPoints, 1, int.MaxValue);
    }

    public virtual void Hit(int damage)
    {
        hitPoints -= damage;
        if (hitPoints <= 0)
            OnDefeated();
    }

    public override void StartRoute()
    {
        base.StartRoute();
        AllEnemies.Add(this);
    }

    protected override void WhileUpdatingMovement()
    {
        if (Velocity.x > 0f)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;
    }

    protected override void OnRouteComplete()
    {
        ReachedDestination?.Invoke();
        Defeated?.Invoke();
        AllEnemies.Remove(this);
        // TODO: gameobjects should be cached
        // and reused, or use ECS.
        Destroy(gameObject);
    }
    protected void OnDefeated()
    {
        Defeated?.Invoke();
        AllEnemies.Remove(this);
        // TODO: gameobjects should be cached
        // and reused, or use ECS.
        Destroy(gameObject);
    }


    public event Action Defeated;
}
