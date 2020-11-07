using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Represents an enemy pathway on a grid.
/// </summary>
[Serializable]
public sealed class EnemyPath
{
    public Vector2Int start;
    public Vector2Int end;
}

public sealed class WaveManager : MonoBehaviour
{
    public event Action AllWavesComplete;
    public event Action WaveComplete;

    [SerializeField] private NodeGrid grid = null;
    [SerializeField] private PlayerState playerState = null;
    
    public PlayerState PlayerState { get { return playerState; } }

    public NodeGrid Grid { get { return grid; } }


    public void OnValidateWaves()
    {
        foreach (WaveBatch batch in transform.GetComponentsInChildren<WaveBatch>())
            batch.OnValidate();
    }

    public EnemyPath[] GetAllEnemyPaths()
    {
        List<EnemyPath> enemyPaths = new List<EnemyPath>();
        foreach (WaveBatch batch in transform.GetComponentsInChildren<WaveBatch>())
            if (!enemyPaths.Contains(batch.Path))
                enemyPaths.Add(batch.Path);
        return enemyPaths.ToArray();
    }

    private List<Dictionary<WaveBatch, bool>> waveStates;
    private int currentWave;

    private void Start()
    {
        // Populate the waves for this stage based on the inspector hierarchy.
        waveStates = new List<Dictionary<WaveBatch, bool>>();
        foreach (Transform child in transform)
        {
            Dictionary<WaveBatch, bool> wave = new Dictionary<WaveBatch, bool>();
            foreach (WaveBatch batch in child.GetComponents<WaveBatch>())
            {
                wave.Add(batch, false);
                batch.BatchComplete += OnBatchComplete;
            }
            waveStates.Add(wave);
        }
        // Initiate state for the first wave.
        currentWave = -1;
    }

    public void TriggerWave()
    {
        currentWave++;
        // TODO this should not be hard coded here :'(
        switch (currentWave)
        {
            case 1:
                AudioSingleton.PlayBackgroundMusic(BackgroundTrack.Synth);
                break;
            case 2:
                AudioSingleton.PlayBackgroundMusic(BackgroundTrack.Rock);
                break;
        }

        foreach (WaveBatch batch in waveStates[currentWave].Keys)
            batch.BeginWave();
    }

    private void OnBatchComplete(WaveBatch batch)
    {
        waveStates[currentWave][batch] = true;
        bool isWaveComplete = true;
        foreach (bool isBatchComplete in waveStates[currentWave].Values)
        {
            if (!isBatchComplete)
            {
                isWaveComplete = false;
                break;
            }
        }
        if (isWaveComplete)
        {
            if (currentWave == waveStates.Count - 1)
            {
                AllWavesComplete?.Invoke();
                // TODO this should not be done here!!!
                SceneManager.LoadScene("Menu");
            }
            else
                WaveComplete?.Invoke();
        }
    }
}
