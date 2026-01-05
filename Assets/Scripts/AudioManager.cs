using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip backgroundMusic;

    [Header("SFX Clips (Index based)")]
    public AudioClip[] sfxClips; // 0 Jump, 1 Attack, 2 Explode, etc.

    [Header("Game Over SFX")]
    public AudioClip gameOverClip;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Optional: auto-play background music
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            if (!musicSource.isPlaying)
                musicSource.Play();
        }
    }

    public void PlaySFX(int index)
    {
        if (sfxSource == null) return;
        if (sfxClips == null) return;
        if (index < 0 || index >= sfxClips.Length) return;
        if (sfxClips[index] == null) return;

        sfxSource.PlayOneShot(sfxClips[index]);
    }

    public void PlayGameOverSFX()
    {
        if (sfxSource == null) return;
        if (gameOverClip == null) return;

        sfxSource.PlayOneShot(gameOverClip);
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }
}
