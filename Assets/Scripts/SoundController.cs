using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    private const float MUSIC_VOLUME_SCALAR = 0.7f;
    public AudioClip BGM;
    public Sprite MUSIC_ON;
    public Sprite MUSIC_OFF;
    public Sprite SFX_ON;
    public Sprite SFX_OFF;

    private AudioSource m_MusicSource;
    private AudioSource m_SfxSource;
    private bool m_MusicOn;
    private bool m_SfxOn;

    // Start is called before the first frame update
    void Start()
    {
        m_SfxOn = true;

        m_MusicSource = GameObject.FindGameObjectWithTag("MusicPlayer").GetComponent<AudioSource>();
        m_SfxSource = GameObject.FindGameObjectWithTag("SfxPlayer").GetComponent<AudioSource>();

        m_MusicSource.loop = true;
        m_MusicSource.clip = BGM;
        // disable music by default
        DisableMusic();
        SetVolume(0.5f);
    }

    /**
     * Sets the volume of the sound controller based on value
     * 
     * @param volume: float conatining sound to change to
     */
    public void SetVolume(float volume)
    {   
        m_MusicSource.volume = volume * MUSIC_VOLUME_SCALAR;
        m_SfxSource.volume = volume;
    }
    /**
     * Setst the volume of the sound controller based on slider
     * 
     * @param slider: to use if being called from a slider object
     */
    public void SetVolume(Slider slider)
    {
        SetVolume(slider.value);
    }
    // toggle music on / off
    public void ToggleMusic()
    {
        if(m_MusicOn)
        {
            DisableMusic();
        }
        else
        {
            EnableMusic();
        }
    }
    // turn on music

    void EnableMusic()
    {
        m_MusicOn = true;
        m_MusicSource.Play();

        Image playIcon = GameObject.FindGameObjectWithTag("MusicButton").GetComponent<Image>();
        playIcon.sprite = MUSIC_ON;
    }
    // turn off music
    void DisableMusic()
    {
        m_MusicOn = false;
        m_MusicSource.Stop();

        Image playIcon = GameObject.FindGameObjectWithTag("MusicButton").GetComponent<Image>();
        playIcon.sprite = MUSIC_OFF;
    }
    // toggle sfx on / off
    public void ToggleSFX()
    {
        m_SfxOn = !m_SfxOn;

        Image sfxIcon = GameObject.FindGameObjectWithTag("SfxButton").GetComponent<Image>();
        sfxIcon.sprite = m_SfxOn ? SFX_ON : SFX_OFF;
    }
    /**
     * Plays a specified audio clip in the sfx channel (only if its unmuted)
     * 
     * @param clip: audio clip to play
     */
    public void PlaySFX(AudioClip clip)
    {
        if (!m_SfxOn)
        {
            return;
        }
        m_SfxSource.clip = clip;
        m_SfxSource.Play();
    }
}
