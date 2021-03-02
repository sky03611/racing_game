using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysycsUpdate : MonoBehaviour
{
    private float delta;
    private CarEngine ce;
    private CarTransmission ct;
    private CarController cc;
    private AntiRollBar arb;
    private Dashboard db;

    float throttle;
    
    void Start()
    {
        ce = GetComponent<CarEngine>();
        ct = GetComponent<CarTransmission>();
        cc = GetComponent<CarController>();
        arb = GetComponent<AntiRollBar>();
        db = GetComponent<Dashboard>();
        cc.Initialize();
        ct.Initialize(cc.GetDriveTypeDivider());
        ce.Initialize();
        arb.Initialize();
       
        
    }

    void Update()
    {
        Shifter();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        delta = Time.fixedDeltaTime;
        throttle = Input.GetAxis("Vertical");
        
        ce.TestTorque();
        cc.SteerFactorChanger();
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
        //arb.PhysicsUpdate();


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
