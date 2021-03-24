using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NrcPhysics : MonoBehaviour
{
    private CarGlobalSettings carGlobalSettings;
    private CarController carController;
    private NrcEngine nrcEngine;
    private NrcTransmission nrcTransmission;
    private NrcClutch nrcClutch;
    private NrcDifferential nrcDifferential;

    private Dashboard dashboard;
    private float delta;
    private float throttle;
    private float steering;
    private float rpmToRads;
    private float radsToRpm;
    private float loadToruqe;

    private float wrlVel;
    private float wrrVel;
    private float clutch;

    void Start()
    {
        InitializeGlobalVariables();
        InitializeCarComponents();
    }

    void Update() //this is needed to get good controls over keys that change gears
    {
        Shifter();
        if (Input.GetKey(KeyCode.X))
        {
            clutch = 1f;
        }
        else
        {
            clutch = 0f;
        }
    }

    void FixedUpdate()
    {
        GetGlobalUpdatedParams();
        carController.PhysicsUpdate(steering);
        
        nrcTransmission.PhysicsUpdate(nrcClutch.clutchTorque);
        nrcDifferential.PhysicsUpdate(nrcTransmission.outputTorque);
        //UpdateWheels(0f,0f,Input.GetAxis("Vertical") *500, Input.GetAxis("Vertical") *500);
        UpdateWheels(0.001f, 0.001f, nrcDifferential.outputTorqueLeft, nrcDifferential.outputTorqueRight);
        nrcDifferential.GeInputShaftVelocity(wrlVel, wrrVel);
        nrcTransmission.GetInputShaftVelocity(nrcDifferential.differentialVelocity);
        nrcClutch.UpdatePhysics(nrcTransmission.inputShaftVelocity, nrcEngine.engineAngularVelocity, nrcTransmission.currentGearRatio, clutch);
        nrcEngine.PhysicsUpdate(throttle, delta, nrcClutch.clutchTorque);
        
        
        dashboard.PhysicsUpdate(nrcEngine.engineRpm);
        
    }

    private void InitializeGlobalVariables()
    {
        rpmToRads = Mathf.PI * 2 / 60;
        radsToRpm = 1 / rpmToRads;
        carGlobalSettings = GetComponent<CarGlobalSettings>();
        carController = GetComponent<CarController>();
        nrcEngine = GetComponent<NrcEngine>();
        nrcTransmission = GetComponent<NrcTransmission>();
        nrcClutch = GetComponent<NrcClutch>();
        nrcDifferential = GetComponent<NrcDifferential>();
        dashboard = GetComponent<Dashboard>();
    }

    private void InitializeCarComponents()
    {
        for (int i = 0; i < carGlobalSettings.wheels.Length; i++)
        {
            carGlobalSettings.wheels[i].Initialize();
        }
        carController.Initialize();
        nrcEngine.Initialize(rpmToRads, radsToRpm);
        nrcClutch.Initialize(radsToRpm);
        nrcTransmission.Initialize();
        nrcDifferential.Initialize();
        dashboard.Initialize(nrcEngine.maxEngineRpm);

    }
     
    private void GetGlobalUpdatedParams()
    {
        delta = Time.fixedDeltaTime;
        throttle = Input.GetAxisRaw("Vertical"); //TODO switch to new system
        steering = Input.GetAxis("Horizontal");
    }

    private void UpdateWheels(float driveTorqueFL, float driveTorqueFR, float driveTorqueRL, float driveTorqueRR)
    {
        wrlVel = carGlobalSettings.wheels[2].wheelAngularVelocity;
        wrrVel = carGlobalSettings.wheels[3].wheelAngularVelocity;
        for (int i = 0; i < carGlobalSettings.wheels.Length; i++)
        {
            carGlobalSettings.wheels[i].PhysicsUpdate(delta,driveTorqueFL,driveTorqueFR,driveTorqueRL,driveTorqueRR);
            //carGlobalSettings.wheels[i].PhysicsUpdate(delta, driveTorqueFL, driveTorqueFR, driveTorqueRL, driveTorqueRR);
        }

    }

    private void Shifter()
    {
        if (Input.GetKey(KeyCode.P))
        {
            nrcTransmission.GearUp();
        }

        if (Input.GetKey(KeyCode.L))
        {
            nrcTransmission.GearDown();
        }
    }
}
