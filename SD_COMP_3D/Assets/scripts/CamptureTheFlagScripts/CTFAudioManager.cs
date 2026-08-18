using UnityEngine;

public class CTFAudioManager : MonoBehaviour
{
    public static CTFAudioManager Instance { get; private set; }

    [Header("Background Music")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;

    [Range(0f, 1f)]
    public float musicVolume = 0.5f;


    [Header("Player Sound Effects")]

    public AudioSource dashSource;

    [Range(0f, 1f)]
    public float dashVolume = 1f;


    public AudioSource pickupSource;

    [Range(0f, 1f)]
    public float pickupVolume = 1f;


    public AudioSource jumpSource;

    [Range(0f, 1f)]
    public float jumpVolume = 1f;


    public AudioSource dieSource;

    [Range(0f, 1f)]
    public float dieVolume = 1f;


    [Header("Scene Settings")]
    public bool persistAcrossScenes = true;


    private void Awake()
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


    private void Start()
    {
        if (backgroundMusic != null)
        {
            PlayMusic(backgroundMusic);
        }
    }


    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null)
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

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

    public void PlayDashSound()
    {
        if (dashSource == null)
            return;

        dashSource.volume = dashVolume;
        dashSource.Play();
    }


    public void PlayPickupSound()
    {
        if (pickupSource == null)
            return;

        pickupSource.volume = pickupVolume;
        pickupSource.Play();
    }

    public void PlayJumpSound()
    {
        if (jumpSource == null)
            return;

        jumpSource.volume = jumpVolume;
        jumpSource.Play();
    }

    public void PlayDieSound()
    {
        if (dieSource == null)
            return;

        dieSource.volume = dieVolume;
        dieSource.Play();
    }
}