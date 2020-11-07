using System.Collections;
using UnityEngine;

public sealed class GusherLauncher : Tower
{
    [SerializeField] private float shootInterval = 1f;
    [SerializeField] private float rangeTiles = 4f;
    [SerializeField] private float shotSpeed = 1f;
    [SerializeField] private int shotDamage = 3;
    [SerializeField] private float shotRadius = .3f;

    [SerializeField] private GameObject projectilePrefab = null;

    private float scaledRange;
    private float scaledShotSpeed;

    private void OnValidate()
    {
        shootInterval = Mathf.Clamp(shootInterval, 0.1f, float.MaxValue);
        rangeTiles = Mathf.Clamp(rangeTiles, 0.5f, float.MaxValue);
        shotSpeed = Mathf.Clamp(shotSpeed, 0.1f, float.MaxValue);
        shotDamage = Mathf.Clamp(shotDamage, 1, int.MaxValue);
        shotRadius = Mathf.Clamp(shotRadius, 0.1f, float.MaxValue);
    }


    public override void OnPlacement(NodeGrid inGrid)
    {
        base.OnPlacement(inGrid);
        scaledRange = grid.GridUnit * rangeTiles;
        scaledShotSpeed = grid.GridUnit * shotSpeed;
        StartCoroutine(FireShot());
    }

    private IEnumerator FireShot()
    {
        while (true)
        {
            if (Enemy.AllEnemies.Count > 0)
            {
                Enemy closestEnemy = null;
                float closestDistance = float.MaxValue;
                foreach (Enemy enemy in Enemy.AllEnemies)
                {
                    float distance = Vector2.Distance(enemy.transform.position, location);
                    if (distance < scaledRange
                        && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemy;
                    }
                }
                if (closestEnemy != null)
                {
                    Vector2 enemyDirection = (closestEnemy.Position - location).normalized;
                    
                    Projectile newProjectile = Instantiate(projectilePrefab).GetComponent<Projectile>();
                    newProjectile.transform.position = transform.position;
                    newProjectile.velocity = enemyDirection * scaledShotSpeed;
                    newProjectile.damage = shotDamage;
                    newProjectile.radius = shotRadius;
                    newProjectile.pierce = 0;
                    newProjectile.distanceLife = scaledRange;
                }
            }
            yield return new WaitForSeconds(shootInterval);
        }
    }
}
