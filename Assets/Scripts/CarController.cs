using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public Vector3 centerOfMass;
    public AnimationCurve steerFactorCurve;
    public AnimationCurve backSteerFactorCurve;
    private Rigidbody rb;
    public float steeringAngle;
    private float steerInputs;
    public RayCastWheel[] rayCastWheels;
    public float wheelBase;
    public float turnRadius;
    public float rearTrack;
    public float steerFactor;
    public float steerBackTime;
    private float ackermannAngleLeft;
    private float ackermannAngleRight;
    private float driveTypeInt;
    private CarEngine ce;
    private CarTransmission ct;
    private int driveTypeDivider;

    public float lerptester;

    internal enum driveType
    {
        awd,
        fwd,
        rwd
    };
    [SerializeField]
    private driveType wheelDrive = driveType.awd;
    public void Initialize()
    {
        WheelDriveType();
        ct = GetComponent<CarTransmission>();
        rearTrack = rearTrack / 2;
        CenterOfMassCorrector();
    }

    private void CenterOfMassCorrector()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;
        ce = GetComponent<CarEngine>();
        
    }

    private void WheelDriveType()
    {
        if (wheelDrive == driveType.awd)
        {
            driveTypeDivider = 4;
        }
        if (wheelDrive == driveType.fwd)
        {
            driveTypeDivider = 2;
        }
        if(wheelDrive == driveType.rwd)
        {
            driveTypeDivider = 2;
        }
    }

    public void SteerFactorChanger()
    {
        steerFactor = steerFactorCurve.Evaluate(rb.velocity.magnitude*3.6f); //todo Evaluate+steerBackTime 
    }
    public int GetDriveTypeDivider()
    {
        return driveTypeDivider;
    }

    public float DriveTypeInt()
    {
        if (wheelDrive == driveType.awd)
        {
            driveTypeInt = 0;
        }
        else if(wheelDrive == driveType.fwd)
        {
            driveTypeInt = 1;
        }
        else
        {
            driveTypeInt = 2;
        }

        return driveTypeInt;
    }

    // Update is called once per frame
    void Update()
    {
        Physics.gravity = new Vector3(0, lerptester, 0);
        steerInputs = Input.GetAxisRaw("Horizontal");
        if (Input.GetAxis("Horizontal") > 0)
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrack)) * steerInputs * steerFactor;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrack)) * steerInputs * steerFactor;
        }
        else if (Input.GetAxis("Horizontal") < 0)
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrack)) * steerInputs * steerFactor;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrack)) * steerInputs * steerFactor;
        }
        else
        {
            ackermannAngleLeft = 0;
            ackermannAngleRight = 0;
        }
        

        foreach (RayCastWheel wheel in rayCastWheels)
        {
            if (wheel.wheelFL)
                wheel.steerAngle = ackermannAngleLeft;
            else if (wheel.wheelFR)
                wheel.steerAngle = ackermannAngleRight;
            else
                wheel.steerAngle = 0;


        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + transform.rotation * centerOfMass, 0.5f);
    }

}
