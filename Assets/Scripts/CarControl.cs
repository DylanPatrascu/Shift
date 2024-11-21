using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;

public class CarControl : MonoBehaviour
{
    private const float MPH_TO_KMH = 3.6f;

    internal enum DriveType
    {
        frontWheelDrive,
        rearWheelDrive,
        allWheelDrive
    }


    public WheelCollider[] wheels = new WheelCollider[4];
    public GameObject[] wheelMesh = new GameObject[4];
    public GameObject centerOfMass;
    public float wheelRadius = 0.47f;
    public int motorTorque = 2000;
    public int brakeTorque = 90000; 
    public float steeringMax = 40f;
    private Rigidbody carRB;
    public float speedKmh;
    public float maxSpeedKmh = 260f;

    public float downForceValue = 50f;

    private float verticalInput;
    private float horizontalInput;
    private bool handbrakeInput;
    private bool boostInput;
    public float thrust = 1000f;

    public float totalPower;
    public AnimationCurve enginePower;
    public float wheelsRPM;

    public float engineRPM;
    public float maxRPM = 9000f;
    public float smoothTime = 0.01f;
    public float[] gears;
    public int currentGear = 0;

    public bool reverse = false;

    [SerializeField] private DriveType driveMode;

    private WheelFrictionCurve forwardFriction;
    private WheelFrictionCurve sidewaysFriction;
    public float handBrakeFrictionMultiplier = 1.7f;
    public float handBrakeFriction = 0f;

    private void Start()
    {
        GetObjects();
    }

    private void FixedUpdate()
    {
        UserInput();
        AnimateWheels();
        MoveVehicle();
        SteerVehicle();
        ApplyDownForce();
        CalculateEnginePower();
        AdjustTraction();
        Debug.Log("WheelRPM: " + wheelsRPM + "||| Engine RPM:" + engineRPM + "|||| Speed:" + speedKmh);
    }

    private void Update()
    {
        GearShift();

    }

    private void UserInput()
    {
        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
        handbrakeInput = (Input.GetAxis("Jump") != 0);

        if (Input.GetKey(KeyCode.LeftControl))
        {
            boostInput = true;
        } 
        else
        {
            boostInput = false;
        }

    }


    private bool IsGrounded()
    {
        for(int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i].isGrounded == false)
            {
                return false;
            }
        }
        return true;
    }

    private void GearShift()
    {
        if (!IsGrounded()) return;
        
        if (Input.GetKeyDown(KeyCode.E) && currentGear < gears.Length - 1)
        {
            currentGear++;
        }
        else if ((Input.GetKeyDown(KeyCode.Q) && currentGear >= 1))
        {
            currentGear--;
        }

    }

    private void CalculateEnginePower()
    {
        // Update the wheels' RPM
        WheelRPM();

        if (verticalInput != 0)
        {
            carRB.drag = 0.005f;
        }
        if (verticalInput == 0)
        {
            carRB.drag = 0.1f;
        }
        // Evaluate engine power from the curve using engine RPM
        float rawEnginePower = enginePower.Evaluate(engineRPM);

        // Calculate total power with current gear and input
        totalPower = rawEnginePower * gears[currentGear] * verticalInput;

        // Smoothly calculate engine RPM based on wheels RPM and gear ratio
        float velocity = 0.0f;
        float targetRPM = 1000 + (Mathf.Abs(wheelsRPM) * gears[currentGear] * MPH_TO_KMH);
        engineRPM = Mathf.SmoothDamp(engineRPM, targetRPM, ref velocity, smoothTime);
    }

    private void WheelRPM()
    {
        float sum = 0;
        int r = 0;
        for (int i = 0; i < 4; i++)
        {
            sum += wheels[i].rpm;
            r++;
        }
        wheelsRPM = (r != 0) ? sum / r : 0;

        if(wheelsRPM < 0 && !reverse)
        {
            reverse = true;
        }
        else if(wheelsRPM > 0 && reverse)
        {
            reverse = false;
        }
    }
    
    private void MoveVehicle()
    {

        if (driveMode == DriveType.allWheelDrive)
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].motorTorque = verticalInput * Mathf.Abs(totalPower / 4);
            }
        }
        else if (driveMode == DriveType.rearWheelDrive)
        {
            for (int i = 2; i < wheels.Length; i++)
            {
                wheels[i].motorTorque = verticalInput * Mathf.Abs(totalPower / 2);
            }
        }
        else
        {
            for (int i = 0; i < wheels.Length - 2; i++)
            {
                wheels[i].motorTorque = verticalInput * Mathf.Abs(totalPower / 2);
            }
        }

        if(boostInput)
        {
            carRB.AddForce(Vector3.forward * thrust);
        }

        speedKmh = carRB.velocity.magnitude * MPH_TO_KMH;
    }

    private void SteerVehicle()
    {
        float wheelbase = 2.55f; // Length between front and rear axles
        float trackWidth = 1.5f; // Distance between left and right wheels

        if (Mathf.Abs(horizontalInput) > 0.01f) // Prevent division by zero for very small input
        {
            float turningRadius = Mathf.Abs(1 / horizontalInput); // Simplified radius

            float innerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turningRadius + (trackWidth / 2)));
            float outerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turningRadius - (trackWidth / 2)));

            // Clamp angles to maximum steering angle
            innerAngle = Mathf.Clamp(innerAngle, -steeringMax, steeringMax);
            outerAngle = Mathf.Clamp(outerAngle, -steeringMax, steeringMax);

            if (horizontalInput > 0) // Turning right
            {
                wheels[0].steerAngle = outerAngle; // Front-left wheel
                wheels[1].steerAngle = innerAngle; // Front-right wheel
            }
            else if (horizontalInput < 0) // Turning left
            {
                wheels[0].steerAngle = -innerAngle; // Front-left wheel
                wheels[1].steerAngle = -outerAngle; // Front-right wheel
            }
        }
        else
        {
            wheels[0].steerAngle = 0;
            wheels[1].steerAngle = 0;
        }
    }

    private void ApplyDownForce()
    {
        carRB.AddForce(-transform.up * downForceValue * carRB.velocity.magnitude);
    }


    private float driftFactor;
    private void AdjustTraction()
    {
        float driftSmoothFactor = 0.7f * Time.deltaTime;

        if (handbrakeInput)
        {
            sidewaysFriction = wheels[0].sidewaysFriction;
            forwardFriction = wheels[0].forwardFriction;

            float velocity = 0;
            forwardFriction.extremumValue = forwardFriction.asymptoteValue = sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = Mathf.SmoothDamp(forwardFriction.asymptoteValue, driftFactor * handBrakeFrictionMultiplier, ref velocity, driftSmoothFactor);

            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].forwardFriction = forwardFriction;
                wheels[i].sidewaysFriction = sidewaysFriction;

            }
            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = forwardFriction.extremumValue = forwardFriction.asymptoteValue = 1.1f;

            for (int i = 0; i < 2; i++)
            {
                wheels[i].sidewaysFriction = sidewaysFriction;
                wheels[i].forwardFriction = forwardFriction;
            }
            carRB.AddForce(transform.forward * (speedKmh / 400) * 10000);
        }
        else
        {
            forwardFriction = wheels[0].forwardFriction;
            sidewaysFriction = wheels[0].sidewaysFriction;

            forwardFriction.extremumValue = forwardFriction.asymptoteValue = sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = ((speedKmh * handBrakeFrictionMultiplier) / 300) + 1;

            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].forwardFriction = forwardFriction;
                wheels[i].sidewaysFriction = sidewaysFriction;

            }
        }

        for (int i = 2; i < wheels.Length; i++)
        {
            WheelHit wheelHit;
            wheels[i].GetGroundHit(out wheelHit);
            if (wheelHit.sidewaysSlip < 0)
            {
                driftFactor = (1 + -horizontalInput) * Mathf.Abs(wheelHit.sidewaysSlip);
            }
            if (wheelHit.sidewaysSlip > 0)
            {
                driftFactor = (1 + horizontalInput) * Mathf.Abs(wheelHit.sidewaysSlip);

            }
        }
    }


    private void AnimateWheels()
    {
        Vector3 wheelPosition = Vector3.zero;
        Quaternion wheelRotation = Quaternion.identity;

        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].GetWorldPose(out wheelPosition, out wheelRotation);
            wheelMesh[i].transform.position = wheelPosition;
            wheelMesh[i].transform.rotation = wheelRotation;

        }
    }

    private void GetObjects()
    {
        carRB = GetComponent<Rigidbody>();
        carRB.centerOfMass = centerOfMass.transform.localPosition;
    }
}