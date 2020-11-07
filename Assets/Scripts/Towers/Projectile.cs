using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO this is horrendous x_x
/// <summary>
/// Very simple class that handles projectiles.
/// </summary>
public class Projectile : MonoBehaviour
{
    public Vector2 velocity;
    public int damage;
    public int pierce;
    public float radius;
    public float distanceLife;

    protected List<Enemy> enemiesPierced;
    private Vector2 spawnPosition;

    private void Start()
    {
        enemiesPierced = new List<Enemy>();
        spawnPosition = transform.position;
    }

    private void Update()
    {
        // Apply travel this frame.
        transform.position += (Vector3)velocity * Time.deltaTime;
        // Check to see if this projectile has out lived its life.
        if (Vector2.Distance(transform.position, spawnPosition) > distanceLife)
            Destroy(gameObject);
        else
        {
            // Check for enemies to hit this frame.
            foreach (Enemy enemy in Enemy.AllEnemies)
            {
                if (Vector2.Distance(enemy.transform.position, transform.position) < radius)
                {
                    if (!enemiesPierced.Contains(enemy))
                    {
                        enemiesPierced.Add(enemy);
                        enemy.Hit(damage);
                        if (enemiesPierced.Count > pierce)
                        {
                            OnEnemyHit();
                            break;
                        }
                    }
                }
            }
        }
    }

    protected virtual void OnEnemyHit()
    {
        Destroy(gameObject);
    }
}
