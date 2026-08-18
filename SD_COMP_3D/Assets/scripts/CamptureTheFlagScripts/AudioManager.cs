using UnityEngine;

// Put ONE of these in your first scene (e.g. the main menu), with
// 'Persist Across Scenes' checked, so background music keeps playing
// through scene changes into gameplay. Anything else in the game can call
// AudioManager.Instance.PlaySFX(clip) etc. from anywhere.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Background Music")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    [Header("Sound Effects")]
    public AudioSource sfxSource;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    public bool persistAcrossScenes = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        if (backgroundMusic != null)
        {
            PlayMusic(backgroundMusic);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    // General-purpose SFX, plays through the shared 2D sfxSource - good for
    // UI sounds and anything that doesn't need to come from a specific
    // world position.
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // For sounds that should come from a specific world position (e.g. a
    // pickup, an explosion) - uses Unity's built-in one-shot 3D playback,
    // no dedicated AudioSource component needed on the object itself.
    public void PlaySFXAtPoint(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, position, sfxVolume);
    }
}