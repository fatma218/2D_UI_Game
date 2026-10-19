using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Puzzle 2: The Lullaby Stones.
// The music box plays a melody. The stones all look the same; only their sound tells them apart.
// Touch the stones in the order of the melody to open the gate.
public class SoundPuzzle : MonoBehaviour
{
    [Header("Scene links")]
    public Transform player;
    public Transform musicBox;
    public Transform[] stones;
    public GameObject gate;

    [Header("UI")]
    public TMP_Text prompt;
    public TMP_Text thought;

    [Header("Puzzle")]
    public int[] melody = { 2, 0, 3, 1 };
    public float noteGap = 0.75f;
    public float interactRange = 1.6f;
    [Range(0f, 1f)] public float volume = 0.8f;

    readonly float[] freqs = { 261.63f, 329.63f, 392.00f, 523.25f };
    AudioSource src;
    AudioClip[] tones;
    AudioClip wrongClip, successClip;
    readonly List<int> attempt = new List<int>();
    bool busy, solved, heardOnce;
    Coroutine thoughtRoutine;

    void Start()
    {
        src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 0f;
        src.playOnAwake = false;

        tones = new AudioClip[freqs.Length];
        for (int i = 0; i < freqs.Length; i++) tones[i] = MakeTone(new[] { freqs[i] }, 0.8f, 4f);
        wrongClip = MakeTone(new[] { 98f, 103f }, 0.6f, 5f);
        successClip = MakeTone(new[] { 261.63f, 329.63f, 392f, 523.25f }, 2.2f, 1.6f);

        if (prompt) prompt.gameObject.SetActive(false);
        if (thought) thought.text = "";
    }

    void Update()
    {
        if (player == null) return;
        if (solved) { if (prompt) prompt.gameObject.SetActive(false); return; }

        // Find the closest thing the player can interact with: -1 = music box, 0.. = stone index, -2 = none
        int target = -2;
        float best = interactRange;
        if (musicBox != null)
        {
            float d = Vector2.Distance(player.position, musicBox.position);
            if (d < best) { best = d; target = -1; }
        }
        for (int i = 0; i < stones.Length; i++)
        {
            if (stones[i] == null) continue;
            float d = Vector2.Distance(player.position, stones[i].position);
            if (d < best) { best = d; target = i; }
        }

        if (prompt)
        {
            bool show = target != -2 && !busy;
            prompt.gameObject.SetActive(show);
            if (show) prompt.text = target == -1 ? "E : listen" : "E : touch";
        }

        if (target != -2 && !busy && InteractPressed())
        {
            if (target == -1) StartCoroutine(PlayMelody());
            else StartCoroutine(TouchStone(target));
        }
    }

    IEnumerator PlayMelody()
    {
        busy = true;
        attempt.Clear();
        if (!heardOnce) { heardOnce = true; Say("A melody... I know this melody."); }
        yield return StartCoroutine(Flash(musicBox));
        foreach (int n in melody)
        {
            src.PlayOneShot(tones[n], volume);
            yield return new WaitForSeconds(noteGap);
        }
        busy = false;
    }

    IEnumerator TouchStone(int i)
    {
        src.PlayOneShot(tones[i], volume);
        StartCoroutine(Flash(stones[i]));
        attempt.Add(i);

        if (attempt.Count < melody.Length) yield break;

        busy = true;
        yield return new WaitForSeconds(0.9f);

        bool ok = true;
        for (int k = 0; k < melody.Length; k++) if (attempt[k] != melody[k]) ok = false;

        if (ok)
        {
            solved = true;
            src.PlayOneShot(successClip, volume);
            Say("I hummed this to him every night. The path is open.", 6f);
            yield return StartCoroutine(OpenGate());
        }
        else
        {
            src.PlayOneShot(wrongClip, volume);
            Say("No... that is not how it goes.");
            attempt.Clear();
        }
        busy = false;
    }

    IEnumerator Flash(Transform t)
    {
        if (t == null) yield break;
        var sr = t.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;
        Color orig = sr.color;
        Vector3 scale = t.localScale;
        float time = 0f;
        while (time < 0.45f)
        {
            time += Time.deltaTime;
            float k = time / 0.45f;
            sr.color = Color.Lerp(Color.white, orig, k);
            t.localScale = scale * Mathf.Lerp(1.2f, 1f, k);
            yield return null;
        }
        sr.color = orig;
        t.localScale = scale;
    }

    IEnumerator OpenGate()
    {
        if (gate == null) yield break;
        var col = gate.GetComponent<Collider2D>();
        if (col) col.enabled = false;
        var sr = gate.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            float time = 0f;
            while (time < 2.5f)
            {
                time += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, time / 2.5f);
                sr.color = c;
                yield return null;
            }
        }
        gate.SetActive(false);
    }

    void Say(string line, float seconds = 4f)
    {
        if (thought == null) return;
        if (thoughtRoutine != null) StopCoroutine(thoughtRoutine);
        thoughtRoutine = StartCoroutine(ShowThought(line, seconds));
    }

    IEnumerator ShowThought(string line, float seconds)
    {
        thought.text = line;
        yield return new WaitForSeconds(seconds);
        thought.text = "";
    }

    static AudioClip MakeTone(float[] frequencies, float duration, float decay)
    {
        int rate = 44100;
        int n = (int)(rate * duration);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float env = Mathf.Exp(-t * decay) * Mathf.Min(1f, t * 60f);
            float s = 0f;
            foreach (float f in frequencies)
                s += Mathf.Sin(2f * Mathf.PI * f * t) + 0.35f * Mathf.Sin(4f * Mathf.PI * f * t) + 0.12f * Mathf.Sin(6f * Mathf.PI * f * t);
            data[i] = s / frequencies.Length * 0.4f * env;
        }
        var clip = AudioClip.Create("tone", n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    static bool InteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        return k != null && k.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }
}
