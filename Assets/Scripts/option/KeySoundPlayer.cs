using UnityEngine;

public class KeySoundPlayer : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource keySoundSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Lane Key Sound Clips")]
    [SerializeField] private AudioClip lane1Clip; // D - KeySound
    [SerializeField] private AudioClip lane2Clip; // F - KeySound
    [SerializeField] private AudioClip lane3Clip; // J - SFX
    [SerializeField] private AudioClip lane4Clip; // K - Music

    private void Awake()
    {
        if (keySoundSource != null)
            keySoundSource.playOnAwake = false;

        if (sfxSource != null)
            sfxSource.playOnAwake = false;

        if (musicSource != null)
            musicSource.playOnAwake = false;
    }

    public void PlayLane1Sound()
    {
        PlayClip(keySoundSource, lane1Clip);
    }

    public void PlayLane2Sound()
    {
        PlayClip(keySoundSource, lane2Clip);
    }

    public void PlayLane3Sound()
    {
        PlayClip(sfxSource, lane3Clip);
    }

    public void PlayLane4Sound()
    {
        PlayClip(musicSource, lane4Clip);
    }

    private void PlayClip(AudioSource source, AudioClip clip)
    {
        if (source == null)
        {
            Debug.LogWarning("AudioSource is not assigned.");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning("AudioClip is not assigned.");
            return;
        }

        source.PlayOneShot(clip);
    }
}