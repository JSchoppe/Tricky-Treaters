using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class LivesCounter : MonoBehaviour
{
    #region Inspector Fields
    [Tooltip("The UI text element that is written to.")]
    [SerializeField] private Text livesText = null;
    [Tooltip("The player whose money will be observed.")]
    [SerializeField] private PlayerState linkedPlayer = null;
    #endregion
    #region Value Initialization + Binding
    private void Start()
    {
        linkedPlayer.LivesChanged += UpdateText;
        UpdateText(linkedPlayer.Lives);
    }
    private void UpdateText(int newValue)
    {
        livesText.text = $"{linkedPlayer.Lives}";
    }
    #endregion
}
