using UnityEngine;
using UnityEngine.SceneManagement;

public class TopBarUI : MonoBehaviour
{
    public void Restart()
    {
        // Eğer LevelManagers varsa onun üzerinden restart en doğru yöntem
        if (LevelManagers.Instance != null)
        {
            LevelManagers.Instance.RestartLevel();
            return;
        }

        // Fallback: sahneyi reload
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoMainMenu()
    {
        // LevelManagers varsa onun içindeki GoMenu'yu kullan
        if (LevelManagers.Instance != null)
        {
            LevelManagers.Instance.GoMenu();
            return;
        }

        // Fallback: direkt menü sahnesi
        SceneManager.LoadScene("MainMenu");
    }
}
