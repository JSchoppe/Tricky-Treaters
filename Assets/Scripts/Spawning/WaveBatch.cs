using System;
using System.Collections;
using UnityEngine;

// TODO Add tunneling pathing type.
/// <summary>
/// Represents the mode of transit for enemies in this wave.
/// </summary>
public enum EnemyTraversalType : byte
{
    Walking, Flying
}

/// <summary>
/// Represents a wave of enemies that spawn over time.
/// </summary>
public sealed class WaveBatch : MonoBehaviour
{
    #region Events
    /// <summary>
    /// Called once all enemies in this wave have been defeated.
    /// </summary>
    public event Action<WaveBatch> BatchComplete;
    /// <summary>
    /// Called every time a new path is calculated for this enemy batch.
    /// </summary>
    public event Action<Vector2Int[]> BatchPathRecalculated;
    #endregion
    #region Inspector Fields
    [Tooltip("The enemy that will be instantiated.")]
    [SerializeField] private GameObject enemyPrefab = null;
    [Tooltip("The wave manager for this batch.")]
    [SerializeField] private WaveManager waveManager = null;
    [Header("Wave Parameters")]
    [Tooltip("Pathing technique for these enemies.")]
    [SerializeField] private EnemyTraversalType traversalType = EnemyTraversalType.Walking;
    [Tooltip("The start and end of the enemy path.")]
    [SerializeField] private EnemyPath path = null;
    [Tooltip("The total batch duration in seconds.")]
    [SerializeField] private float waveDuration = 10f;
    [Tooltip("The total number of enemies in this batch.")]
    [SerializeField] private int waveCount = 10;
    #endregion
    #region Inspector Functions
    public void OnValidate()
    {
        // Clamp values to prevent errors.
        path.start.x = Mathf.Clamp(path.start.x, 0, int.MaxValue);
        path.start.y = Mathf.Clamp(path.start.y, 0, int.MaxValue);
        path.end.x = Mathf.Clamp(path.end.x, 0, int.MaxValue);
        path.end.y = Mathf.Clamp(path.end.y, 0, int.MaxValue);
        waveDuration = Mathf.Clamp(waveDuration, 0f, float.MaxValue);
        waveCount = Mathf.Clamp(waveCount, 1, int.MaxValue);
        // If the references have been defined,
        // generate a preview for the enemy path.
        if (waveManager != null && waveManager.Grid != null)
        {
            if (waveManager.Grid.TryFindRoute(Path.start, Path.end, out Vector2Int[] path))
                calculatedPath = path;
            else
                Debug.Log("This wave batch has no suitable path by default!");
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (calculatedPath != null)
        {
            // Draw the preview path.
            Gizmos.color = Color.green;
            for (int i = 1; i < calculatedPath.Length; i++)
            {
                Gizmos.DrawLine(waveManager.Grid.OriginTile + new Vector2(calculatedPath[i - 1].x, calculatedPath[i - 1].y) * waveManager.Grid.GridUnit,
                    waveManager.Grid.OriginTile + new Vector2(calculatedPath[i].x, calculatedPath[i].y) * waveManager.Grid.GridUnit);
            }
        }
    }
    #endregion

    private Vector2Int[] calculatedPath;


    public EnemyTraversalType TraversalType { get { return traversalType; } }


    /// <summary>
    /// The path that is associated with this wave.
    /// </summary>
    public EnemyPath Path { get { return path; } }


    public void BeginWave()
    {
        waveManager.Grid.GridUpdated += OnGridUpdated;
        Calculate();
        enemiesDefeated = 0;
        StartCoroutine(SpawnEnemies(waveDuration / waveCount, waveCount));
    }

    private int enemiesDefeated;

    private IEnumerator SpawnEnemies(float interval, int enemies)
    {
        for (int i = 0; i < enemies; i++)
        {
            yield return new WaitForSeconds(interval);
            Enemy newEnemy = Instantiate(enemyPrefab).GetComponent<Enemy>();
            newEnemy.grid = waveManager.Grid;
            newEnemy.Path = calculatedPath;
            newEnemy.parentBatch = this;
            newEnemy.StartRoute();
            newEnemy.Defeated += OnEnemyDefeated;
            newEnemy.ReachedDestination += OnEnemyReachedDestination;
        }
    }

    private void OnEnemyDefeated()
    {
        enemiesDefeated++;
        if (enemiesDefeated >= waveCount)
        {
            waveManager.Grid.GridUpdated -= OnGridUpdated;
            BatchComplete?.Invoke(this);
        }
    }
    private void OnEnemyReachedDestination()
    {
        waveManager.PlayerState.Lives--;
    }

    // Calculates a default best path for
    // the enemies moving along the path.
    private void Calculate()
    {
        if (waveManager.Grid.TryFindRoute(Path.start, Path.end, out Vector2Int[] path))
            calculatedPath = path;
        else
            Debug.LogError("No path could be found for enemies!");
    }
    private void OnGridUpdated()
    {
        Calculate();
        BatchPathRecalculated?.Invoke(calculatedPath);
    }
}
