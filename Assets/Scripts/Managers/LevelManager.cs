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

public class LevelManager : Singleton<LevelManager>
{
    [Header("Levels (size = 5)")]
    [SerializeField]
    private LevelConfig[] levels = new LevelConfig[]
    {
        new LevelConfig { targetScore = 500, moves = 15 },
        new LevelConfig { targetScore = 1500, moves = 20 },
        new LevelConfig { targetScore = 2000, moves = 25 },
        new LevelConfig { targetScore = 3000, moves = 30 },
        new LevelConfig { targetScore = 5000, moves = 35 }
    };

    [Header("UI / Scene")]
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private GameObject winPanel;     // opsiyonel
    [SerializeField] private GameObject finalWinPanel;// opsiyonel
    [SerializeField] private UnityEngine.UI.Image backgroundImage; // opsiyonel
    [SerializeField] private GameObject failPanel;

    private int _levelIndex;
    private int _remainingMoves;
    private bool _isPlaying;

    // ✅ Endless
    private bool _isEndless;
    public bool IsEndless => _isEndless;

    public bool IsPlaying => _isPlaying;
    public int RemainingMoves => _remainingMoves;
    public int CurrentLevel => _levelIndex + 1;
    
    public string CurrentLevelName
    {
        get
        {
            if (_isEndless) return "Endless Mode";
            return "Level " + (_levelIndex + 1);
        }
    }

    // Level hedeflerini UI için dışarı ver
    public int CurrentTargetScore => (levels != null && levels.Length > 0)
        ? levels[_levelIndex].targetScore
        : 100;

    public int CurrentTargetMoves => (levels != null && levels.Length > 0)
        ? levels[_levelIndex].moves
        : 0;

    public event Action<int, int> OnMovesChanged; // (remainingMoves, totalMoves)

    protected override void Init()
    {
        base.Init();

        // 1. Data Setup (Awake is safe for Prefs/Statics)
        // ✅ Seçim kaynağı: GameSession -> yoksa PlayerPrefs
        // Used indirect check to strictly avoid Singleton.Instance access which throws error if missing
        var session = FindFirstObjectByType<GameSession>();
        if (session != null && GameSession.HasSelection)
        {
            _isEndless = GameSession.IsEndless;

            if (!_isEndless)
                _levelIndex = Mathf.Clamp(GameSession.StartLevelIndex, 0, levels.Length - 1);
            else
                _levelIndex = 0;

            GameSession.HasSelection = false;
        }
        else
        {
            _isEndless = false;
            _levelIndex = PlayerPrefs.GetInt("LEVEL_INDEX", 0);
            _levelIndex = Mathf.Clamp(_levelIndex, 0, levels.Length - 1);
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != gameSceneName)
            return;

        // 2. Dependency Setup (Events)
        var score = ScoreManager.Instance;
        if (score != null)
            score.OnScoreChanged += HandleScoreChanged;

        // Note: StartLevel is now called by GameManager via BeginGame()
    }

    public void BeginGame()
    {
        StartLevel(_levelIndex);
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void GoMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartLevel()
    {
        StartLevel(_levelIndex);
    }

    public void ResetProgress()
    {
        _levelIndex = 0;
        PlayerPrefs.SetInt("LEVEL_INDEX", 0);
        PlayerPrefs.Save();
    }

    public event Action OnLevelStarted;

    public void StartLevel(int index)
    {
        StartCoroutine(StartLevelRoutine(index));
    }

    private IEnumerator StartLevelRoutine(int index)
    {
        _levelIndex = Mathf.Clamp(index, 0, levels.Length - 1);
        
        // Notify UI immediately about the change
        OnLevelStarted?.Invoke();

        // Önce input kilitle
        _isPlaying = false;

        // UI panellerini kapat
        if (winPanel) winPanel.SetActive(false);
        if (finalWinPanel) finalWinPanel.SetActive(false);
        if (failPanel) failPanel.SetActive(false);

        // Background
        if (backgroundImage && levels[_levelIndex].background)
            backgroundImage.sprite = levels[_levelIndex].background;

        // Skor sıfırla
        var score = ScoreManager.Instance;
        if (score != null) score.ResetScore();

        // ✅ Moves ayarla (endless ise çok büyük)
        if (_isEndless)
        {
            _remainingMoves = int.MaxValue / 4; // pratikte sınırsız
            OnMovesChanged?.Invoke(_remainingMoves, _remainingMoves);
        }
        else
        {
            _remainingMoves = Mathf.Max(0, levels[_levelIndex].moves);
            OnMovesChanged?.Invoke(_remainingMoves, levels[_levelIndex].moves);
        }

        // Grid reset + yeniden doldur
        var grid = (MatchableGrid)MatchableGrid.Instance;
        if (grid != null)
            yield return StartCoroutine(grid.ResetGrid(false));

        // Artık oynanabilir
        _isPlaying = true;

        // Re-enable cursor since GameManager Setup disabled it and we broke out early
        if (Cursor.Instance != null) Cursor.Instance.enabled = true;
    }

    public void OnMoveUsed()
    {
        if (!_isPlaying) return;

        // ✅ Endless modda move tüketme / fail yok
        if (_isEndless)
        {
            OnMovesChanged?.Invoke(_remainingMoves, _remainingMoves);
            return;
        }

        Debug.Log($"Move used. Remaining moves: {_remainingMoves} -> {_remainingMoves - 1}");

        _remainingMoves--;

        // Signal UI
        OnMovesChanged?.Invoke(_remainingMoves, levels[_levelIndex].moves);

        if (_remainingMoves <= 0)
        {
            _remainingMoves = 0;
            _isPlaying = false; // Stop input immediately

            // We DO NOT show FailPanel here yet. We wait for OnBoardSettled.
        }
    }

    public void OnBoardSettled()
    {
        // Called when grid is idle (no matches occurring)

        // If we ran out of moves AND didn't win yet -> FAIL
        if (!_isPlaying && _remainingMoves == 0)
        {
            // Check score one last time? 
            // HandleScoreChanged would have triggered WIN if we had enough score.
            // So if we are here, and not Won (we can check active panels or state), then Fail.

            // Check if Win detected?
            bool won = (winPanel != null && winPanel.activeSelf) || (finalWinPanel != null && finalWinPanel.activeSelf);
            if (!won)
            {
                int target = levels[_levelIndex].targetScore;
                int currentScore = (ScoreManager.Instance != null) ? ScoreManager.Instance.Score : 0;

                // Double check score just in case
                if (currentScore < target && failPanel)
                {
                    failPanel.SetActive(true);
                }
                else if (currentScore >= target)
                {
                    // Should have been handled by HandleScoreChanged, but force it if missed
                    HandleScoreChanged(currentScore);
                }
            }
        }
    }

    private void HandleScoreChanged(int newScore)
    {
        // Don't check if (!_isPlaying) because we might have run out of moves 
        // but the final cascade pushed us over the score target.
        if (_isEndless) return;

        // Prevent multiple triggers if already showing a panel
        if ((winPanel != null && winPanel.activeSelf) || (finalWinPanel != null && finalWinPanel.activeSelf))
            return;

        int target = levels[_levelIndex].targetScore;
        if (newScore >= target)
        {
            // Win!
            _isPlaying = false;

            if (failPanel) failPanel.SetActive(false); // Hide fail if it accidentally showed

            bool isFinal = (_levelIndex >= levels.Length - 1);
            if (isFinal)
            {
                if (finalWinPanel) finalWinPanel.SetActive(true);
            }
            else
            {
                if (winPanel) winPanel.SetActive(true);
            }
        }
    }

    public void NextLevel()
    {
        // ✅ Endless modda next level yok (istersen menüye döndürürsün)
        if (_isEndless) return;

        if (_levelIndex >= levels.Length - 1)
            return;

        _levelIndex++;
        StartLevel(_levelIndex);

        PlayerPrefs.SetInt("LEVEL_INDEX", _levelIndex);
        PlayerPrefs.Save();
    }
}
