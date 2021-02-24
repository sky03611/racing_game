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

    private float totalDriveAxisAngularVelocity = 0f;
    private float clutchAngularVelocity = 0f;

    float engineangularvel1;
    float currentTotalDriveAxisAngularVelocity;
    float lastDriveAxis;


    //DashBoard
    private Dashboard dashBoard;
    private CarAudio ca;
    private CarTransmission ct;
    private CarController cc;


    void Start()
    {
        
        dashBoard = GetComponent<Dashboard>();
        dashBoard.SetEngineMaxRpm(engineMaxRpm);
        ca = GetComponent<CarAudio>();
        ct = GetComponent<CarTransmission>();
        cc = GetComponent<CarController>();
        rpmToRadsSec = Mathf.PI * 2 / 60;
        radsSecToRpm = 1 / rpmToRadsSec;
        engineRpm = engineIdleRpm; // на всякий случай
    }

   void FixedUpdate()
   {
        
       torquePower = engineTorqueCurve.Evaluate(engineRpm); 
       engineTorqueValue = Mathf.Lerp(backTorque, engineRpm, throtleValue) / inertia * Time.deltaTime; // Суммарный крутящий угловой момент radsSec EAC
       engineAngularVelocity = Mathf.Clamp(engineTorqueValue + engineAngularVelocity, engineIdleRpm * rpmToRadsSec, engineMaxRpm * rpmToRadsSec);
        SetEngineAngularVelocity();
        engineRpm = engineAngularVelocity * radsSecToRpm;
       dashBoard.SetEngineRpm(engineRpm); // Send Rpm to dashboard
       ca.SetEngingeRpm(engineRpm); // send Rpm to audio script
       //transmissionEngineTorque = ct.TotalGearRatio() * torquePower;
       //Debug.Log("transmissionEngineTorque " + ct.TotalGearRatio());
   }

    public void SetEngineAngularVelocity()
    {
        totalDriveAxisAngularVelocity = 0;
        //cc.rayCastWheels[0].GetWheelAngularVelocity();
        if (cc.DriveTypeInt() == 0)
        {
            for (int i = 0; i < cc.rayCastWheels.Length; i++)
            {
                totalDriveAxisAngularVelocity += cc.rayCastWheels[i].GetWheelAngularVelocity() / 4;
            }
        }
        else if (cc.DriveTypeInt() == 1)
        {
            for (int i = 0; i < cc.rayCastWheels.Length-2; i++)
            {
                totalDriveAxisAngularVelocity += cc.rayCastWheels[i].GetWheelAngularVelocity() / 2;
            }
        }
        else
        {
            for (int i = 2; i < cc.rayCastWheels.Length; i++)
            {
                totalDriveAxisAngularVelocity += cc.rayCastWheels[i].GetWheelAngularVelocity() / 2;
            }
        }
        clutchAngularVelocity = totalDriveAxisAngularVelocity * ct.TotalGearRatio();
        if (ct.GetCurrentGear() != 1)
        {
            engineAngularVelocity = Mathf.Clamp((clutchAngularVelocity - engineAngularVelocity) * 0.2f + engineAngularVelocity, engineIdleRpm * Mathf.PI * 2 / 60, engineMaxRpm * Mathf.PI * 2 / 60);
            
        }
        Debug.Log(clutchAngularVelocity - engineAngularVelocity);
        

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
