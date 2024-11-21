using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class TempScript : MonoBehaviour
{
    public float speed = 30.0f;
    public float slowmoTimeScale = 1f;

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed * slowmoTimeScale);
    }
}
