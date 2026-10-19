using UnityEngine;

// Gentle horizontal sway for a UI layer. Front layers move more than back layers.
public class ParallaxSway : MonoBehaviour
{
    public float amplitude = 10f;
    public float speed = 0.25f;
    public float phase = 0f;

    RectTransform rt;
    Vector2 start;

    void Awake() { rt = (RectTransform)transform; start = rt.anchoredPosition; }

    void Update()
    {
        float x = Mathf.Sin(Time.unscaledTime * speed + phase) * amplitude;
        rt.anchoredPosition = start + new Vector2(x, 0f);
    }
}
