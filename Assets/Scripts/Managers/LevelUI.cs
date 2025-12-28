using TMPro;
using UnityEngine;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text levelText;

    private void Update()
    {
        // if (LevelManagers.Instance != null)
        // {
        //     levelText.text = $"Level {LevelManagers.Instance.CurrentLevel} / 5";
        // }
        if (LevelManagers.Instance.IsEndless)
            levelText.text = "Endless Mode";
        else
            levelText.text = $"Level {LevelManagers.Instance.CurrentLevel}";

    }
}
