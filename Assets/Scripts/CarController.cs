using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

// Tutorial Link: https://www.youtube.com/playlist?list=PLtYhPiKW6dMUdJPfA1HH2HbPjp45PU9Tf
public class CarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody carRB;
    [SerializeField] private Transform[] rayPoints;
    [SerializeField] private LayerMask drivable;
    [SerializeField] private Transform accelerationPoint;
    [SerializeField] private GameObject[] tires = new GameObject[4];
    [SerializeField] private GameObject[] frontTireParents = new GameObject[2];
    [SerializeField] private TrailRenderer[] skidMarks = new TrailRenderer[2];
    [SerializeField] private ParticleSystem[] skidSmokes = new ParticleSystem[2];
    [SerializeField] private AudioSource engineSound, skidSound;


    [Header("Suspension Settings")]
    [SerializeField] private float springStiffness;
    //the lower the damper stiffness, the bouncier the car
    [SerializeField] private float damperStiffness;
    [SerializeField] private float restLength;
    [SerializeField] private float springTravel;
    [SerializeField] private float wheelRadius;
    [SerializeField] private float brakingDeceleration = 100f;
    [SerializeField] private float brakingDragCoefficient = 0.5f;

    private int[] wheelsGrounded = new int[4];
    private bool isGrounded = false;

    [Header("Input")]
    private float moveInput = 0;
    private float steerInput = 0;

    [Header("Car Settings")]
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float steerStrength = 15f;
    [SerializeField] private AnimationCurve turningCurve;
    [SerializeField] private float dragCoeffecient = 1f;

    [Header("Visuals")]
    [SerializeField] private float tireRotationSpeed = 3000f;
    [SerializeField] private float maxSteeringAngle = 30f;
    [SerializeField] private float minSideSkidVelocity = 10f;

    [Header("Audio")]
    [SerializeField][RangeAttribute(0,1)] private float minPitch = 1f;
    [SerializeField][RangeAttribute(1, 5)] private float maxPitch = 5f;



    private Vector3 currentCarLocalVelocity = Vector3.zero;
    private float carVelocityRatio = 0;

    #region Unity Functions
    private void Start()
    {
        carRB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        CalculateCarVelocity();
        Movement();
        Visuals();
        EngineSound();
    }
    private void Update()
    {
        GetPlayerInput();
    }
    #endregion

    #region Input Handling
    private void GetPlayerInput()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }
    #endregion

    #region Audio

    private void EngineSound()
    {
        engineSound.pitch = Mathf.Lerp(minPitch, maxPitch, Mathf.Abs(carVelocityRatio));
    }

    private void ToggleSkidSound(bool toggle)
    {
        if (skidSound.isPlaying == true && toggle == false)
        {
            skidSound.Stop();
        }
        else if (skidSound.isPlaying == false && toggle == true)
        {
            skidSound.Play();
        }
        
    }
    #endregion

    #region Visuals

    private void Visuals()
    {
        TireVisuals();
        VFX();
    }
    private void SetTirePosition(GameObject tire, Vector3 targetPosition)
    {
        tire.transform.position = targetPosition;
    }

    private void TireVisuals()
    {
        float steeringAngle = maxSteeringAngle * steerInput;

        for (int i = 0; i < tires.Length; i++)
        {
            if (i < 2)
            {
                tires[i].transform.Rotate(Vector3.right, tireRotationSpeed * carVelocityRatio * Time.deltaTime, Space.Self);
                frontTireParents[i].transform.localEulerAngles = new Vector3(frontTireParents[i].transform.localEulerAngles.x, steeringAngle, frontTireParents[i].transform.localEulerAngles.z);
            }
            else
            {
                tires[i].transform.Rotate(Vector3.right, tireRotationSpeed * moveInput * Time.deltaTime, Space.Self);
            }
        }
    }

    private void VFX()
    {
        if(isGrounded && Mathf.Abs(currentCarLocalVelocity.x) > minSideSkidVelocity && carVelocityRatio > 0)
        {
            ToggleSkidMarks(true);
            ToggleSkidSmokes(true);
            ToggleSkidSound(true);
        }
        else
        {
            ToggleSkidMarks(false);
            ToggleSkidSmokes(false);
            ToggleSkidSound(false);
        }
    }

    private void ToggleSkidMarks(bool toggle)
    {
        foreach (var skidMark in skidMarks)
        {
            skidMark.emitting = toggle;
        }
    }

    private void ToggleSkidSmokes(bool toggle)
    {
        foreach (var skidSmoke in skidSmokes)
        {
            if(toggle)
            {
                skidSmoke.Play();
            }
            else
            {
                skidSmoke.Stop();
            }
        }
    }
    #endregion

    #region Movement
    private void Movement()
    {
        if(isGrounded)
        {
            Acceleration();
            Deceleration();
            Turn();
            SidewayDrag();
        }
    }
    private void Acceleration()
    {
        if(currentCarLocalVelocity.z < maxSpeed)
        {
            carRB.AddForceAtPosition(acceleration * moveInput * transform.forward, accelerationPoint.position, ForceMode.Acceleration);
        }
        else
        {
            Debug.Log(currentCarLocalVelocity.z);
        }
    }

    private void Deceleration()
    {
        carRB.AddForce((Input.GetKey(KeyCode.Space) ? brakingDeceleration : deceleration) * carVelocityRatio * -carRB.transform.forward, ForceMode.Acceleration);
    }
    private void Turn()
    {
        carRB.AddRelativeTorque(steerStrength * steerInput * turningCurve.Evaluate(Mathf.Abs(carVelocityRatio)) * Mathf.Sign(carVelocityRatio) * carRB.transform.up, ForceMode.Acceleration);

    }

    private void SidewayDrag()
    {
        float currentSidewaySpeed = currentCarLocalVelocity.x;
        float dragForceMagnitude = -currentSidewaySpeed * (Input.GetKey(KeyCode.Space) ? brakingDragCoefficient : dragCoeffecient);

        Vector3 dragForce = transform.right * dragForceMagnitude;
        carRB.AddForceAtPosition(dragForce, carRB.worldCenterOfMass, ForceMode.Acceleration);
    }
    #endregion

    #region Car Status Check
    private void GroundCheck()
    {
        int tempGroundedWheels = 0;
        for (int i = 0; i < wheelsGrounded.Length; i++)
        {
            tempGroundedWheels += wheelsGrounded[i];
        }
        if(tempGroundedWheels > 1)
        {
            isGrounded = true;
        } 
        else
        {
            isGrounded = false;
        }
    }

    private void CalculateCarVelocity()
    {
        currentCarLocalVelocity = transform.InverseTransformDirection(carRB.velocity);
        carVelocityRatio = currentCarLocalVelocity.z / maxSpeed;
    }
    #endregion

    #region Suspension
    private void Suspension()
    {
        for (int i = 0;i < rayPoints.Length; i++)
        {
            RaycastHit hit;
            float maxLength = restLength + springTravel;

            if (Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, maxLength + wheelRadius, drivable))
            {
                wheelsGrounded[i] = 1;
                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = (restLength - currentSpringLength) / springTravel;

                float springVelocity = Vector3.Dot(carRB.GetPointVelocity(rayPoints[i].position), rayPoints[i].up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springStiffness * springCompression;

                float netForce = springForce - dampForce;

                carRB.AddForceAtPosition(netForce * rayPoints[i].up, rayPoints[i].position);

                // Visuals
                SetTirePosition(tires[i], hit.point + rayPoints[i].up * wheelRadius);

                Debug.DrawLine(rayPoints[i].position, hit.point, Color.red);

            }
            else
            {
                wheelsGrounded[i] = 0;

                // Visuals
                SetTirePosition(tires[i], rayPoints[i].position - rayPoints[i].up * maxLength);

                Debug.DrawLine(rayPoints[i].position, rayPoints[i].position + (wheelRadius + maxLength) * -rayPoints[i].up, Color.green);
            }
        }
    }
    #endregion
}
