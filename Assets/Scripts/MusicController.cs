using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MusicController : MonoBehaviour
{

    public SongSO[] songs;
    public int currentSong = 0;
    public AudioSource track0Source;
    public AudioSource track1Source;
    public AudioSource track2Source;
    public AudioSource track3Source;
    public AudioSource track4Source;
    public AudioSource track5Source;
    public CarControl car;

    public bool paused = false;
    private void Awake()
    {
        LoadAllSongs();
        LoadSong(currentSong);
        CheckGear();
    }

    private void Update()
    {
        CheckGear();
        if (!track0Source.isPlaying && !paused)
        {
            NextSong();
        }
        SkipSong();
        
    }

    
    public void LoadAllSongs()
    {
        track0Source.mute = true;
        track1Source.mute = true;
        track2Source.mute = true;
        track3Source.mute = true;
        track4Source.mute = true;
        track5Source.mute = true;
        for (int i = 0; i < songs.Length; i++)
        {
            track0Source.clip = songs[i].track0;
            track1Source.clip = songs[i].track1;
            track2Source.clip = songs[i].track2;
            track3Source.clip = songs[i].track3;
            track4Source.clip = songs[i].track4;
            track5Source.clip = songs[i].track5;
            track0Source.Play();
            track1Source.Play();
            track2Source.Play();
            track3Source.Play();
            track4Source.Play();
            track5Source.Play();
        }
        track0Source.mute = false;
        track1Source.mute = false;
        track2Source.mute = false;
        track3Source.mute = false;
        track4Source.mute = false;
        track5Source.mute = false;
    }
    public void NextSong()
    {
        currentSong = Random.Range(0, songs.Length);
        LoadSong(currentSong);
    }

    public void SkipSong()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            NextSong();
        }
    }

    public void PauseMusic()
    {
        AudioSource[] tracks = { track0Source, track1Source, track2Source, track3Source, track4Source, track5Source };
        for (int i = 0; i < tracks.Length; i++) {
            tracks[i].Pause();
        }
        paused = true;
    }

    public void ResumeMusic()
    {
        AudioSource[] tracks = { track0Source, track1Source, track2Source, track3Source, track4Source, track5Source };
        for (int i = 0; i < tracks.Length; i++)
        {
            tracks[i].UnPause();
        }
        paused = false;
    }


    public void LoadSong(int nextSong)
    {
        track0Source.clip = songs[nextSong].track0;
        track1Source.clip = songs[nextSong].track1;
        track2Source.clip = songs[nextSong].track2;
        track3Source.clip = songs[nextSong].track3;
        track4Source.clip = songs[nextSong].track4;
        track5Source.clip = songs[nextSong].track5;
        track0Source.Play();
        track1Source.Play();
        track2Source.Play();
        track3Source.Play();
        track4Source.Play();
        track5Source.Play();
    }

    public void CheckGear()
    {
        AudioSource[] tracks = { track0Source, track1Source, track2Source, track3Source, track4Source, track5Source };
        float fadeLength = 1.25f;
        for (int i = 0; i < tracks.Length; i++)
        {
            if (car.currentGear <= 0)
            {
                if (i == 0)
                {
                    StartCoroutine(FadeIn(tracks[i], tracks[i].volume, 1f, fadeLength));
                }
                else
                {
                    tracks[i].volume = 0;
                }
            }
            else
            {
                if (i <= car.currentGear)
                {
                    StartCoroutine(FadeIn(tracks[i], tracks[i].volume, 1f, fadeLength));
                }
                else
                {
                    StartCoroutine(FadeOut(tracks[i], tracks[i].volume, 0f, fadeLength));
                }
            }
        }
    }

    private IEnumerator FadeIn(AudioSource track, float startVolume, float targetVolume, float duration = 0.5f)
    {
        float time = 0f;
        while (time < duration)
        {
            track.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        track.volume = targetVolume; 
    }

    private IEnumerator FadeOut(AudioSource track, float startVolume, float targetVolume, float duration = 0.5f)
    {
        float time = 0f;
        while (time < duration)
        {
            track.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        track.volume = targetVolume;
    }

}
