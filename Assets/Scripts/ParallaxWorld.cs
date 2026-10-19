using UnityEngine;

// Keeps the layered forest backdrop behind the playfield and slides each layer at its own speed.
// follow = 1 means the layer stays pinned to the camera (far away). Lower values move more with the world.
public class ParallaxWorld : MonoBehaviour
{
    public Transform cam;
    public Transform[] layers;
    public float[] follow;
    public float tileWorldWidth = 16.2f;
    public float yOffset = 0.5f;

    void LateUpdate()
    {
        if (cam == null) return;
        Vector3 c = cam.position;
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i] == null) continue;
            float f = (follow != null && i < follow.Length) ? follow[i] : 1f;
            float off = Mathf.Repeat(c.x * (1f - f), tileWorldWidth);
            layers[i].position = new Vector3(c.x - off, c.y + yOffset, 0f);
        }
    }
}
