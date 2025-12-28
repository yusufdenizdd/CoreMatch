using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject modeSelectPanel;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "SampleScene";

    private void Start()
    {
        ShowStart();
        //music
        if (AudioMixer.Instance != null)
            AudioMixer.Instance.PlayMusic();

    }

    public void OnStartClicked()
    {
        startPanel.SetActive(false);
        modeSelectPanel.SetActive(true);
    }

    public void OnBackClicked()
    {
        ShowStart();
    }

    private void ShowStart()
    {
        startPanel.SetActive(true);
        modeSelectPanel.SetActive(false);
    }

    // Level 1..5
    public void PlayLevel(int levelNumber)
    {
        GameSession.HasSelection = true;
        GameSession.IsEndless = false;
        GameSession.StartLevelIndex = Mathf.Clamp(levelNumber - 1, 0, 4);
        SceneManager.LoadScene(gameSceneName);
    }


    public void PlayEndless()
    {
        GameSession.HasSelection = true;
        GameSession.IsEndless = true;
        GameSession.StartLevelIndex = 0; // önemli: 0 ver
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }


    public void Quit()
    {
        Debug.Log("Quitting...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
