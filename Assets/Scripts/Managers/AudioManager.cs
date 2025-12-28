using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameplayMusic;

    [SerializeField] private AudioClip clickSfx;
    [SerializeField] private AudioClip matchSfx;
    [SerializeField] private AudioClip winSfx;
    [SerializeField] private AudioClip loseSfx;

    private const string PREF_MUSIC = "PREF_MUSIC_VOL";
    private const string PREF_SFX = "PREF_SFX_VOL";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        float musicVol = PlayerPrefs.GetFloat(PREF_MUSIC, 0.7f);
        float sfxVol = PlayerPrefs.GetFloat(PREF_SFX, 0.9f);

        if (musicSource) musicSource.volume = musicVol;
        if (sfxSource) sfxSource.volume = sfxVol;
    }

    public void PlayMenuMusic() => PlayMusic(mainMenuMusic);
    public void PlayGameplayMusic() => PlayMusic(gameplayMusic);

    public void PlayMusic(AudioClip clip)
    {
        if (!musicSource || !clip) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (!musicSource) return;
        musicSource.Stop();
        musicSource.clip = null;
    }

    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (!sfxSource || !clip) return;
        sfxSource.PlayOneShot(clip, volumeScale);
    }

    // Kısayollar
    public void Click() => PlaySfx(clickSfx);
    public void Match() => PlaySfx(matchSfx);
    public void Win() => PlaySfx(winSfx);
    public void Lose() => PlaySfx(loseSfx);

    public void SetMusicVolume(float v)
    {
        v = Mathf.Clamp01(v);
        if (musicSource) musicSource.volume = v;
        PlayerPrefs.SetFloat(PREF_MUSIC, v);
        PlayerPrefs.Save();
    }

    public void SetSfxVolume(float v)
    {
        v = Mathf.Clamp01(v);
        if (sfxSource) sfxSource.volume = v;
        PlayerPrefs.SetFloat(PREF_SFX, v);
        PlayerPrefs.Save();
    }
}
