using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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


    // Update is called once per frame
    private void FixedUpdate()
    {
        kphText.text = car.speedKmh.ToString("0"); // Update speed text
        UpdateSpeedometerNeedle(); // Update speedometer needle
        UpdateTachometerNeedle(); // Update tachometer needle
        updateGearText();
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
    public void updateGearText()
    {
        gearText.text = (!car.reverse)? (car.currentGear + 1).ToString() : "R";
    }
}
