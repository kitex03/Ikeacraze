using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    public void PlayMenuMusic()
    {
        musicSource.clip = menuMusic;
        musicSource.Play();
    }

    public void PlayGameMusic()
    {
        musicSource.clip = gameMusic;
        musicSource.Play();
    }
}
