using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScoreManager : Singleton<ScoreManager>
{
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI scoreText;
    private MatchablePool _pool;
    private MatchableGrid _grid;
    private AudioMixer _audioMixer;
    public event Action<int> OnScoreChanged;
    [SerializeField] private Transform collectionPoint;
    private int _score;

    public int Score
    {
        get
        {
            return _score;
        }
    }

    private float _timeSinceLastScore;
    [SerializeField] private float maxComboTime;
    [SerializeField] private float currentComboTime;
    private int _comboMultiplier;

    private bool _timerIsActive;
    /*protected override void Init()
    {
        //scoreText = GetComponent<TMP_Text>();

    }*/

    [SerializeField] private Slider comboSlider;

    private void Start()
    {
        _grid = (MatchableGrid)MatchableGrid.Instance;
        _pool = (MatchablePool)MatchablePool.Instance;
        _audioMixer = AudioMixer.Instance;

        comboText.enabled = false;
        comboSlider.gameObject.SetActive(false);

    }
    public void ResetScore()
    {
        _score = 0;
        if (scoreText) scoreText.text = "Score: " + _score;
        OnScoreChanged?.Invoke(_score);
    }
    public void AddScore(int amount)
    {
        _score += amount * IncreaseCombo();
        scoreText.text = "Score: " + _score;
        _timeSinceLastScore = 0;
        if (!_timerIsActive)
        {
            StartCoroutine(ComboTimer());
        }
        _audioMixer.PlaySound(SoundEffects.score);

        OnScoreChanged?.Invoke(_score);

    }
    private IEnumerator ComboTimer()
    {
        _timerIsActive = true;
        comboText.enabled = true;
        comboSlider.gameObject.SetActive(true);

        do
        {
            _timeSinceLastScore += Time.deltaTime;
            comboSlider.value = 1 - (_timeSinceLastScore / maxComboTime);

            yield return null;
        } while (_timeSinceLastScore < currentComboTime);
        _comboMultiplier = 0;
        comboText.enabled = false;
        comboSlider.gameObject.SetActive(false);
        _timerIsActive = false;

    }
    private int IncreaseCombo()
    {
        comboText.text = "Combo x" + ++_comboMultiplier;
        currentComboTime = maxComboTime - Mathf.Log(_comboMultiplier) / 2;
        return _comboMultiplier;

    }
    public IEnumerator ResolveMatch(Match toResolve, MatchType powerupUsed = MatchType.invalid)
    {
        Matchable matchable;
        Matchable powerupFormed = null;

        Transform target = collectionPoint;

        //powerup (zaten bir powerup sonucu resolve ediyorsak tekrar powerup oluşturmasın)
        if (powerupUsed == MatchType.invalid && toResolve.Count > 3)
        {

            powerupFormed = _pool.UpgradeMatchable(toResolve.ToBeUpgraded, toResolve.GetMatchType);
            toResolve.RemoveMatchable(powerupFormed);

            target = powerupFormed.transform;

            powerupFormed.SortingOrder = 3;
            _audioMixer.PlaySound(SoundEffects.upgrade);
        }
        else
        {
            _audioMixer.PlaySound(SoundEffects.resolve);
        }


        for (int i = 0; i < toResolve.Count; i++)
        {
            matchable = toResolve.Matchables[i];

            //match5 powerup'ı mı kontrol et, match5 powerup ise resolve veya remove yapma
            if (powerupUsed != MatchType.match5 && matchable.IsGem)
            {
                continue;
            }



            // remove the matchables from the grid
            _grid.RemoveItemAt(matchable.position);

            //move them off to the side of the screen
            if (i == toResolve.Count - 1)
            {
                yield return StartCoroutine(matchable.Resolve(target));
            }
            else
            {
                StartCoroutine(matchable.Resolve(target));
            }



        }

        //update the player's score
        AddScore(toResolve.Count * toResolve.Count);

        if (powerupFormed != null)
        {
            powerupFormed.SortingOrder = 3;
        }



        yield return null;
    }


}

