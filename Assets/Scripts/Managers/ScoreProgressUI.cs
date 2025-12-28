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

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= UpdateBar;
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
        // Sınıf adı LevelManager (eski LevelManagers değil)
        lm = LevelManager.Instance;
        while (lm == null)
        {
            lm = LevelManager.Instance;
            yield return null;
        }

        // 4. Endless Mode kontrolü - LevelManager üzerinden
        if (lm.IsEndless)
        {
            // İstersen burada levelNameText'e "Endless" yazdırıp barı kapatabilirsin
            // Ama kullanıcı "Endless modda bu bar olmayacak" dediği için komple gizliyoruz
            if (levelNameText != null) levelNameText.text = lm.CurrentLevelName;
            
            // Eğer barı gizleyip sadece ismi göstermek istersen:
            if (fillImage.transform.parent != null) 
                 fillImage.transform.parent.gameObject.SetActive(false); // Bar container gizle
            else
                 fillImage.gameObject.SetActive(false);
                 
            // Ama basitçe tüm objeyi kapatmak isteniyorsa:
            // gameObject.SetActive(false);
            // yield break;
            
            // Biz şimdilik sadece bar kısmını etkisiz hale getirelim ama objeyi açık tutalım ki isim yazsın
        }
        else
        {
            // Normal Mod: İsim yaz
            if (levelNameText != null) 
                levelNameText.text = lm.CurrentLevelName;
        }

        // Event bağla
        ScoreManager.Instance.OnScoreChanged -= UpdateBar;
        ScoreManager.Instance.OnScoreChanged += UpdateBar;

        currentFill = 0f;
        if (fillImage != null) fillImage.fillAmount = 0f;

        UpdateBar(ScoreManager.Instance.Score);
    }

    private void UpdateBar(int score)
    {
        if (lm == null) lm = LevelManager.Instance;
        
        // Endless modda bar güncelleme yok
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
