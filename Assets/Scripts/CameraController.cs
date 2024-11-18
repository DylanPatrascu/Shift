using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject player; // Assign the player GameObject in the Inspector
    [SerializeField] private CinemachineVirtualCamera virtualCamera; // Assign your Cinemachine virtual camera

    private CarControl car;
    private Rigidbody carRB;
    private const float MPH_TO_KMH = 3.6f;

    [Range(0, 2)] public float smoothTime;

    [Header("FOV Settings")]
    [SerializeField] private float minFOV = 60f; // Minimum FOV
    [SerializeField] private float maxFOV = 120f; // Maximum FOV
    [SerializeField] private float maxSpeed = 300f; // Speed at which FOV reaches maxFOV (CHANGE TO 250)
    [SerializeField] private float boostFOV = 30f; // FOV increase when boosting
    [SerializeField] private float boostSpeed = 5f; // Speed of FOV change when boosting

    private float targetFOV;

    private void Awake()
    {
        car = player.GetComponent<CarControl>();
        carRB = player.GetComponent<Rigidbody>(); // Assuming the car has a Rigidbody
    }

    private void Update()
    {

        AdjustFOV();
        BoostingFOV();
    }

    private void AdjustFOV()
    {
        if (carRB != null && virtualCamera != null)
        {
            // Calculate current speed in km/h
            float speedKmh = carRB.velocity.magnitude * MPH_TO_KMH;

            // Interpolate FOV based on speed
            targetFOV = Mathf.Lerp(minFOV, maxFOV, speedKmh / maxSpeed);
        }
    }

    private void BoostingFOV()
    {
        // If boost (Ctrl) is held down, smoothly transition to boosted FOV
        if (Input.GetKey(KeyCode.LeftControl))
        {
            targetFOV = Mathf.Lerp(targetFOV, virtualCamera.m_Lens.FieldOfView + boostFOV, boostSpeed * Time.deltaTime);
        }
        else
        {
            // Smoothly return to the normal FOV
            targetFOV = Mathf.Lerp(targetFOV, Mathf.Lerp(minFOV, maxFOV, carRB.velocity.magnitude * MPH_TO_KMH / maxSpeed), smoothTime * Time.deltaTime);
        }

        // Set the virtual camera's FOV with smooth transition
        virtualCamera.m_Lens.FieldOfView = targetFOV;
    }
}
