using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPhysicsUpdate : MonoBehaviour
{
    private float delta;
    private float[] steerAngle = new float[2];
    private Rigidbody rb;
    private NrcSteering nrcSteering;
    private NrcEngine nrcEngine;
    private NrcTransmission nrcTransmission;
    public Vector3 centerOfMass;
    public NewRayCast[] nrc;

    private float inputV;
    private float inputH;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        nrcSteering = GetComponent<NrcSteering>();
        nrcEngine = GetComponent<NrcEngine>();
        nrcTransmission = GetComponent<NrcTransmission>();

        nrcSteering.Initialize();
        nrcEngine.Initialize();
        nrcTransmission.Initialize();
        
        for (int i = 0; i < nrc.Length; i++)
        {
            nrc[i].Initialize();
        }

    }

    void Update()
    {
        Shifter();
    }

    void FixedUpdate()
    {
        inputV = Input.GetAxis("Vertical");
        inputH = Input.GetAxis("Horizontal");
        Steering();
        CenterOfMassCorrector();
        nrcEngine.PhysicsUpdate(inputV, delta);
        nrcTransmission.PhysicsUpdate();
        delta = Time.fixedDeltaTime;
        for (int i = 0; i < nrc.Length; i++)
        {
            nrc[i].UpdatePhysics(delta, nrcTransmission.transmissionTorque, steerAngle[0], steerAngle[1]); //TODO Optimize
        }
    }

    private void CenterOfMassCorrector()
    {
        ;
        rb.centerOfMass = centerOfMass;
       
    }

    public void Steering()
    {
        nrcSteering.PhysicsUpdate(inputH);
        steerAngle[0] = nrcSteering.ackermannAngleLeft;
        steerAngle[1] = nrcSteering.ackermannAngleRight;
    }

    private void Shifter()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            nrcTransmission.GearUp();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            nrcTransmission.GearDown();
        }
    }
}
