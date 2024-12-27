using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public float slowDownLength = 5f;
    public bool slowmo = false;
    public float meshRefreshRate = 0.1f;
    public MeshRenderer[] meshRenderers;
    public GameObject trafficParent;
    public CarControl player;
    public Transform posToSpawn;
    public float meshDestroyDelay = 3f;
    public Material effectMaterial;

    private float currentHue = 0f; 
    public float saturation = 0.5f; 
    public float brightness = 0.5f; 

    private bool isInSlowMo = false;
    private float cycleSpeed;


    private void Start()
    {
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            meshRenderers = GetComponentsInChildren<MeshRenderer>();
        }

        
    }

    private void Update()
    {
        HandleSlowMoToggle();
    }

   
    private void SetNPCTimeScale(float timeScale)
    {
        // Get all NPCController components from the parent's children
        NPCController[] npcControllers = trafficParent.GetComponentsInChildren<NPCController>();

        // Iterate through each component and set its slowmoTimeScale
        foreach (var npcController in npcControllers)
        {
            npcController.slowmoTimeScale = timeScale;
        }
    }

    public void SlowMo()
    {
        slowmo = true;
        isInSlowMo = true;
        SetNPCTimeScale(0.25f);
        Time.timeScale = 0.5f;
        player.steeringMax = 70f;
        currentHue = 165f;

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
                    for(int i = 0; i < originalMaterials.Length; i++)
                    {
                        originalMaterials[i] = effectMaterial;
                    }
                    trailRenderer.materials = originalMaterials;

                    Color trailColor = GetCyclingColor();
                    trailColor.a = 0.5f;

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
        SetNPCTimeScale(1f);
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
