using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastWheel : MonoBehaviour
{
    private Rigidbody rb;
    private CarController cc;
    public AnimationCurve slipForceCurve;
    public bool wheelFL;
    public bool wheelFR;
    public bool wheelRL;
    public bool wheelRR;

    public GameObject wheelMesh;
    public float springRestLength = 50f;
    public float springStiffness = 500f;
    public float damper = 10f;

    public float springTravel = 10f;
    public float wheelRadius = 34f;
    private float wheelInertia;
    private float wheelAngularVelocity;
    private float totalGearRatio;
    public float wheelMass;

    private float rayLength;
    private float rayMinLenth;
    private bool isGrounded = false;
    private float maxLength;
    private float springLength;
    private float springForce;
    private float springLastLength;
    private float damperForce;
    private float springVelocity;
    private Vector3 wheelVelocityLS;
    private Vector3 finalForce;
    private float forceY;
    private float forceX;
    private float forceZ;
    private float wheelForwardVelocity;
    private float maxWheelSpeed;
    private float longSlipVelocity;
    private float lateralSlipNormalized;
    private float longSlipNormalized;
    private float lateralLongSlipNormalized;
    private Vector2 longLateralVector;
    public float longStiffness = 1f;

    public float steerAngle;
    public float cornerStiffness;
    private float wheelAngle;
    private CarEngine ce;
    private CarTransmission ct;
    public bool isDriven;

    private float aboba3;
    private float wheelAngularVelocityTest;
    // Start is called before the first frame update
    void Start()
    {
        ce = transform.root.GetComponent<CarEngine>();
        cc = transform.root.GetComponent<CarController>();
        rb = transform.root.GetComponent<Rigidbody>();
        ct = transform.root.GetComponent<CarTransmission>();
        rayLength = springTravel + springRestLength + wheelRadius;
        rayMinLenth = springRestLength - springTravel;
    }

    void Update()
    {
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, Time.deltaTime * cc.steerBackTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        ApplySuspinsionForces();
        GetWheelSlipCombined();
        TireFriction();
        WheelAngularVelocity();
        WheelRolling();
    }

    private void WheelAngularVelocity()
    {
        wheelInertia = Mathf.Pow(wheelRadius, 2) * wheelMass * 0.5f; //todo
        if (isDriven)
        {
            // wheelAngularVelocityTest += Time.fixedDeltaTime * ct.GetTrasmissionTorque() / wheelInertia;
            // wheelAngularVelocity = Mathf.Min(maxWheelSpeed, wheelAngularVelocityTest);
            wheelAngularVelocity = Mathf.Sign(maxWheelSpeed) * Mathf.Min(Mathf.Abs(wheelAngularVelocity += (ct.GetTrasmissionTorque() / wheelInertia) * Time.fixedDeltaTime), Mathf.Abs(maxWheelSpeed));

        }
        else
        {
            wheelAngularVelocity = 0f;
        }
        
        
       //wheelAngularVelocity = Mathf.Sign(maxWheelSpeed) * Mathf.Min(Mathf.Abs(wheelAngularVelocity += (ce.GetEngineTorque() / wheelInertia) * Time.fixedDeltaTime), Mathf.Abs(maxWheelSpeed));
        //max wheel speed on current gear
        if (ct.TotalGearRatio() != 0)
        { 
           maxWheelSpeed = ce.EngineAngularVelocity() / ct.TotalGearRatio();    
        }
        else
        {
            maxWheelSpeed = 0;
        }
      //  Debug.Log(wheelAngularVelocity);
    }

    private void ApplySuspinsionForces()
    {
        rayLength = springTravel + springRestLength + wheelRadius;
        springLastLength = springLength;
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, rayLength))
        {
            //wheelVelocityLS = this.transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
            Vector3 velocityAtTouch = rb.GetPointVelocity(hit.point);
            wheelVelocityLS =transform.InverseTransformDirection(velocityAtTouch);
            
            wheelForwardVelocity = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point)).z;
            springLength = hit.distance - wheelRadius;
            springVelocity = (springLastLength - springLength) / Time.fixedDeltaTime;
            damperForce = damper * springVelocity;
            Mathf.Clamp(springLength, rayMinLenth, rayLength);
            springForce = (springStiffness * (springRestLength - springLength)) + damperForce;
            forceZ = springForce;

            // wheelMesh.transform.localPosition = new Vector3(wheelMesh.transform.localPosition.x, wheelRadius - hit.distance, wheelMesh.transform.localPosition.z);
           finalForce = ((springForce * transform.up) + (forceY * transform.right) + (forceX * transform.forward));
           // finalForce = new Vector3(forceY, springForce, forceX);
            rb.AddForceAtPosition(finalForce, hit.point);
            Debug.DrawRay(transform.position, (-transform.up * rayLength));
        }
    }

    private void WheelRolling()
    {
        var angularVelocity = (wheelAngularVelocity * Mathf.Rad2Deg) * Time.fixedDeltaTime;
        if (totalGearRatio == 1)
        {
            wheelMesh.transform.Rotate(0, 0, 0, Space.Self);
        }
        else
        {
            wheelMesh.transform.Rotate(angularVelocity, 0, 0, Space.Self);
        }
        if (totalGearRatio < 0)
        {
            wheelMesh.transform.Rotate(-1 * angularVelocity, 0, 0, Space.Self);
        }
    }

    private void TireFriction()
    {

    }

    private void GetWheelSlipCombined()
    {
       lateralSlipNormalized = Mathf.Clamp(wheelVelocityLS.y * cornerStiffness*-1, -1, 1);
       longSlipVelocity = wheelAngularVelocity * wheelRadius - wheelVelocityLS.x; 

       if(wheelVelocityLS.x * longSlipVelocity > 0)
        {
            longSlipNormalized = Mathf.Clamp((ct.GetTrasmissionTorque() / wheelRadius) / Mathf.Max(0.0f, forceZ),-2,2);
            
        }
        else
        {
           
            longSlipNormalized = Mathf.Clamp(longSlipVelocity * longStiffness, -2, 2);
        }
        longLateralVector.x = longSlipNormalized;
        longLateralVector.y = lateralSlipNormalized;
        var combinedSlip = longLateralVector.magnitude;
        Vector2 tireForceNormalized = slipForceCurve.Evaluate(combinedSlip) * longLateralVector.normalized * Mathf.Max(0.0f, forceZ);
        
        forceX = tireForceNormalized.x;
        forceY = tireForceNormalized.y;
        
       // if (wheelFL)
       // {
       //     Debug.Log("wheelFL " +tireForceNormalized.y);
       // }
       // else if (wheelFR)
       // {
       //     Debug.Log("wheelFR " + tireForceNormalized.y);
       // }
       // else if (wheelRR)
       // {
       //     Debug.Log("wheelRR " + tireForceNormalized.y);
       // }
       // else if (wheelRL)
       // {
       //     Debug.Log("wheelRL " + tireForceNormalized.y);
       // }
    }

    private void IsGrounded(RaycastHit hit)
    {
        if (hit.collider != null)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        if (!isGrounded)
        {
            springLength = rayLength;
        }
        else
        {
            rayLength = hit.distance - wheelRadius;
        }
    }
}
