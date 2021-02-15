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
    public float throtleValue;
    private float torquePower;
    private float engineTorqueValue;
    private float engineAngularVelocity;
    private float rpmToRadsSec;
    private float radsSecToRpm;
    private float transmissionEngineTorque;
    

    //DashBoard
    private Dashboard dashBoard;
    private CarAudio ca;
    private CarTransmission ct;

    void Start()
    {
        dashBoard = GetComponent<Dashboard>();
        dashBoard.SetEngineMaxRpm(engineMaxRpm);
        ca = GetComponent<CarAudio>();
        ct = GetComponent<CarTransmission>();
        rpmToRadsSec = Mathf.PI * 2 / 60;
        radsSecToRpm = 1 / rpmToRadsSec;
        engineRpm = engineIdleRpm; // на всякий случай
    }

    void FixedUpdate()
    { 
        torquePower = engineTorqueCurve.Evaluate(engineRpm); 
        engineTorqueValue = Mathf.Lerp(backTorque, engineRpm, throtleValue) / inertia * Time.deltaTime; // Суммарный крутящий угловой момент radsSec EAC
        engineAngularVelocity = Mathf.Clamp(engineTorqueValue + engineAngularVelocity, engineIdleRpm * rpmToRadsSec, engineMaxRpm * rpmToRadsSec);
        engineRpm = engineAngularVelocity * radsSecToRpm;
        dashBoard.SetEngineRpm(engineRpm); // Send Rpm to dashboard
        ca.SetEngingeRpm(engineRpm); // send Rpm to audio script
        transmissionEngineTorque = ct.TotalGearRatio() * torquePower;

    }

    public float GetEngineTransTorque()
    {
        return transmissionEngineTorque;
    }

    public float GetEngineTorque()
    {
        return transmissionEngineTorque;
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
