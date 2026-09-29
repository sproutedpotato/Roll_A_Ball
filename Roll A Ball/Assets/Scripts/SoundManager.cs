using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    public void SetBGMVolume()
    {
        float volume = bgmSlider.value;
        float decibels = VolumeToDecibels(volume);

        audioMixer.SetFloat("BGM", decibels);
    }

    public void SetSFXVolume()
    {
        float volume = sfxSlider.value;
        float decibels = VolumeToDecibels(volume);

        audioMixer.SetFloat("SFX", decibels);
    }

    private float VolumeToDecibels(float volume)
    {
        return volume <= 0.0001f
            ? -80f
            : Mathf.Log10(volume) * 20f;
    }
}