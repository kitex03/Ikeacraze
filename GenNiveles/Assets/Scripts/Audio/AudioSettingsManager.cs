using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    public AudioMixer audioMixer; // Referencia al mezclador de audio

    // Método para configurar el volumen de la música
    public void SetMusicVolume(Slider musicSlider)
    {
        float volume = musicSlider.value; // Obtener el valor del slider
        if (volume == 0f)
        {
            audioMixer.SetFloat("MusicVolume", -80f); // Silencio total en -80 dB
        }
        else
        {
            float volumeInDecibels = Mathf.Log10(volume) * 20; // Conversión a decibelios
            audioMixer.SetFloat("MusicVolume", volumeInDecibels);
        }
    }

    // Método para configurar el volumen de los efectos de sonido
    public void SetSFXVolume(Slider sfxSlider)
    {
        float volume = sfxSlider.value; // Obtener el valor del slider
        if (volume == 0f)
        {
            audioMixer.SetFloat("SFXVolume", -80f); // Silencio total en -80 dB
        }
        else
        {
            float volumeInDecibels = Mathf.Log10(volume) * 20; // Conversión a decibelios
            audioMixer.SetFloat("SFXVolume", volumeInDecibels);
        }
    }
}
