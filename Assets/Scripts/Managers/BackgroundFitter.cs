using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFitter : MonoBehaviour
{
    void Start()
    {
        var cam = Camera.main;
        var sr = GetComponent<SpriteRenderer>();

        float screenHeight = cam.orthographicSize * 2f;
        float screenWidth = screenHeight * cam.aspect;

        Vector2 spriteSize = sr.sprite.bounds.size;

        float scale = Mathf.Max(
            screenWidth / spriteSize.x,
            screenHeight / spriteSize.y
        );

        transform.localScale = new Vector3(scale, scale, 1f);
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
    }
}
