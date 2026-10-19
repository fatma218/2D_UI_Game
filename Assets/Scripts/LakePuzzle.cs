using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Puzzle 1: the player finds the lake by sound, touches the water and understands.
public class LakePuzzle : MonoBehaviour
{
    [Header("Scene links")]
    public Transform player;
    public MonoBehaviour playerController;
    public Collider2D lakeZone;
    public AudioSource waterSource;
    public AudioClip touchClip;

    [Header("UI")]
    public Image fog;
    public GameObject prompt;
    public GameObject panel;
    public TMP_Text panelText;

    [Header("Tuning")]
    public float hearRange = 14f;
    public float touchRange = 0.6f;
    public float maxVolume = 0.9f;
    public float fogFar = 0.85f;
    public float fogNear = 0.35f;
    public float fogUnderstood = 0.2f;

    [TextArea] public string[] lines;

    bool busy;
    bool understood;

    void Start()
    {
        if (panel) panel.SetActive(false);
        if (prompt) prompt.SetActive(false);
        if (waterSource) { waterSource.loop = true; waterSource.volume = 0f; waterSource.spatialBlend = 0f; waterSource.Play(); }
    }

    void Update()
    {
        if (busy || player == null || lakeZone == null) return;

        Vector2 p = player.position;
        Vector2 closest = lakeZone.ClosestPoint(p);
        float d = Vector2.Distance(p, closest);
        float t = Mathf.Clamp01(1f - d / hearRange);

        // Hearing: louder and panned toward the lake
        if (waterSource)
        {
            waterSource.volume = t * t * maxVolume;
            waterSource.panStereo = Mathf.Clamp((closest.x - p.x) / 6f, -1f, 1f);
        }

        // Vision: the fog thins out near the lake, and more once he understands
        if (fog)
        {
            float target = understood ? fogUnderstood : Mathf.Lerp(fogFar, fogNear, t);
            Color c = fog.color;
            c.a = Mathf.Lerp(c.a, target, Time.deltaTime * 3f);
            fog.color = c;
        }

        bool near = d <= touchRange;
        if (prompt) prompt.SetActive(near && !understood);
        if (near && !understood && InteractPressed()) StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        busy = true;
        if (prompt) prompt.SetActive(false);
        if (playerController) playerController.enabled = false;
        if (waterSource && touchClip) waterSource.PlayOneShot(touchClip, 1f);

        panel.SetActive(true);
        yield return null;
        foreach (string line in lines)
        {
            panelText.text = line;
            yield return null;
            yield return new WaitUntil(InteractPressed);
        }
        panel.SetActive(false);

        if (playerController) playerController.enabled = true;
        understood = true;
        busy = false;
    }

    static bool InteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        return k != null && (k.eKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
#endif
    }
}
