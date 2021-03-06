using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour
{
    public AnimationCurve engineTorqueCurve;
    private float engineRpm;
    public float engineMaxRpm = 7000f;
    public float engineIdleRpm = 600f;
    public float inertia = 0.3f;
    public float backTorque = -100f;
    public float throttle;
    private float torquePower;
    private float engineAngularVelocity;
    private float rpmToRadsSec;
    private float radsSecToRpm;
    private float engineAngularAcc;
    public float clutch;
    public bool clutchEng;
    private float averageWheelSpeed;

    private CarAudio ca;
    private CarTransmission ct;
    private CarController cc;
    private Rigidbody rb;
    
    void Update()
    {
        AutoClutch();
    }
    public void Initialize()
    {
        ct = GetComponent<CarTransmission>();
        cc = GetComponent<CarController>();
        ca = GetComponent<CarAudio>();
        rb = GetComponent<Rigidbody>();
        rpmToRadsSec = Mathf.PI * 2 / 60;
        radsSecToRpm = 1 / rpmToRadsSec;
        engineRpm = engineIdleRpm; // 
    }

    public void UpdatePhysics(float delta, float var)
    {
        throttle = var;
        torquePower = Mathf.Lerp(backTorque, engineTorqueCurve.Evaluate(engineRpm) * throttle, throttle);
        engineAngularAcc = torquePower / inertia;
        engineAngularVelocity += engineAngularAcc * delta;
        if (ct.GetCurrentGear() != 1)
        {
            engineAngularVelocity = Mathf.Clamp(((Mathf.Abs(averageWheelSpeed) * ct.GetTotalGearRatio() - engineAngularVelocity) * clutch) + engineAngularVelocity, engineIdleRpm * rpmToRadsSec, engineMaxRpm * rpmToRadsSec);
        }
        else
        {
            torquePower = Mathf.Lerp(backTorque * 5, engineTorqueCurve.Evaluate(engineRpm) * throttle, throttle);
            engineAngularAcc = torquePower / inertia;
            engineAngularVelocity += engineAngularAcc * delta;
            engineAngularVelocity = Mathf.Clamp(engineAngularVelocity, engineIdleRpm * rpmToRadsSec, engineMaxRpm * rpmToRadsSec);
        }
        //engineAngularVelocity = Mathf.Clamp(engineAngularVelocity, engineIdleRpm * rpmToRadsSec, engineMaxRpm * rpmToRadsSec);
        engineRpm = engineAngularVelocity * radsSecToRpm;
        ca.SetEngingeRpm(engineRpm);
        
    }

    public void AutoClutch()
    {
        if (Input.GetKey(KeyCode.X))
        {
            clutchEng = true;
            clutch = 0.000001f;
        }
        else
        {
            clutchEng = false;
            float speedForward = Mathf.Abs(Vector3.Dot(rb.velocity, rb.transform.forward));
            clutch = Mathf.Lerp(0.1f, 0.6f, speedForward / 10); 
        }
    }

    public void AverageWheelSpeed()
    {
        if (cc.DriveTypeInt() == 0)
        {
            averageWheelSpeed = 0;
            for (int i = 0; i < cc.rayCastWheels.Length; i++)
            {
                averageWheelSpeed += cc.rayCastWheels[i].GetWheelAngularVelocity() / cc.GetDriveTypeDivider();
            }
        }
        if (cc.DriveTypeInt() == 1)
        {
            averageWheelSpeed = 0;
            for (int i = 0; i < cc.rayCastWheels.Length-2; i++)
            {
                averageWheelSpeed += cc.rayCastWheels[i].GetWheelAngularVelocity() / cc.GetDriveTypeDivider();
            }
        }
        if (cc.DriveTypeInt() == 2)
        {
            averageWheelSpeed = 0;
            for (int i = 2; i < cc.rayCastWheels.Length; i++)
            {
                averageWheelSpeed += cc.rayCastWheels[i].GetWheelAngularVelocity() / cc.GetDriveTypeDivider();
            }
        }
    }

    public float GetEngineTorque()
    {
        return torquePower;
    }

    public float GetEngineRPM()
    {
        return engineRpm;
    }

    public float EngineAngularVelocity()
    {
        return engineAngularVelocity;
    }
}