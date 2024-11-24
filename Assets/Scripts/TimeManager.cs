using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public float slowDownLength = 5f;
    public bool slowmo = false;

    public TempScript objectSpeeds;
    public CarControl player;


    private void Update()
    {
        HandleSlowMoToggle();
    }

    public void SlowMo()
    {
        slowmo = true;
        objectSpeeds.slowmoTimeScale = 0.5f;
        player.slowmoTimeScale = 0.75f;
        StartCoroutine(ExitSlowMoAfterDuration());
    }

    private IEnumerator ExitSlowMoAfterDuration()
    {
        yield return new WaitForSecondsRealtime(slowDownLength);
        ExitSlowMo();
    }

    private void ExitSlowMo()
    {
        objectSpeeds.slowmoTimeScale = 1f;
        player.slowmoTimeScale = 1f;

        slowmo = false;
    }

    private void HandleSlowMoToggle()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (slowmo)
                ExitSlowMo();
            else
                SlowMo();
        }
    }
}
