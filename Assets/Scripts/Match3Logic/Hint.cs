using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class Hint : Singleton<Hint>
{
    private SpriteRenderer _spriteRenderer;

    private Transform _hintLocation;

    private Coroutine _autoHintCR;

    [SerializeField] private float delayBeforeAutoHint;

    [SerializeField] private Button _hintButton;

    protected override void Init()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;
        _hintButton.interactable = false;
    }

    public void PointHint(Transform hintLocation)
    {
        CancelHint();
        transform.position = hintLocation.position;
        _spriteRenderer.enabled = true;


    }

    public void CancelHint()
    {
        _spriteRenderer.enabled = false;
        _hintButton.interactable = false;
        if (_autoHintCR != null)
        {

            StopCoroutine(_autoHintCR);
        }
        _autoHintCR = null;
    }

    public void EnableHintButton()
    {
        _hintButton.interactable = true;

    }

    public void StartAutoHint(Transform hintLocation)
    {
        // Önce eskisi varsa durdur ve temizle
        if (_autoHintCR != null)
        {
            StopCoroutine(_autoHintCR);
        }
        _hintLocation = hintLocation;

        _autoHintCR = StartCoroutine(WaitAndPointHint());
    }
    private IEnumerator WaitAndPointHint()
    {
        yield return new WaitForSeconds(delayBeforeAutoHint);
        PointHint(_hintLocation);
    }
}
