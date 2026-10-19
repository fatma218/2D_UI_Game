using UnityEngine;

// Draws objects in front of or behind each other depending on their Y position (feet = pivot).
[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    public bool dynamic = true;
    public int offset = 0;
    SpriteRenderer sr;

    void Awake() { sr = GetComponent<SpriteRenderer>(); Apply(); }
    void LateUpdate() { if (dynamic) Apply(); }
    void Apply() { if (sr != null) sr.sortingOrder = -Mathf.RoundToInt(transform.position.y * 20f) + offset; }
}
