using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysycsUpdate : MonoBehaviour
{
    private float delta;
    private CarEngine ce;
    private CarTransmission ct;
    private CarController cc;
    private Dashboard db;
    float throttle;
    
    void Start()
    {
        ce = GetComponent<CarEngine>();
        ct = GetComponent<CarTransmission>();
        cc = GetComponent<CarController>();
        ct.Initialize();
        ce.Initialize();
        db = GetComponent<Dashboard>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        delta = Time.fixedDeltaTime;
        throttle = Input.GetAxis("Vertical");
        Shifter();
        ce.TestTorque();

        ce.UpdatePhysics(delta, throttle);
        ct.PhysicsUpdate(delta);
        db.PhysicsUpdate();
        
        for (int i = 0; i < cc.rayCastWheels.Length; i++)
        {
            cc.rayCastWheels[i].PhysicsUpdate(delta, 0);
        }

        
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
