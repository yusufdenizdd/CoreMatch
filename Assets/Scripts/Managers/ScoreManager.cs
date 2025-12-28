using System.Collections;
using TMPro;
using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private TMP_Text scoreText;
    private MatchablePool _pool;
    private MatchableGrid _grid;

    [SerializeField] private Transform collectionPoint;

    private int _score;
    public event Action<int> OnScoreChanged;

    public int Score => _score;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        scoreText = GetComponent<TMP_Text>();
        if (scoreText == null)
            Debug.LogError("ScoreManager aynı objede TMP_Text bulamadı. ScoreManager'ı TMP Text objesine eklediğinden emin ol.");
    }

    private void Start()
    {
        _grid = (MatchableGrid)MatchableGrid.Instance;
        _pool = (MatchablePool)MatchablePool.Instance;
    }

    public void ResetScore()
    {
        _score = 0;
        if (scoreText) scoreText.text = "Score: " + _score;
        OnScoreChanged?.Invoke(_score);
    }

    public void AddScore(int amount)
    {
        _score += amount;
        if (scoreText) scoreText.text = "Score: " + _score;
        OnScoreChanged?.Invoke(_score);
    }

    public IEnumerator ResolveMatch(Match toResolve, MatchType powerupUsed = MatchType.invalid)
    {
        Matchable matchable;
        Matchable powerupFormed = null;

        Transform target = collectionPoint;

        if (powerupUsed == MatchType.invalid && toResolve.Count > 3)
        {
            powerupFormed = _pool.UpgradeMatchable(toResolve.ToBeUpgraded, toResolve.GetMatchType);
            toResolve.RemoveMatchable(powerupFormed);

            target = powerupFormed.transform;
            powerupFormed.SortingOrder = 3;
        }

        for (int i = 0; i < toResolve.Count; i++)
        {
            matchable = toResolve.Matchables[i];

            if (powerupUsed != MatchType.match5 && matchable.IsGem)
                continue;

            _grid.RemoveItemAt(matchable.position);

            if (i == toResolve.Count - 1)
                yield return StartCoroutine(matchable.Resolve(target));
            else
                StartCoroutine(matchable.Resolve(target));
        }

        AddScore(toResolve.Count * toResolve.Count);

        if (powerupFormed != null)
            powerupFormed.SortingOrder = 3;

        yield return null;
    }
}
