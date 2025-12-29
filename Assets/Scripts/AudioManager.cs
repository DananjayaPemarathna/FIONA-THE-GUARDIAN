using UnityEngine;

/// <summary>
/// Central audio controller using a simple Singleton pattern.
/// Responsibilities:
/// - Plays looping background music
/// - Plays sound effects (SFX) by index
/// - Persists across scene loads via DontDestroyOnLoad
/// 
/// SFX Index Convention (based on your project):
/// 0 = Jump
/// 1 = Attack
/// 2 = Death
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource; // Handles background music (looping)
    [SerializeField] private AudioSource sfxSource;   // Handles one-shot sound effects

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip[] sfxClips; // 0=Jump, 1=Attack, 2=Death

    private void Awake()
    {
        // Singleton Pattern:
        // Ensures only one AudioManager exists and can be accessed globally.
        if (instance == null)
        {
            instance = this;

            // Keep this AudioManager alive when scenes change.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Prevent duplicates when scenes reload.
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Auto-play background music at startup (if assigned).
        PlayMusic(backgroundMusic);
    }

    /// <summary>
    /// Plays looping background music using the musicSource.
    /// </summary>
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        // Safety check: musicSource must be assigned in Inspector.
        if (musicSource == null) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    /// <summary>
    /// Plays a one-shot sound effect using an index from the sfxClips array.
    /// Adds slight pitch variation for a more natural feel.
    /// </summary>
    public void PlaySFX(int index)
    {
        // Safety checks to avoid runtime errors.
        if (sfxSource == null) return;
        if (sfxClips == null || sfxClips.Length == 0) return;
        if (index < 0 || index >= sfxClips.Length) return;
        if (sfxClips[index] == null) return;

        // Slight random pitch variation for better audio feel (avoids repetition).
        sfxSource.pitch = Random.Range(0.9f, 1.1f);

        // Play the selected clip without interrupting currently playing clips.
        sfxSource.PlayOneShot(sfxClips[index]);
    }
}
