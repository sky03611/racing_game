using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelEngineSync : MonoBehaviour
{
    private CarController cc;
    private CarTransmission ct;
    private CarEngine ce;
    private float totalDriveAxisAngularVelocity = 0f;
    private float clutchAngularVelocity = 0f;
    private float engineAngularVel;
    float test1;

    void Start()
    {
        cc = GetComponent<CarController>();
        ct = GetComponent<CarTransmission>();
        ce = GetComponent<CarEngine>();
    }

    //void FixedUpdate()
    //{
    //    totalDriveAxisAngularVelocity = 0;
    //    engineAngularVel = ce.EngineAngularVelocity();
    //    //cc.rayCastWheels[0].GetWheelAngularVelocity();
    //    if (cc.DriveTypeInt() == 0)
    //    {
    //        for (int i = 0; i < cc.rayCastWheels.Length; i++)
    //        {
    //            totalDriveAxisAngularVelocity += cc.rayCastWheels[i].GetWheelAngularVelocity() / 4;
    //        }
    //    }
    //    else if (cc.DriveTypeInt() == 1)
    //    {
    //        for (int i = 0; i < cc.rayCastWheels.Length - 2; i++)
    //        {
    //            totalDriveAxisAngularVelocity += cc.rayCastWheels[i].GetWheelAngularVelocity() / 2;
    //        }
    //    }
    //    else
    //    {
    //        for (int i = 2; i < cc.rayCastWheels.Length; i++)
    //        {
    //            totalDriveAxisAngularVelocity += cc.rayCastWheels[i].GetWheelAngularVelocity() / 2;
    //        }
    //    }
    //    clutchAngularVelocity = totalDriveAxisAngularVelocity * ct.TotalGearRatio();
    //    if (ct.GetCurrentGear() != 1) 
    //            {
    //       test1 = Mathf.Clamp((clutchAngularVelocity - engineAngularVel) * 5f +engineAngularVel, ce.engineIdleRpm * Mathf.PI * 2 / 60, ce.engineMaxRpm * Mathf.PI * 2 / 60);
    //       // ce.SetEngineAngularVelocity(test1);
    //            }
    //}
}
