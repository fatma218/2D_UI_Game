using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioClip hoverClip;
    public AudioClip clickClip;
    [Range(0f, 1f)] public float volume = 0.7f;
    [Range(0f, 0.3f)] public float pitchVariation = 0.08f;

    AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    void Play(AudioClip clip)
    {
        if (clip == null) return;
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.PlayOneShot(clip, volume);
    }

    public void OnPointerEnter(PointerEventData e) { Play(hoverClip); }
    public void OnPointerClick(PointerEventData e) { Play(clickClip); }
}
