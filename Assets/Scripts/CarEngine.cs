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

    float deltaTime;
    float baseTorque;
    float toEngine;
    public float clutch;
    float testTorque;
    float testAcc;
    float testVel;
    float averageWheelSpeed;



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
        dashBoard = GetComponent<Dashboard>();
        ca = GetComponent<CarAudio>();
        rpmToRadsSec = Mathf.PI * 2 / 60;
        radsSecToRpm = 1 / rpmToRadsSec;
        engineRpm = engineIdleRpm; // 
    }


    public void UpdatePhysics(float delta, float var)
    {
        
        deltaTime = delta;
        throttle = var;
        torquePower = Mathf.Lerp(backTorque, engineTorqueCurve.Evaluate(engineRpm) * throttle, throttle);
        engineAngularAcc = torquePower / inertia;
        engineAngularVelocity += engineAngularAcc * delta;
        if (ct.GetCurrentGear() != 1)
        {
            engineAngularVelocity = Mathf.Clamp(((averageWheelSpeed * ct.GetTotalGearRatio() - engineAngularVelocity) * clutch) + engineAngularVelocity, engineIdleRpm * rpmToRadsSec, engineMaxRpm * rpmToRadsSec);
        }
        else
        {
            torquePower = Mathf.Lerp(backTorque*5, engineTorqueCurve.Evaluate(engineRpm) * throttle, throttle);
            engineAngularAcc = torquePower / inertia;
            engineAngularVelocity += engineAngularAcc * delta;
            engineAngularVelocity = Mathf.Clamp(engineAngularVelocity, engineIdleRpm * rpmToRadsSec, engineMaxRpm * rpmToRadsSec);
        }
        //engineAngularVelocity = Mathf.Clamp(engineAngularVelocity, engineIdleRpm * rpmToRadsSec, engineMaxRpm * rpmToRadsSec);
        engineRpm = engineAngularVelocity*radsSecToRpm;
        ca.SetEngingeRpm(engineRpm);
        


    }

    public void TestTorque()
    {
            averageWheelSpeed = (cc.rayCastWheels[3].GetWheelAngularVelocity() + cc.rayCastWheels[2].GetWheelAngularVelocity())/ 2;
            testTorque = torquePower;
        
        
    }
    public float ToWheels()
    {
        if (ct.GetTotalGearRatio() != 0)
        {
            var toWheels = torquePower * ct.GetTotalGearRatio();
            return toWheels;
        }
        else
        {
            float toWheels = 0;
            return toWheels;
        }
    }

    public void ToEngine()
    {
        toEngine = (cc.rayCastWheels[3].GetWheelAngularVelocity() * ct.GetTotalGearRatio() - engineAngularVelocity) * clutch ;
        Debug.Log("ToEngine= " + toEngine + " + engineAngularVelocity" + engineAngularVelocity) ;

        toEngine = Mathf.Clamp(toEngine, engineIdleRpm, engineMaxRpm);
       
    }

    public float BaseTorque()
    {
        baseTorque += (cc.rayCastWheels[3].GetWheelAngularVelocity() * radsSecToRpm - engineAngularVelocity * radsSecToRpm) * clutch / (cc.rayCastWheels[0].wheelInertia + inertia);
        return baseTorque;
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