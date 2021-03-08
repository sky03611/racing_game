using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NrcEngine : MonoBehaviour
{
    public AnimationCurve engineTorqueCurve;
    private NrcTransmission nrcTransmission;
    private float throttle;
    private float engineRpm;
    private float engineAngularAcc;
    private float engineAngularVelocity;
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
        rpmToRadsSec = Mathf.PI * 2 / 60;
        radsSecToRpm = 1 / rpmToRadsSec;
        engineRpm = engineIdleRpm; 
    }

    // Update is called once per frame
    public void PhysicsUpdate(float throttleInput, float delta)
    {
        throttle = throttleInput;
        torquePower = Mathf.Lerp(backTorque, engineTorqueCurve.Evaluate(engineRpm) * throttle, throttle);
        if (engineRpm == engineIdleRpm && torquePower<0)
        {
            torquePower = 0;
        }
        engineAngularAcc = torquePower / inertia;
        engineAngularVelocity += engineAngularAcc * delta;
        //engineAngularVelocity = Mathf.Clamp(((Mathf.Abs(averageWheelSpeed) * ct.GetTotalGearRatio() - engineAngularVelocity) * clutch) + engineAngularVelocity, engineIdleRpm * rpmToRadsSec, engineMaxRpm * rpmToRadsSec);
        engineAngularVelocity = Mathf.Clamp(engineAngularVelocity, engineIdleRpm * rpmToRadsSec, engineMaxRpm * rpmToRadsSec);
        engineRpm = engineAngularVelocity * radsSecToRpm;
        Debug.Log("Torque= " + torquePower);
        Debug.Log("Rpm= " + engineRpm);
        nrcTransmission.TransmissionTorque(torquePower);
    }

    
}
