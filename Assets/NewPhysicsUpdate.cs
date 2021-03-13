using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPhysicsUpdate : MonoBehaviour
{
    private float delta;
    private float[] steerAngle = new float[2];
    private Rigidbody rb;
    private NrcSteering nrcSteering;
    private NrcEngine nrcEngine;
    private NrcTransmission nrcTransmission;
    private NrcDashBoard nrcDashBoard;
    public Vector3 centerOfMass;
    public NewRayCast[] nrc;

    private float inputV;
    private float inputH;


    private float toEngineTorque;
    private float currentGearRatio;
    private float engineAngularVelocity;

    public bool rwd = false;
    public bool fwd = false;
    public bool awd = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        nrcSteering = GetComponent<NrcSteering>();
        nrcEngine = GetComponent<NrcEngine>();
        nrcTransmission = GetComponent<NrcTransmission>();
        nrcDashBoard = GetComponent<NrcDashBoard>();

        nrcSteering.Initialize();
        nrcEngine.Initialize();
        nrcTransmission.Initialize();
        nrcDashBoard.Initialize();
        
        for (int i = 0; i < nrc.Length; i++)
        {
            nrc[i].Initialize();
        }

    }

    void Update()
    {
        Shifter();
    }

    void FixedUpdate()
    {
        delta = Time.fixedDeltaTime;
        inputV = Input.GetAxis("Vertical");
        inputH = Input.GetAxis("Horizontal");
        currentGearRatio = nrcTransmission.currentGearRatio;
        engineAngularVelocity = nrcEngine.engineAngularVelocity;
        Steering();
        CenterOfMassCorrector();
        nrcEngine.PhysicsUpdate(inputV, delta);
        nrcTransmission.PhysicsUpdate();
        toEngineTorque = nrcTransmission.transmissionTorque;
        WheelsUpdater();
    }

    private void WheelsUpdater()
    {
        if(awd)
        AllWheelDrive();
        if(fwd)
        FrontWheelDrive();
        if(rwd)
        RearWheelDrive();
    }

    private void RearWheelDrive()
    {
        for (int i = 0; i < nrc.Length-2; i++)
        {
            
            nrc[i].UpdatePhysics(delta, 0, steerAngle[0], steerAngle[1],currentGearRatio, engineAngularVelocity); //TODO Optimize
        }

        for (int i = 2; i < nrc.Length; i++)
        {
            nrc[i].UpdatePhysics(delta, toEngineTorque/2, steerAngle[0], steerAngle[1], currentGearRatio, engineAngularVelocity); //TODO Optimize
        }
    }

    private void FrontWheelDrive()
    {
        for (int i = 0; i < nrc.Length-2; i++)
        {
            nrc[i].UpdatePhysics(delta, toEngineTorque/2, steerAngle[0], steerAngle[1], currentGearRatio, engineAngularVelocity); //TODO Optimize
        }

        for (int i = 2; i < nrc.Length ; i++)
        {
            nrc[i].UpdatePhysics(delta, 0, steerAngle[0], steerAngle[1], currentGearRatio, engineAngularVelocity); //TODO Optimize
        }
    }

    private void AllWheelDrive()
    {
        for (int i = 0; i < nrc.Length; i++)
        {
            nrc[i].UpdatePhysics(delta, toEngineTorque/4, steerAngle[0], steerAngle[1], currentGearRatio, engineAngularVelocity); //TODO Optimize
        }
    }

    private void CenterOfMassCorrector()
    {
        ;
        rb.centerOfMass = centerOfMass;
       
    }

    public void Steering()
    {
        nrcSteering.PhysicsUpdate(inputH);
        steerAngle[0] = nrcSteering.ackermannAngleLeft;
        steerAngle[1] = nrcSteering.ackermannAngleRight;
    }

    private void Shifter()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            nrcTransmission.GearUp();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            nrcTransmission.GearDown();
        }
    }
}
