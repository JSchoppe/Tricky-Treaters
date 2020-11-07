using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GlobalsPostInit : MonoBehaviour
{
    // TODO this class is a hotfix for scene management.
    private void LateUpdate()
    {
        // Advance to the main menu after
        // all globals have initialized in Start.
        SceneManager.LoadScene("Menu");
    }
}
