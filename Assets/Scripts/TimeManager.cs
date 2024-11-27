using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public float slowDownLength = 5f;
    public bool slowmo = false;
    public float meshRefreshRate = 0.1f;
    public MeshRenderer[] meshRenderers;
    public NPCController objectSpeeds;
    public CarControl player;
    public Transform posToSpawn;
    public float meshDestroyDelay = 3f;

    // HSV color cycling variables (using 0-255 hue range)
    private float currentHue = 0f; // Current hue value (0 to 255)
    public float saturation = 0.5f; // Saturation (0 to 1)
    public float brightness = 0.5f; // Brightness (0 to 1)

    private bool isInSlowMo = false;
    private float cycleSpeed;

    private void Start()
    {
        // Cache all MeshRenderers
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            meshRenderers = GetComponentsInChildren<MeshRenderer>();
        }

        // Set the initial sorting layer for the car (real car will always be on top)
        foreach (var renderer in meshRenderers)
        {
            renderer.sortingLayerName = "RealCar";
            renderer.sortingOrder = 1;
        }
    }

    private void Update()
    {
        HandleSlowMoToggle();
    }

    public void SlowMo()
    {
        slowmo = true;
        isInSlowMo = true;
        objectSpeeds.slowmoTimeScale = 0.25f;
        Time.timeScale = 0.5f;
        player.steeringMax = 70f;
        currentHue = 165f;

        foreach (var renderer in meshRenderers)
        {
            renderer.sortingLayerName = "RealCar";
            renderer.sortingOrder = 1;
        }

        cycleSpeed = 1f / slowDownLength; 

        StartCoroutine(ActivateTrail());
        StartCoroutine(ExitSlowMoAfterDuration());
    }

    private IEnumerator ExitSlowMoAfterDuration()
    {
        yield return new WaitForSecondsRealtime(slowDownLength);
        ExitSlowMo();
    }

    private IEnumerator ActivateTrail()
    {
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            meshRenderers = GetComponentsInChildren<MeshRenderer>();
        }

        while (isInSlowMo)
        {
            foreach (var renderer in meshRenderers)
            {
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    GameObject trailObject = new GameObject($"{renderer.name}_TrailMesh");
                    trailObject.transform.position = renderer.transform.position;
                    trailObject.transform.rotation = renderer.transform.rotation;
                    trailObject.transform.localScale = renderer.transform.lossyScale;

                    MeshRenderer trailRenderer = trailObject.AddComponent<MeshRenderer>();
                    MeshFilter trailFilter = trailObject.AddComponent<MeshFilter>();

                    trailFilter.mesh = Instantiate(meshFilter.sharedMesh);

                    Material[] originalMaterials = renderer.materials;
                    trailRenderer.materials = originalMaterials;

                    trailRenderer.sortingLayerName = "Default"; 
                    trailRenderer.sortingOrder = 0;

                    Color trailColor = GetCyclingColor();
                    trailColor.a = 0.1f;

                    for (int i = 0; i < trailRenderer.materials.Length; i++)
                    {
                        trailRenderer.materials[i].color = trailColor;
                    }

                    Destroy(trailObject, meshDestroyDelay);
                }
            }

            currentHue += 10.625f;

            if (currentHue >= 255f)
            {
                currentHue -= 255f;
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }
    }

    private Color GetCyclingColor()
    {
        float normalizedHue = currentHue / 255f;
        return Color.HSVToRGB(normalizedHue, saturation, brightness);
    }

    private void ExitSlowMo()
    {
        objectSpeeds.slowmoTimeScale = 1f;
        player.steeringMax = 50f;
        Time.timeScale = 1f;
        isInSlowMo = false;
        slowmo = false;
    }

    private void HandleSlowMoToggle()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (slowmo)
            {
                ExitSlowMo();
            }
            else
            {
                SlowMo();
            }
        }
    }
}
