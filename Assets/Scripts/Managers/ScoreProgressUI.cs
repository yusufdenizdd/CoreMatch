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

    [Header("Optional")]
    [SerializeField] private string gameSceneName = "SampleScene";

    private float currentFill = 0f;
    private int maxScore = 100;

    private Coroutine bindRoutine;
    private Coroutine animRoutine;

    private LevelManagers lm;

    private void OnEnable()
    {
        bindRoutine = StartCoroutine(BindWhenReady());
    }

    private void OnDisable()
    {
        if (bindRoutine != null) StopCoroutine(bindRoutine);
        if (animRoutine != null) StopCoroutine(animRoutine);

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= UpdateBar;
    }

    private IEnumerator BindWhenReady()
    {
        // oyun sahnesinde değilsek gizle
        if (SceneManager.GetActiveScene().name != gameSceneName)
        {
            gameObject.SetActive(false);
            yield break;
        }

        // ✅ Endless modda gizle
        if (GameSession.IsEndless)
        {
            gameObject.SetActive(false);
            yield break;
        }

        while (ScoreManager.Instance == null)
            yield return null;

        lm = FindFirstObjectByType<LevelManagers>();
        if (lm == null)
        {
            gameObject.SetActive(false);
            yield break;
        }

        ScoreManager.Instance.OnScoreChanged -= UpdateBar;
        ScoreManager.Instance.OnScoreChanged += UpdateBar;

        currentFill = 0f;
        if (fillImage != null) fillImage.fillAmount = 0f;

        UpdateBar(ScoreManager.Instance.Score);
    }

    private void UpdateBar(int score)
    {
        if (lm == null) lm = FindFirstObjectByType<LevelManagers>();
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
