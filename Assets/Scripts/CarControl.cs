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

    internal enum CarType
    {
        manual,
        automatic
    }

    public WheelCollider[] wheels = new WheelCollider[4];
    public GameObject[] wheelMesh = new GameObject[4];
    public GameObject centerOfMass;
    public float wheelRadius = 0.47f;
    public float steeringMax;
    private Rigidbody carRB;
    public float speedKmh;

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
    public float[] gears;
    public int currentGear = 0;

    public bool reverse = false;

    [SerializeField] private DriveType driveMode;
    [SerializeField] private CarType carMode;

    private WheelFrictionCurve forwardFriction;
    private WheelFrictionCurve sidewaysFriction;
    public float handBrakeFrictionMultiplier = 1.7f;
    public float handBrakeFriction = 0f;
    public float brakePower = 0;
    public float slowmoTimeScale = 1f;
    public TrailRenderer[] skidMarks = new TrailRenderer[2];
    public ParticleSystem[] skidSmokes = new ParticleSystem[2];
    private float driftFactor;



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
        //Debug.Log("WheelRPM: " + wheelsRPM + "||| Engine RPM:" + engineRPM + "|||| Speed:" + speedKmh);
    }

    private void Update()
    {
        GearShift();
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            Debug.Log("SLOWMO");
        } 

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
            if (toggle)
            {
                skidSmoke.Play();
            }
            else
            {
                skidSmoke.Stop();
            }
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

        int previousGear = currentGear;

        //manual
        if (carMode == CarType.manual)
        {
            if (Input.GetKeyDown(KeyCode.E) && currentGear < gears.Length - 1)
            {
                currentGear++;
                reverse = false;
            }
            else if (Input.GetKeyDown(KeyCode.Q) && currentGear >= 0)
            {
                if(currentGear == 0)
                {
                    reverse = true;
                }
                else
                {
                    currentGear--;
                }
            }
        }
        //automatic
        else
        {
            if (engineRPM >= maxRPM / gears[currentGear] && currentGear < gears.Length - 1)
            {
                currentGear++;
            }
            else if (engineRPM < maxRPM / (gears[currentGear] * 0.8f) && currentGear > 0)
            {
                currentGear--;
            }
        }
        //gear down speed loss
        if (currentGear < previousGear && speedKmh > 50f) 
        {
            float wheelSpeed = speedKmh / MPH_TO_KMH;
            engineRPM = Mathf.Clamp(wheelSpeed * gears[currentGear] * 60f / (Mathf.PI * wheelRadius * 2f), 1000f, maxRPM);

            float currentGearRatio = gears[currentGear];
            float previousGearRatio = gears[previousGear];
            float adjustmentFactor = previousGearRatio / currentGearRatio;
            carRB.velocity *= adjustmentFactor;
        }
    }

    private void CalculateEnginePower()
    {
        WheelRPM();

        if (verticalInput != 0)
        {
            carRB.drag = 0.005f;
        }
        if (verticalInput == 0)
        {
            carRB.drag = 0.1f;
        }

        float rawEnginePower = enginePower.Evaluate(engineRPM);

        float maxRPMForGear = maxRPM / gears[currentGear];
        engineRPM = Mathf.Clamp(engineRPM, 1000f, maxRPMForGear);

        totalPower = rawEnginePower * gears[currentGear] * verticalInput;

        float velocity = 0.0f;
        float targetRPM = 1000 + (Mathf.Abs(wheelsRPM) * gears[currentGear] * MPH_TO_KMH);
        engineRPM = Mathf.SmoothDamp(engineRPM, targetRPM, ref velocity, 0.01f);
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
        if(engineRPM < maxRPM)
        {
            wheelsRPM = (r != 0) ? sum / r : 0;
        }

        if (wheelsRPM < 0 && !reverse && currentGear == 0)
        {
            reverse = true;
        }
        else if(wheelsRPM > 0 && reverse)
        {
            reverse = false;
        }
    }

    private void BrakeVehicle()
    {

        if (verticalInput < 0)
        {
            brakePower = (speedKmh >= 10) ? 500 : 0;
        }
        else if (verticalInput == 0 && (speedKmh <= 10 || speedKmh >= -10))
        {
            brakePower = 10;
        }
        else
        {
            brakePower = 0;
        }


    }

    private void MoveVehicle()
    {
        BrakeVehicle();

        float appliedInput = reverse || verticalInput >= 0 ? verticalInput : 0;

        float dragForce = 0.5f * carRB.velocity.magnitude * carRB.velocity.magnitude;
        carRB.AddForce(-carRB.velocity.normalized * dragForce);

        if (driveMode == DriveType.allWheelDrive)
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].motorTorque = (appliedInput * Mathf.Abs(totalPower / 4)) * slowmoTimeScale;
                wheels[i].brakeTorque = brakePower;
            }
        }
        else if (driveMode == DriveType.rearWheelDrive)
        {
            for (int i = 2; i < wheels.Length; i++)
            {
                wheels[i].motorTorque = appliedInput * Mathf.Abs(totalPower / 2);
            }
            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].brakeTorque = brakePower;
            }
        }
        else
        {
            for (int i = 0; i < wheels.Length - 2; i++)
            {
                wheels[i].motorTorque = appliedInput * Mathf.Abs(totalPower / 2);
            }
            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].brakeTorque = brakePower;
            }
        }

        if (boostInput)
        {
            carRB.AddForce(Vector3.forward * thrust);
        }

        speedKmh = (carRB.velocity.magnitude * MPH_TO_KMH);
    }


    private void SteerVehicle()
    {
        float steeringAngle =  steeringMax * horizontalInput;
        for(int i = 0; i < wheels.Length / 2; i++)
        {
            wheels[i].steerAngle = steeringAngle;
        }    
    }



    private void ApplyDownForce()
    {
        carRB.AddForce(-transform.up * downForceValue * carRB.velocity.magnitude);
    }


    private void AdjustTraction()
    {
        float driftSmoothFactor = 0.7f * Time.deltaTime ;

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
            ToggleSkidMarks(true);
            //ToggleSkidSmokes(true);
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
            ToggleSkidMarks(false);
            //ToggleSkidSmokes(false);
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