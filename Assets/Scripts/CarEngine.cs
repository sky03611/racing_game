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
    private float engineTorqueValue;
    private float engineAngularVelocity;
    private float rpmToRadsSec;
    private float radsSecToRpm;
    private float engineAngularAcc;
    private float totalDriveAxisAngularVelocity = 0f;
    private float clutchAngularVelocity = 0f;



    //DashBoard
    private Dashboard dashBoard;
    private CarAudio ca;
    private CarTransmission ct;
    private CarController cc;


    public void Initialize()
    {
        
       // dashBoard = GetComponent<Dashboard>();
       // dashBoard.SetEngineMaxRpm(engineMaxRpm);
       // ca = GetComponent<CarAudio>();
        ct = GetComponent<CarTransmission>();
        cc = GetComponent<CarController>();
        rpmToRadsSec = Mathf.PI * 2 / 60;
        radsSecToRpm = 1 / rpmToRadsSec;
        engineRpm = engineIdleRpm; // 
    }


    public void UpdatePhysics(float delta, float var)
    {
        throttle = var;
        torquePower = Mathf.Lerp(backTorque, engineTorqueCurve.Evaluate(engineRpm) * throttle, throttle);
        engineAngularAcc = torquePower/ inertia; 
        engineAngularVelocity += engineAngularAcc  * delta;
        engineAngularVelocity = Mathf.Clamp(engineAngularVelocity, engineIdleRpm * rpmToRadsSec, engineMaxRpm * rpmToRadsSec);
        engineRpm = engineAngularVelocity * radsSecToRpm;
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
