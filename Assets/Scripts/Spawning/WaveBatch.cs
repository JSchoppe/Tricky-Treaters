using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class WaveBatch : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab = null;

    [SerializeField] private WaveManager waveManager = null;

    [SerializeField] private Vector2Int spawn = new Vector2Int(0, 0);

    [SerializeField] private Vector2Int destination = new Vector2Int(5, 5);

    private void OnValidate()
    {
        if (spawn.x < 0)
            spawn.x = -1;
        if (spawn.y < 0)
            spawn.y = 0;
        if (destination.x < 0)
            destination.x = 0;
        if (destination.y < 0)
            destination.y = 0;

        if (waveManager != null)
        {
            if (waveManager.Grid != null)
            {
                if (waveManager.Grid.TryFindRoute(ref spawn, ref destination, out Vector2Int[] path))
                {
                    this.path = path;
                }
                else
                {
                    Debug.Log("No Suitable Path Found.");
                }
            }
        }
    }

    private Vector2Int[] path;
    private void OnDrawGizmos()
    {
        if (path != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 1; i < path.Length; i++)
            {
                Gizmos.DrawLine(waveManager.Grid.OriginTile + new Vector2(path[i - 1].x, path[i - 1].y) * waveManager.Grid.GridUnit,
                    waveManager.Grid.OriginTile + new Vector2(path[i].x, path[i].y) * waveManager.Grid.GridUnit);
            }
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
