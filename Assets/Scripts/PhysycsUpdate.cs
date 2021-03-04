using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysycsUpdate : MonoBehaviour
{
    private float delta;
    private CarEngine ce;
    private CarTransmission ct;
    private CarController cc;
    private Breaks breaks;
    private Dashboard db;

    float throttle;
    
    void Start()
    {
        ce = GetComponent<CarEngine>();
        ct = GetComponent<CarTransmission>();
        cc = GetComponent<CarController>();
        db = GetComponent<Dashboard>();
        breaks = GetComponent<Breaks>();
        cc.Initialize();
        ct.Initialize(cc.GetDriveTypeDivider());
        ce.Initialize();
        breaks.Initialize();
        AudioListener.volume = 0.1f;
       
    }

    void Update()
    {
        Shifter();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        throttle = Input.GetAxisRaw("Vertical");

            breaks.PhysicsUpdate(delta, throttle);
        delta = Time.fixedDeltaTime;
        
        ce.AverageWheelSpeed();
        ce.UpdatePhysics(delta, throttle);
        ct.PhysicsUpdate(delta);
        db.PhysicsUpdate();
        //awd
        if (cc.DriveTypeInt() == 0)
        {
            for (int i = 0; i < cc.rayCastWheels.Length; i++)
            {
                cc.rayCastWheels[i].PhysicsUpdate(delta, ct.GetTransmissionTorque());
            }
        }
        //fwd
        if (cc.DriveTypeInt() == 1)
        {
            for (int i = 0; i < cc.rayCastWheels.Length-2; i++)
            {
                cc.rayCastWheels[i].PhysicsUpdate(delta, ct.GetTransmissionTorque());
            }
            for (int i = 2; i < cc.rayCastWheels.Length; i++)
            {
                cc.rayCastWheels[i].PhysicsUpdate(delta, 0f);
            }
        }
    
        //rwd
        if (cc.DriveTypeInt() == 2)
        {
            for (int i = 2; i < cc.rayCastWheels.Length ; i++)
            {
                cc.rayCastWheels[i].PhysicsUpdate(delta, ct.GetTransmissionTorque());
            }

            for (int i = 0; i < cc.rayCastWheels.Length-2; i++)
            {
                cc.rayCastWheels[i].PhysicsUpdate(delta, 0f);
            }
        }
        cc.SteerFactorChanger();
        
        


    }

    private void Shifter()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ct.GearUp();

        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            ct.GearDown();
        }
    }
}
