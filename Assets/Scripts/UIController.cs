using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JetBrains.Annotations;
public class UIController : MonoBehaviour
{
    public CarControl car; 
    public GameObject SpeedometerNeedle; 
    public GameObject TachometerNeedle; 
    public TMP_Text kphText;
    public TMP_Text gearText;
    private float tachStartPosition = 154f;
    private float tachEndPosition = -27f;
    private float speedStartPosition = 142f;
    private float speedEndPosition = -127f; 

    private float maxSpeed = 260f; //ideally 300, but 260 to match UI

    public Image albumCover;
    public TMP_Text songName;
    public TMP_Text artistName;
    public TMP_Text elapsedTime;
    public TMP_Text songLength;
    public MusicController musicController;



    private void FixedUpdate()
    {
        kphText.text = car.speedKmh.ToString("0"); 
        UpdateSpeedometerNeedle();
        UpdateTachometerNeedle(); 
        UpdateGearText();
        UpdateSong();
    }

    void Start()
    {
        SpeedometerNeedle.transform.localRotation = Quaternion.Euler(0, 0, speedStartPosition);

        TachometerNeedle.transform.localRotation = Quaternion.Euler(0, 0, tachStartPosition);
    }

    public void UpdateSpeedometerNeedle()
    {
        float desiredPosition = speedEndPosition - speedStartPosition; 
        float temp = Mathf.Clamp01(car.speedKmh / maxSpeed); 
        float needleRotation = speedStartPosition + temp * desiredPosition;

        SpeedometerNeedle.transform.localRotation = Quaternion.Euler(0, 0, needleRotation);
    }


    public void UpdateTachometerNeedle()
    {
        float desiredPosition = tachEndPosition - tachStartPosition;
        float temp = Mathf.Clamp01(car.engineRPM / car.maxRPM);
        float needleRotation = tachStartPosition + temp * desiredPosition;
        TachometerNeedle.transform.localRotation = Quaternion.Euler(0, 0, needleRotation);
    }
    public void UpdateGearText()
    {
        gearText.text = (!car.reverse)? (car.currentGear + 1).ToString() : "R";
    }

    public void UpdateSong()
    {
        albumCover.sprite = musicController.songs[musicController.currentSong].albumCover;
        songName.text = musicController.songs[musicController.currentSong].songName;
        artistName.text = musicController.songs[musicController.currentSong].artist;
        
        int songLengthMinutes = Mathf.FloorToInt(musicController.songs[musicController.currentSong].songLength / 60F);
        int songLengthSeconds = Mathf.FloorToInt(musicController.songs[musicController.currentSong].songLength - songLengthMinutes * 60);
        string songLengthNiceTime = string.Format("{0:0}:{1:00}", songLengthMinutes, songLengthSeconds);

        int elapsedTimeMinutes = Mathf.FloorToInt(musicController.track0Source.time / 60F);
        int elapsedTimeSeconds = Mathf.FloorToInt(musicController.track0Source.time - elapsedTimeMinutes * 60);
        string elapsedTimeNiceTime = string.Format("{0:0}:{1:00}", elapsedTimeMinutes, elapsedTimeSeconds);

        songLength.text = songLengthNiceTime;
        elapsedTime.text = elapsedTimeNiceTime;
    }
}
