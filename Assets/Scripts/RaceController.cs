using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RaceController : MonoBehaviour
{
    public GameObject player;
    public GameObject endCanvas;
    public TMP_Text timerText;
    public int lapCounter = 0;
    public float timer;
    public UIController uiController;
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == player)
        {
            lapCounter++;
            if (lapCounter == 1)
            {
                uiController.timeUI.SetActive(true);
                uiController.lapUI.SetActive(true);

            }
            if(lapCounter == 4)
            {
                endCanvas.SetActive(true);
                int elapsedTimeMinutes = Mathf.FloorToInt(timer / 60F);
                int elapsedTimeSeconds = Mathf.FloorToInt(timer - elapsedTimeMinutes * 60);
                string elapsedTime = string.Format("{0:0}:{1:00}", elapsedTimeMinutes, elapsedTimeSeconds);
                timerText.text = elapsedTime;
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        endCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimer();
    }

    private void UpdateTimer()
    {
        if (lapCounter >= 1 && lapCounter < 4)
        {
            timer += Time.deltaTime;
        }
    }
}
