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
    private void Awake()
    {
        LoadSong(currentSong);
        CheckGear();
    }

    private void Update()
    {
        CheckGear();
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
        for (int i = 0; i < tracks.Length; i++)
        {
            if(car.currentGear <= 0)
            {
                tracks[0].mute = false;
                if(i != 0)
                {
                    tracks[i].mute = true;
                }
            }
            else
            {
                tracks[i].mute = i > car.currentGear;
            }
        }
    }
}
