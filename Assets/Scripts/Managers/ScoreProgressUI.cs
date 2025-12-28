using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreProgressUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text levelNameText; // Level adı için referans
    [SerializeField] private TMP_Text movesText; // Hamle sayısı referansı

    [Header("Optional")]
    [SerializeField] private string gameSceneName = "SampleScene";

    private float currentFill = 0f;
    private int maxScore = 100;

    private Coroutine bindRoutine;
    private Coroutine animRoutine;

    private LevelManager lm;

    private void OnEnable()
    {
        bindRoutine = StartCoroutine(BindWhenReady());
    }

    private void OnDisable()
    {
        if (bindRoutine != null) StopCoroutine(bindRoutine);
        if (animRoutine != null) StopCoroutine(animRoutine);

        if (ScoreManager.HasInstance)
            ScoreManager.Instance.OnScoreChanged -= UpdateBar;
            
        if (lm != null)
            lm.OnMovesChanged -= UpdateMoves;
    }

    private IEnumerator BindWhenReady()
    {
        // 1. Oyun sahnesinde değilsek gizle
        if (SceneManager.GetActiveScene().name != gameSceneName)
        {
            gameObject.SetActive(false);
            yield break;
        }

        // 2. ScoreManager bekle
        while (ScoreManager.Instance == null)
            yield return null;

        // 3. LevelManager bekle ve referans al
        lm = LevelManager.Instance;
        while (lm == null)
        {
            lm = LevelManager.Instance;
            yield return null;
        }

        // 4. Endless Mode kontrolü
        if (lm.IsEndless)
        {
            if (levelNameText != null) levelNameText.text = lm.CurrentLevelName;
            
            // Moves Endless'da gizlensin mi? "∞" yazabiliriz.
            if (movesText != null) movesText.text = ""; 

            // Barı gizle
            if (fillImage != null)
            {
                if (fillImage.transform.parent != null) 
                     fillImage.transform.parent.gameObject.SetActive(false);
                else
                     fillImage.gameObject.SetActive(false);
            }
        }
        else
        {
            if (levelNameText != null) 
                levelNameText.text = lm.CurrentLevelName;
                
            // İlk hamle sayısını yaz
            UpdateMoves(lm.RemainingMoves, lm.CurrentTargetMoves);
        }

        // Event bağla
        ScoreManager.Instance.OnScoreChanged -= UpdateBar;
        ScoreManager.Instance.OnScoreChanged += UpdateBar;
        
        lm.OnMovesChanged -= UpdateMoves;
        lm.OnMovesChanged += UpdateMoves;

        currentFill = 0f;
        if (fillImage != null) fillImage.fillAmount = 0f;

        UpdateBar(ScoreManager.Instance.Score);
    }

    private void UpdateMoves(int remaining, int total)
    {
        if (movesText != null && !lm.IsEndless)
        {
            movesText.text = "Moves: " + remaining;
        }
    }

    private void UpdateBar(int score)
    {
        if (lm == null) lm = LevelManager.Instance;
        
        if (lm != null && lm.IsEndless) return;

        if (lm != null)
            maxScore = Mathf.Max(1, lm.CurrentTargetScore);

        float targetFill = Mathf.Clamp01((float)score / maxScore);

        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(AnimateFill(targetFill));

        if (scoreText != null)
            scoreText.text = $"{score} / {maxScore}";
    }

    private IEnumerator AnimateFill(float target)
    {
        while (Mathf.Abs(currentFill - target) > 0.001f)
        {
            currentFill = Mathf.Lerp(currentFill, target, Time.deltaTime * 6f);
            if (fillImage != null)
                fillImage.fillAmount = currentFill;
            yield return null;
        }

        currentFill = target;
        if (fillImage != null)
            fillImage.fillAmount = currentFill;
    }
}
