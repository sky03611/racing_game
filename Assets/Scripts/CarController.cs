﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public Vector3 centerOfMass;
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

    internal enum driveType
    {
        awd,
        fwd,
        rwd
    };
    [SerializeField]
    private driveType wheelDrive = driveType.awd;
    void Start()
    {
        ct = GetComponent<CarTransmission>();
        rearTrack = rearTrack / 2;
        WheelDriveType();
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
            for (int i = 0; i < rayCastWheels.Length; i++)
            {
                rayCastWheels[i].SetDriveType();
                ct.DriveTypeForce(1f);
            }
        }
        else if (wheelDrive == driveType.fwd)
        {
            for (int i = 0; i < rayCastWheels.Length - 2; i++)
            {
                rayCastWheels[i].SetDriveType();
                ct.DriveTypeForce(1f);
            }
        }

        else if (wheelDrive == driveType.rwd)
        {
            for (int i = 2; i < rayCastWheels.Length; i++)
            {
                rayCastWheels[i].SetDriveType();
                ct.DriveTypeForce(1f);
            }
        }
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
        ce.throtleValue = Input.GetAxis("Vertical");
        steerInputs = Input.GetAxis("Horizontal");
        if (Input.GetAxis("Horizontal") > 0)
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrack)) * Input.GetAxis("Horizontal") * steerFactor;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrack)) * Input.GetAxis("Horizontal") * steerFactor;
        }
        else if (Input.GetAxis("Horizontal") < 0)
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrack)) * Input.GetAxis("Horizontal") * steerFactor;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrack)) * Input.GetAxis("Horizontal") * steerFactor;
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
