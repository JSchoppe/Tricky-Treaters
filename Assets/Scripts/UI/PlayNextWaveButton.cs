using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayNextWaveButton : MonoBehaviour
{
    [SerializeField] private WaveManager waveManager = null;
    [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Color activeNormalColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color activeHoverColor = new Color(0.7f, 1f, 0.7f, 1f);
    [SerializeField] private SpriteRenderer playButton = null;

    private bool waveIsActive;

    private void OnMouseEnter()
    {
        if (!waveIsActive)
            playButton.color = activeHoverColor;
    }
    private void OnMouseExit()
    {
        if (waveIsActive)
            playButton.color = inactiveColor;
        else
            playButton.color = activeNormalColor;
    }
    private void OnMouseDown()
    {
        if (!waveIsActive)
        {
            waveManager.TriggerWave();
            waveIsActive = true;
        }    
    }

    private void Start()
    {
        waveManager.WaveComplete += OnWaveComplete;
        OnWaveComplete();
    }
    private void OnWaveComplete()
    {
        waveIsActive = false;
        playButton.color = activeNormalColor;
    }
}
