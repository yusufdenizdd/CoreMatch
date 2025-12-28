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

    // ✅ Endless
    private bool _isEndless;
    public bool IsEndless => _isEndless;

    public bool IsPlaying => _isPlaying;
    public int RemainingMoves => _remainingMoves;
    public int CurrentLevel => _levelIndex + 1;

    // Level hedeflerini UI için dışarı ver
    public int CurrentTargetScore => (levels != null && levels.Length > 0)
        ? levels[_levelIndex].targetScore
        : 100;

    public int CurrentTargetMoves => (levels != null && levels.Length > 0)
        ? levels[_levelIndex].moves
        : 0;

    public event Action<int, int> OnMovesChanged; // (remainingMoves, totalMoves)

    protected override void Init() { }

    private void Start()
    {
        // ✅ Bu script sadece oyun sahnesinde çalışsın
        if (SceneManager.GetActiveScene().name != gameSceneName)
            return;

        // Score event
        var score = ScoreManager.Instance;
        if (score != null)
            score.OnScoreChanged += HandleScoreChanged;

        // ✅ Seçim kaynağı: GameSession -> yoksa PlayerPrefs
        if (GameSession.HasSelection)
        {
            _isEndless = GameSession.IsEndless;

            if (!_isEndless)
                _levelIndex = Mathf.Clamp(GameSession.StartLevelIndex, 0, levels.Length - 1);
            else
                _levelIndex = 0; // endless'te index önemli değil (istersen 0 kalsın)

            GameSession.HasSelection = false; // bir kere kullan
        }
        else
        {
            _isEndless = false;
            _levelIndex = PlayerPrefs.GetInt("LEVEL_INDEX", 0);
            _levelIndex = Mathf.Clamp(_levelIndex, 0, levels.Length - 1);
        }

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

    public void StartLevel(int index)
    {
        StartCoroutine(StartLevelRoutine(index));
    }

    private IEnumerator StartLevelRoutine(int index)
    {
        _levelIndex = Mathf.Clamp(index, 0, levels.Length - 1);

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
    }

    public void OnMoveUsed()
    {
        if (!_isPlaying) return;

        // ✅ Endless modda move tüketme / fail yok
        if (_isEndless)
        {
            // İstersen sadece UI update etmek için event gönderebilirsin:
            OnMovesChanged?.Invoke(_remainingMoves, _remainingMoves);
            return;
        }

        Debug.Log($"Move used. Remaining moves: {_remainingMoves} -> {_remainingMoves - 1}");

        _remainingMoves--;
        if (_remainingMoves <= 0)
        {
            _remainingMoves = 0;
            _isPlaying = false;

            int target = levels[_levelIndex].targetScore;
            int currentScore = (ScoreManager.Instance != null) ? ScoreManager.Instance.Score : 0;

            if (currentScore < target)
            {
                if (failPanel) failPanel.SetActive(true);
            }
        }

        OnMovesChanged?.Invoke(_remainingMoves, levels[_levelIndex].moves);
    }

    private void HandleScoreChanged(int newScore)
    {
        if (!_isPlaying) return;

        // ✅ Endless modda win koşulu yok
        if (_isEndless) return;

        int target = levels[_levelIndex].targetScore;
        if (newScore >= target)
        {
            _isPlaying = false;

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
