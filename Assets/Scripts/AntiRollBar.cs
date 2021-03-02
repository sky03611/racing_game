using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRollBar : MonoBehaviour
{
    private CarController cc;
    private Rigidbody rb;
    private RaycastHit[] hit = new RaycastHit[4];
    private bool[] isGrounded = new bool[4];
    private float[] currentLength = new float[4];
    float travelFL;
    float travelFR;
    float travelRL;
    float travelRR;
    public float antiRoll;
    public void Initialize()
    {
        cc = GetComponent<CarController>();
        rb = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
     public void PhysicsUpdate()
    {
        RaycastSingle();
        ApplyAntiRollBar();
    }

    private void RaycastSingle()
    {
        for (int i = 0; i < cc.rayCastWheels.Length; i++)
        {
            if (Physics.Raycast(cc.rayCastWheels[i].transform.position, -cc.rayCastWheels[i].transform.up, out hit[i], (cc.rayCastWheels[i].springRestLength + cc.rayCastWheels[i].wheelRadius)))
            {
                isGrounded[i] = true;
                currentLength[i] = cc.rayCastWheels[i].currentLength;
            }
            else
            {
                isGrounded[i] = false;
            }
        }
    }

    public void ApplyAntiRollBar()
    {
        float travelFL = 1.0f;
        float travelFR = 1.0f;
        float travelRL = 1.0f;
        float travelRR = 1.0f;

        bool groundedFL = isGrounded[0];

        if (groundedFL)
        {
            travelFL = ( -cc.rayCastWheels[0].transform.InverseTransformPoint(hit[0].point).y - cc.rayCastWheels[0].wheelRadius) / currentLength[0];
           
        }

        bool groundedFR = isGrounded[1];

        if (groundedFR)
        {
            travelFR = (-cc.rayCastWheels[1].transform.InverseTransformPoint(hit[1].point).y - cc.rayCastWheels[1].wheelRadius) / currentLength[1];
        }

        bool groundedRL = isGrounded[2];

        if (groundedFL)
        {
            travelFL = (-cc.rayCastWheels[2].transform.InverseTransformPoint(hit[2].point).y - cc.rayCastWheels[2].wheelRadius) / currentLength[2];

        }

        bool groundedRR = isGrounded[3];

        if (groundedFR)
        {
            travelFR = (-cc.rayCastWheels[3].transform.InverseTransformPoint(hit[3].point).y - cc.rayCastWheels[3].wheelRadius) / currentLength[3];
        }


        var antiRollForceF = (travelFL - travelFR) * antiRoll;
        var antiRollForceR = (travelRL - travelRR) * antiRoll;
        Debug.Log(travelFL + " TravelFl");
        Debug.Log(travelFR + " TravelFR");

        if (groundedFL)
            rb.AddForceAtPosition(cc.rayCastWheels[0].transform.up * -antiRollForceF,
                cc.rayCastWheels[0].transform.position);
        if (groundedFR)
            rb.AddForceAtPosition(cc.rayCastWheels[1].transform.up * -antiRollForceF,
                cc.rayCastWheels[1].transform.position);
        if (groundedRL)
            rb.AddForceAtPosition(cc.rayCastWheels[2].transform.up * -antiRollForceR,
                cc.rayCastWheels[2].transform.position);
        if (groundedRR)
            rb.AddForceAtPosition(cc.rayCastWheels[3].transform.up * -antiRollForceR,
                cc.rayCastWheels[3].transform.position);
    }
}
