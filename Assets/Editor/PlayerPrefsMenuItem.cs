using UnityEngine;
using UnityEditor;

public class CustomMenuItem : Editor
{
    [MenuItem("Tools/Clear PlayerPrefs")]
    private static void NewMenuOption()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Player Prefs cleared.");
    }
}