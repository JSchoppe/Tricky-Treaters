using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Binds the money counter to the player data.
/// </summary>
public sealed class MoneyCounter : MonoBehaviour
{
    #region Inspector Fields
    [Tooltip("Text that is written before the value.")]
    [SerializeField] private string prefix = "$";
    [Tooltip("The UI text element that is written to.")]
    [SerializeField] private Text moneyText = null;
    [Tooltip("The player whose money will be observed.")]
    [SerializeField] private PlayerState linkedPlayer = null;
    #endregion
    #region Value Initialization + Binding
    private void Start()
    {
        linkedPlayer.MoneyChanged += UpdateText;
        UpdateText(linkedPlayer.Money);
    }
    private void UpdateText(int newValue)
    {
        moneyText.text = $"{prefix}{linkedPlayer.Money}";
    }
    #endregion
}
