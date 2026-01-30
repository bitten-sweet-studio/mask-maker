using SaintsField;
using UnityEngine;

[CreateAssetMenu(
        fileName = "QuickSoundAsset",
        menuName = MaskMakerStatics.ScriptableObjectMenuName + "/Quick Sound")]
public class QuickSoundAsset : ScriptableObject
{
    [SerializeField]
    private AudioClip[] audioClips;

    [SerializeField, MinMaxSlider(0, 1, 0.1f)]
    private Vector2 _volumeRange;

    [SerializeField, MinMaxSlider(0.5f, 2, 0.1f)]
    private Vector2 _pitchRange;

    [SerializeField]
    private float _spatialBlend = 1f;

    public void Play()
    {
        Play(Vector3.zero);
    }

    public void Play(Transform originTransform)
    {
        if (originTransform == null)
        {
            Play();
            return;
        }

        Play(originTransform.position);
    }

    public void Play(Vector3 position)
    {
        if (audioClips == null || audioClips.Length == 0) return;

        AudioClip clip = audioClips.GetRandomElement();
        if (clip == null) return;

        GameObject audioSource = new GameObject($"QuickSound_{name}");
        audioSource.transform.position = position;

        AudioSource source = audioSource.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = _volumeRange.GetRandomValue();
        source.pitch = _pitchRange.GetRandomValue();
        source.spatialBlend = _spatialBlend;
        source.Play();

        Destroy(audioSource, clip.length + 0.1f);
    }
}
