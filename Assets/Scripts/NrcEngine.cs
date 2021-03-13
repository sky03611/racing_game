using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NrcEngine : MonoBehaviour
{
    public AnimationCurve engineTorqueCurve;
    private NrcTransmission nrcTransmission;
    private NrcDashBoard nrcDashBoard;
    private float throttle;
    private float engineRpm;
    private float engineAngularAcc;
    public float engineAngularVelocity;
    private float torquePower;
    private float rpmToRadsSec;
    private float radsSecToRpm;

    public float backTorque;
    public float inertia;
    public float engineMaxRpm;
    public float engineIdleRpm;
    
    
    public void Initialize()
    {
        nrcTransmission = GetComponent<NrcTransmission>();
        nrcDashBoard = GetComponent<NrcDashBoard>();
        rpmToRadsSec = Mathf.PI * 2 / 60;
        radsSecToRpm = 1 / rpmToRadsSec;
        engineRpm = engineIdleRpm; 
    }

    // Update is called once per frame
    public void PhysicsUpdate(float throttleInput, float delta)
    {
        EngineUpdate(throttleInput, delta);
        nrcDashBoard.SetEngineRpm(engineRpm);
        nrcDashBoard.SetEngineMaxRpm(engineMaxRpm);
    }

    private void EngineUpdate(float throttleInput, float delta)
    {
        throttle = throttleInput;
        torquePower = Mathf.Lerp(backTorque, engineTorqueCurve.Evaluate(engineRpm) * throttle, throttle);
        engineAngularAcc = torquePower / inertia;
        engineAngularVelocity += engineAngularAcc * delta;
        //engineAngularVelocity = Mathf.Clamp(((Mathf.Abs(averageWheelSpeed) * ct.GetTotalGearRatio() - engineAngularVelocity) * clutch) + engineAngularVelocity, engineIdleRpm * rpmToRadsSec, engineMaxRpm * rpmToRadsSec);
        engineAngularVelocity = Mathf.Clamp(engineAngularVelocity, engineIdleRpm * rpmToRadsSec, engineMaxRpm * rpmToRadsSec);
        engineRpm = engineAngularVelocity * radsSecToRpm;

        nrcTransmission.TransmissionTorque(Mathf.Clamp(torquePower,0,20000f));
    }

}
