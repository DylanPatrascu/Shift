using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public float slowDownLength = 5f;
    public bool slowmo = false;

    public TempScript objectSpeeds;


    private void Update()
    {
        HandleSlowMoToggle();
    }

    public void SlowMo()
    {
        slowmo = true;
        objectSpeeds.slowmoTimeScale = 0.5f;
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
