// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class LevelManagers : MonoBehaviour
// {
//     public void LoadGame()
//     {
//         SceneManager.LoadScene("SampleScene");
//     }

//     public void QuitGame()
//     {
//         Debug.Log("Quitting Game...");
//         Application.Quit();
//     }
// }


// Muhsina Yaptığı değişiklikler
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[Serializable]
public class LevelConfig
{
    public int targetScore = 100;
    public int moves = 3;
    public Sprite background;
}

public class LevelManagers : Singleton<LevelManagers>
{
    [Header("Levels (size = 5)")]
    [SerializeField] private LevelConfig[] levels = new LevelConfig[5];

    [Header("UI / Scene")]
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private GameObject winPanel;     // opsiyonel
    [SerializeField] private GameObject finalWinPanel;// opsiyonel
    [SerializeField] private UnityEngine.UI.Image backgroundImage; // opsiyonel
    [SerializeField] private GameObject failPanel;

    private int _levelIndex;
    private int _remainingMoves;
    private bool _isPlaying;

    public bool IsPlaying => _isPlaying;
    public int RemainingMoves => _remainingMoves;
    public int CurrentLevel => _levelIndex + 1;

    protected override void Init() { }

    private void Start()
    {
        // Oyun sahnesindeysek otomatik başlat
        if (SceneManager.GetActiveScene().name == gameSceneName)
        {
            var score = ScoreManager.Instance;
            if (score != null)
                score.OnScoreChanged += HandleScoreChanged;

            StartLevel(_levelIndex);
        }
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    public void GoMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // public void StartLevel(int index)
    // {
    //     _levelIndex = Mathf.Clamp(index, 0, levels.Length - 1);
    //     _remainingMoves = Mathf.Max(0, levels[_levelIndex].moves);
    //     _isPlaying = true;

    //     // UI panelleri kapat
    //     if (winPanel) winPanel.SetActive(false);
    //     if (finalWinPanel) finalWinPanel.SetActive(false);

    //     // background uygula
    //     if (backgroundImage && levels[_levelIndex].background)
    //         backgroundImage.sprite = levels[_levelIndex].background;

    //     // skor sıfırla
    //     var score = ScoreManager.Instance;
    //     if (score != null) score.ResetScore();

    //     // İleride: grid reset + yeniden populate burada yapılabilir.
    // }



    // Muhsina Yaptığı değişiklikler
    public void StartLevel(int index)
    {
        StartCoroutine(StartLevelRoutine(index));
    }

    private IEnumerator StartLevelRoutine(int index)
    {
        _levelIndex = Mathf.Clamp(index, 0, levels.Length - 1);
        _remainingMoves = Mathf.Max(0, levels[_levelIndex].moves);
        // _remainingMoves = 5;

        _isPlaying = false;

        if (winPanel) winPanel.SetActive(false);
        if (finalWinPanel) finalWinPanel.SetActive(false);
        if (failPanel) failPanel.SetActive(false);

        if (backgroundImage && levels[_levelIndex].background)
            backgroundImage.sprite = levels[_levelIndex].background;

        // Skoru sıfırla
        var score = ScoreManager.Instance;
        if (score != null) score.ResetScore();

        // Grid reset + yeniden doldur
        var grid = (MatchableGrid)MatchableGrid.Instance;
        if (grid != null)
            yield return StartCoroutine(grid.ResetGrid(false));

        // Artık oynanabilir
        _isPlaying = true;
    }
    //Muhsina yaptığı değişiklikler


    public void OnMoveUsed()
    {
        Debug.Log($"Move used. Remaining moves: {_remainingMoves} -> {_remainingMoves - 1}");

        if (!_isPlaying) return;

        _remainingMoves--;

        if (_remainingMoves <= 0)
        {
            _remainingMoves = 0;
            _isPlaying = false;

            // ✅ Eğer skor hedefe ulaşmadıysa fail
            int target = levels[_levelIndex].targetScore;
            int currentScore = (ScoreManager.Instance != null) ? ScoreManager.Instance.Score : 0;

            if (currentScore < target)
            {
                if (failPanel) failPanel.SetActive(true);
            }
        }
    }

    private void HandleScoreChanged(int newScore)
    {
        if (!_isPlaying) return;

        int target = levels[_levelIndex].targetScore;
        if (newScore >= target)
        {
            _isPlaying = false;

            bool isFinal = (_levelIndex >= levels.Length - 1);
            if (isFinal)
            {
                if (finalWinPanel) finalWinPanel.SetActive(true);
                // veya: SceneManager.LoadScene("WinScene");
            }
            else
            {
                if (winPanel) winPanel.SetActive(true);
                // Next butonu bu fonksiyonu çağırabilir:
                // NextLevel();
            }
        }
    }

    public void NextLevel()
    {
        if (_levelIndex >= levels.Length - 1)
            return;

        _levelIndex++;
        StartLevel(_levelIndex);

        // İleride: grid’i temizle + repopulate
        // MatchableGrid.Instance.Clear(); gibi
    }
}
