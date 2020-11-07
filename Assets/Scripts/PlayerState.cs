using System;
using UnityEngine;

/// <summary>
/// Holds gameplay state for a player.
/// </summary>
public sealed class PlayerState : MonoBehaviour
{
    #region State Change Events
    public event Action<int> MoneyChanged;
    public event Action<int> LivesChanged;
    #endregion
    #region Inspector Fields
    [Header("Initial Stage Values")]
    [SerializeField] private int money = 100;
    [SerializeField] private int lives = 5;
    private void OnValidate()
    {
        money = Mathf.Clamp(money, 0, int.MaxValue);
        lives = Mathf.Clamp(lives, 1, int.MaxValue);
    }
    #endregion
    #region State Properties
    /// <summary>
    /// The amount of money this player currently has.
    /// </summary>
    public int Money
    {
        get { return money; }
        set
        {
            money = value;
            MoneyChanged?.Invoke(money);
        }
    }
    /// <summary>
    /// The amount of lives this player currently has.
    /// </summary>
    public int Lives
    {
        get { return lives; }
        set
        {
            lives = value;
            LivesChanged?.Invoke(lives);
        }
    }
    #endregion
}
