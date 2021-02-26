using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysycsUpdate : MonoBehaviour
{
    private float delta;
    private CarEngine ce;
    private CarTransmission ct;
    private CarController cc;
    private WheelEngineSync wes;
    private float driveTorque;
    
    void Start()
    {
        ce = GetComponent<CarEngine>();
        ct = GetComponent<CarTransmission>();
        cc = GetComponent<CarController>();
        wes = GetComponent<WheelEngineSync>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
       
        driveTorque = ct.GetTrasmissionTorque();
        delta = Time.fixedDeltaTime;
        for (int i = 0; i < cc.rayCastWheels.Length-2; i++)
        {
            cc.rayCastWheels[i].UpdatePhysics(delta, 0);
        }

        for (int i = 2; i < cc.rayCastWheels.Length ; i++)
        {
            cc.rayCastWheels[i].UpdatePhysics(delta, driveTorque);
        }
        ce.UpdatePhysics(delta);
    }
}
