using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "ScriptableObjects/Song", order = 1)]
public class SongSO : ScriptableObject
{
    public string songName;
    public string artist;
    public int songLength;
    public Image albumCover;
    public AudioClip track0;
    public AudioClip track1;
    public AudioClip track2;
    public AudioClip track3;
    public AudioClip track4;
    public AudioClip track5;
}
