using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Fader : MonoBehaviour
{

    private Image _toFade;
    private Color _faded;
    [SerializeField] private float fadeSpeed = 1;

    private void Awake()
    {
        _toFade = GetComponent<Image>();
        _faded = _toFade.color;
    }

    public void Hide(bool hidden)
    {
        _toFade.enabled = !hidden;
    }

    public IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = _faded.a;
        float t = 0;

        do
        {
            t += Time.deltaTime * fadeSpeed;
            if (t > 1)
            {
                t = 1;
            }
            _faded.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            _toFade.color = _faded;
            yield return null;
        } while (t < 1);
    }
}
