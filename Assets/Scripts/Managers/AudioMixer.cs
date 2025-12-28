using System.Collections;
using UnityEngine;

public enum SoundEffects
{
    land,
    swap,
    resolve,
    upgrade,
    powerup,
    score
}

[RequireComponent(typeof(AudioSource))]
public class AudioMixer : Singleton<AudioMixer>
{
    [SerializeField] private AudioSource music;
    private AudioSource soundEffects;

    [Tooltip("0=land\n" + "1=swap\n" + "2=resolve\n" + "3=upgrade\n" + "4=powerup\n" + "5=score\n")]
    [SerializeField] private AudioClip[] sounds;

    protected override void Init()
    {
        soundEffects = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);
    }

    //play background music
    public void PlayMusic()
    {
        if (music == null) return;
        if (music.isPlaying) return; // Prevent restarting if already playing
        
        music.Play();
    }

    //pause/unpause background music
    public void PauseMusic(bool pause)
    {
        if (pause)
        {
            music.Pause();
        }
        else
        {
            music.UnPause();
        }
    }

    //play sound effect
    public void PlaySound(SoundEffects effect)
    {
        soundEffects.PlayOneShot(sounds[(int)effect]);
    }

    //play sound effect after a time delay
    public IEnumerator PlayDelayedSound(SoundEffects effect, float timeDelay)
    {
        yield return new WaitForSeconds(timeDelay);
        PlaySound(effect);
    }
}
