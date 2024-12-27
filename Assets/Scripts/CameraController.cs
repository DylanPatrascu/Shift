using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    private CarControl car;
    private Rigidbody carRB;
    private const float MPH_TO_KMH = 3.6f;

    [Range(0, 2)] public float smoothTime;

    [Header("FOV Settings")]
    [SerializeField] private float minFOV = 60f;
    [SerializeField] private float maxFOV = 120f;
    [SerializeField] private float maxSpeed = 300f;
    [SerializeField] private float boostFOVIncrease = 30f; 
    [SerializeField] private float fovTransitionSpeed = 5f; 

    private float targetFOV;

    private void Awake()
    {
        car = player.GetComponent<CarControl>();
        carRB = player.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        AdjustFOV();
        ApplyFOV();
    }

    private void AdjustFOV()
    {
        if (carRB != null && virtualCamera != null)
        {
            float speedKmh = carRB.velocity.magnitude * MPH_TO_KMH;

            targetFOV = Mathf.Lerp(minFOV, maxFOV, speedKmh / maxSpeed);

            if (Input.GetKey(KeyCode.LeftControl))
            {
                targetFOV += boostFOVIncrease;
            }
        }
    }

    private void ApplyFOV()
    {
        if (virtualCamera != null)
        {
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, targetFOV, fovTransitionSpeed * Time.deltaTime);
        }
    }
}
