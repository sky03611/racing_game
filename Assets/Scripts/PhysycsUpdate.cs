﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysycsUpdate : MonoBehaviour
{
    private float deltaTime;
    private CarEngine ce;
    private CarTransmission ct;
    private CarController cc;
    private WheelEngineSync wes;
    private float driveTorque;
    private float wheelangularVelComb;
    private RayCastWheel[] wheels;
    private float throttle;

    void Awake()
    {
        GetComponents();
        InitializeComponents();
    }

    void GetComponents()
    {
        wheels = GetComponentsInChildren<RayCastWheel>();
        ce = GetComponent<CarEngine>();
        ct = GetComponent<CarTransmission>();
    }

    void InitializeComponents()
    {
        ce.Initialize();
        ct.Initialize();
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].Initialize();
        }
    }


    void FixedUpdate()
    {
        GetGlobalVariables();
        
        UpdatePhysics();
    }

    void GetGlobalVariables()
    {
        deltaTime = Time.fixedDeltaTime;
        throttle = Input.GetAxisRaw("Vertical");
    }

    void UpdatePhysics()
    {
        Shifter();
        
        ce.UpdatePhysics(deltaTime, throttle); ;
        driveTorque = ce.GetEngineTorque() * ct.GetTotalGearRatio();
        //driveTorque = ct.GetTrasmissionTorque();
        for (int i = 0; i < wheels.Length - 2; i++)
        {
            wheels[i].UpdatePhysics(deltaTime, 0);
            wheels[i].WheelRolling(deltaTime);
        }
        for (int i = 2; i < wheels.Length; i++)
        {
            wheels[i].UpdatePhysics(deltaTime, driveTorque);
            wheels[i].WheelRolling(deltaTime);
        }
       
    }

    private void Shifter()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ct.GearUp();
            
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            ct.GearDown();
        }
    }

   
}
