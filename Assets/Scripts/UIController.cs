using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JetBrains.Annotations;
public class UIController : MonoBehaviour
{
    public CarControl car; // Reference to the car script
    public GameObject SpeedometerNeedle; // Needle for speedometer
    public GameObject TachometerNeedle; // Needle for tachometer
    public TMP_Text kphText; // UI Text for speed
    public TMP_Text gearText; // UI Text for speed
    private float tachStartPosition = 154f; // Tachometer start angle
    private float tachEndPosition = -27f; // Tachometer end angle
    private float speedStartPosition = 142f; // Speedometer start angle
    private float speedEndPosition = -127f; // Speedometer end angle

    public Image albumCover;
    public TMP_Text songName;
    public TMP_Text artistName;
    public TMP_Text elapsedTime;
    public TMP_Text songLength;
    public MusicController musicController;



    // Update is called once per frame
    private void FixedUpdate()
    {
        kphText.text = car.speedKmh.ToString("0"); // Update speed text
        UpdateSpeedometerNeedle(); // Update speedometer needle
        UpdateTachometerNeedle(); // Update tachometer needle
        UpdateGearText();
        UpdateSong();
    }

    void Start()
    {
        // Set the speedometer needle to the starting position
        SpeedometerNeedle.transform.localRotation = Quaternion.Euler(0, 0, speedStartPosition);

        // Set the tachometer needle to the starting position
        TachometerNeedle.transform.localRotation = Quaternion.Euler(0, 0, tachStartPosition);
    }

    public void UpdateSpeedometerNeedle()
    {
        float desiredPosition = speedEndPosition - speedStartPosition; // Difference between start and end
        float temp = Mathf.Clamp01(car.speedKmh / car.maxSpeedKmh); // Normalize speed between 0 and 1
        float needleRotation = speedStartPosition + temp * desiredPosition;

        // Apply the rotation using localRotation to avoid issues with global axes
        SpeedometerNeedle.transform.localRotation = Quaternion.Euler(0, 0, needleRotation);
    }


    // Update the tachometer needle position
    public void UpdateTachometerNeedle()
    {
        float desiredPosition = tachEndPosition - tachStartPosition; // Difference between start and end
        float temp = Mathf.Clamp01(car.engineRPM / car.maxRPM); // Normalize speed between 0 and 1
        float needleRotation = tachStartPosition + temp * desiredPosition;
        // Apply the rotation using localRotation to avoid issues with global axes
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
