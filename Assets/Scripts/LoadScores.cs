using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadScores : MonoBehaviour
{
    public TMP_Text score1, score2, score3, name1, name2, name3;
    void Start()
    {
        score1.text = GetNiceScore(PlayerPrefs.GetFloat("Score1"));
        score2.text = GetNiceScore(PlayerPrefs.GetFloat("Score2"));
        score3.text = GetNiceScore(PlayerPrefs.GetFloat("Score2"));

        name1.text = PlayerPrefs.GetString("Name1");
        name2.text = PlayerPrefs.GetString("Name2");
        name3.text = PlayerPrefs.GetString("Name3");

        if (score1.text == "1000" || score1.text == "0")
        {
            score1.text = "---";
        }

        if (score2.text == "1000" || score2.text == "0")
        {
            score2.text = "---";
        }

        if (score3.text == "1000" || score3.text == "0")
        {
            score3.text = "---";
        }

        if (name1.text == "")
        {
            name1.text = "---";
            score1.text = "---";
        }
        if (name2.text == "")
        {
            name2.text = "---";
            score2.text = "---";

        }
        if (name3.text == "")
        {
            name3.text = "---";
            score3.text = "---";

        }
    }

    public string GetNiceScore(float score)
    {
        int elapsedTimeMinutes = Mathf.FloorToInt(score / 60F);
        int elapsedTimeSeconds = Mathf.FloorToInt(score - elapsedTimeMinutes * 60);
        string elapsedTime = string.Format("{0:0}:{1:00}", elapsedTimeMinutes, elapsedTimeSeconds);
        return elapsedTime;
    }
   
}
