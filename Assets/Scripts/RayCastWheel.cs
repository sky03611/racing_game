using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastWheel : MonoBehaviour
{
    private Rigidbody rb;
    private CarController cc;
    private CarEngine ce;
    private CarTransmission ct;
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
    public float currentLength;

    public float frictionCoefficientX;
    public float frictionCoefficientY;
    public float longStiffness = 1f;
    public float cornerStiffness;

    public float wheelRadius = 34f;
    public float wheelInertia;
    public float wheelMass;
    private float wheelAngularVelocity;
    private Vector3 wheelVelocityLS;

    public bool isGrounded = false;
    private float springForce;
    private float springLastLength;
    private float damperForce;

    private float forceY;
    private float forceX;
    private float forceZ;
    private float maxWheelSpeed;
    private float longSlipVelocity;
    private float lateralSlipNormalized;
    private float longSlipNormalized;
    private float wheelangularVelDeg;
    private Vector2 longLateralVector;
    private float frictionTorque;

    public float steerAngle;
    public float strangeFactor;
    private float wheelAngle;

    private float deltaTime;
    private float driveTorque;
    private float breakVelocity = 1f;
    [HideInInspector]
    public RaycastHit hit;

    void Start()
    {
        ce = transform.root.GetComponent<CarEngine>();
        cc = transform.root.GetComponent<CarController>();
        rb = transform.root.GetComponent<Rigidbody>();
        ct = transform.root.GetComponent<CarTransmission>();
    }

    public void PhysicsUpdate(float delta, float torque)
    {
        deltaTime = delta;
        driveTorque = torque;
        RaycastSingle();
        if (isGrounded)
        {
            GetSuspensionForce();
            ApplySuspinsionForce();
            GetVelocityLocal();
            GetWheelSlipCombined();
            WheelAngularVelocity();
            AddTireForce();
        }
        WheelRolling();
    }

    private void RaycastSingle()
    {
        if (Physics.Raycast(transform.position, -transform.up, out hit, (springRestLength + wheelRadius)))
        {
            isGrounded = true;
            currentLength = (transform.position - (hit.point + (transform.up * wheelRadius))).magnitude;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void GetSuspensionForce()
    {
        springForce = (springRestLength - currentLength) * springStiffness;
        damperForce = ((springLastLength - currentLength) / deltaTime) * damper;
        forceZ = springForce + damperForce;
        springLastLength = currentLength; //Update For Next Frame
    }

    private void ApplySuspinsionForce()
    {
        rb.AddForceAtPosition(forceZ * transform.up, transform.position);
    }

    private void GetVelocityLocal()
    {
        wheelVelocityLS = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
    }

    void Update()
    {
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, deltaTime * cc.steerBackTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
    }
   
    private void WheelAngularVelocity()
    {
        wheelInertia = Mathf.Pow(wheelRadius, 2) * wheelMass * 0.5f;
        frictionTorque = (Mathf.Max(0, forceZ) * wheelRadius * Mathf.Clamp(longSlipVelocity / -strangeFactor, -1, 1)) / wheelInertia*deltaTime;
        var wheelAngularAcceleration = driveTorque / wheelInertia *deltaTime;
        Debug.Log(wheelAngularAcceleration);
        wheelAngularVelocity = Mathf.Sign(maxWheelSpeed)* Mathf.Min(Mathf.Abs(wheelAngularVelocity += wheelAngularAcceleration), Mathf.Abs(maxWheelSpeed));
        wheelAngularVelocity = wheelAngularVelocity + frictionTorque +breakVelocity;
        if (ct.GetTotalGearRatio() != 0)
        {
            maxWheelSpeed = ce.EngineAngularVelocity() / ct.GetTotalGearRatio();
        }
        else
        {
            maxWheelSpeed = 100;
        }
        //Костыль
        if (rb.velocity.magnitude * 3.6 <= 0.2f && wheelAngularAcceleration<=0)
        {
            wheelAngularVelocity = 0;
        }
    }  

    private void WheelRolling()
    {
        wheelangularVelDeg = (wheelAngularVelocity * Mathf.Rad2Deg) * deltaTime;
        wheelMesh.transform.Rotate(wheelangularVelDeg, 0, 0, Space.Self);  
    }

    private void GetWheelSlipCombined()
    {
        if (isGrounded)
        {
            lateralSlipNormalized = Mathf.Clamp(wheelVelocityLS.x * cornerStiffness*-1, -1, 1);
            longSlipVelocity = wheelAngularVelocity * wheelRadius - wheelVelocityLS.z;

            if (wheelVelocityLS.z * longSlipVelocity > 0)
            {
                longSlipNormalized = Mathf.Clamp(driveTorque / wheelRadius / Mathf.Max(0.00001f, forceZ), -2, 2);
            }
            else
            {
                longSlipNormalized = Mathf.Clamp(longSlipVelocity * longStiffness, -1, 1);
            }
            Vector2 lateralLongVector = new Vector2(longSlipNormalized,lateralSlipNormalized);
            longLateralVector = lateralLongVector;
            var combinedSlip = lateralLongVector.magnitude;
            Vector2 tireForceNormalized = slipForceCurve.Evaluate(combinedSlip) * longLateralVector.normalized;
            forceX = tireForceNormalized.x * Mathf.Max(0.0f, forceZ) * frictionCoefficientX;
            if (wheelFL || wheelFR)
            {
                forceY = tireForceNormalized.y * Mathf.Max(0.0f, 3000) * frictionCoefficientY;
            }
            else
            {
                forceY = tireForceNormalized.y * Mathf.Max(0.0f, 3000) * frictionCoefficientY;
            }
        }
    }

    private void AddTireForce()
    {
        rb.AddForceAtPosition(forceX * transform.forward + forceY * transform.right, hit.point);
    }
    
    public float GetWheelAngularVelocity()
    {
        return wheelAngularVelocity;
    }

    public void SetWhelAngularVelocity(float var)
    {
        if (Mathf.Abs(wheelAngularVelocity) < 1)
        {
            breakVelocity = 0;
        }
        else 
            breakVelocity = var;
    }

}
