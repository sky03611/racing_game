using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastWheel : MonoBehaviour
{
    private Rigidbody rb;
    private CarController cc;
    public AnimationCurve longSlipForceCurve;
    public AnimationCurve lateralSlipForceCurve;
    public bool wheelFL;
    public bool wheelFR;
    public bool wheelRL;
    public bool wheelRR;

    public float lateralStiffness;
    public float longStiffness = 1f;
    public float gripCoeff;

    public GameObject wheelMesh;
    public float springRestLength = 50f;
    public float springStiffness = 500f;
    public float damper = 10f;
    public float springTravel = 10f;
    public float currentLength;



    public float frictionCoefficientX;
    public float frictionCoefficientY;

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
    public float skidSoundVectorNorm;

    private float kmhTester;
    private float lateralConst;
    private float longConst;
    [HideInInspector]
    public RaycastHit hit;

    private float sX;
    private float sY;

    void Start()
    {
        cc = transform.root.GetComponent<CarController>();
        rb = transform.root.GetComponent<Rigidbody>();
        lateralConst = lateralStiffness;
        longConst = longStiffness;
    }
    void Update()
    {
        KmPH();
    }

    public void PhysicsUpdate(float delta, float torqueFL, float torqueFR, float torqueRL, float torqueRR)
    {
        deltaTime = delta;
        if (wheelFL)
        {
            driveTorque = torqueFL;
        }

        if (wheelFR)
        {
            driveTorque = torqueFR;
        }

        if (wheelRL)
        {
            driveTorque = torqueRL;
        }

        if (wheelRR)
        {
            driveTorque = torqueRR;
        }
        //driveTorque = torque;
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
        ApplySkidSound();

        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, deltaTime * cc.steerBackTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
    }

    private void ApplySkidSound()
    {
        Vector2 skidVector = new Vector2(longSlipVelocity, wheelVelocityLS.x);
        skidSoundVectorNorm = skidVector.magnitude;

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


    private void WheelAngularVelocity()
    {
        
        wheelInertia = Mathf.Pow(wheelRadius, 2) * wheelMass * 0.5f;
        frictionTorque = (Mathf.Max(0, forceZ) * wheelRadius * Mathf.Clamp(longSlipVelocity / -strangeFactor, -1, 1)) / wheelInertia * deltaTime;
        var wheelAngularAcceleration = driveTorque / wheelInertia;
        wheelAngularVelocity += wheelAngularAcceleration * deltaTime + frictionTorque;
        //wheelAngularVelocity = Mathf.Clamp(wheelAngularVelocity, -120, 120); //temp action
        //wheelAngularVelocity = Mathf.Clamp(wheelAngularVelocity, -ce.EngineMaxAngularVelocity() / ct.currentGearRatio, ce.EngineMaxAngularVelocity() / ct.currentGearRatio); //temp action



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
            longSlipVelocity = (wheelAngularVelocity * wheelRadius) - wheelVelocityLS.z;
            var slipAngle = Mathf.Atan(wheelVelocityLS.x / wheelVelocityLS.z) * Mathf.Rad2Deg;
            //var lateralForceVector = lateralSlipForceCurve.Evaluate(slipAngle) * lateralStiffness;
            var lateralForceVector = lateralSlipForceCurve.Evaluate(wheelVelocityLS.x) * lateralStiffness;
            var longForceVector = longSlipForceCurve.Evaluate(wheelVelocityLS.z) * longStiffness * longSlipVelocity;
            var longLateralVectorsMagn = new Vector2(longForceVector, lateralForceVector).magnitude;
            var longLateralVectorsNorm = new Vector2(longForceVector, lateralForceVector).normalized;
            Vector2 multiVector = longLateralVectorsMagn * longLateralVectorsNorm;
            var maxFriction = forceZ * gripCoeff;
   
            forceX = Mathf.Clamp(multiVector.x, -2, 2) * Mathf.Max(maxFriction, 0);
            forceY = Mathf.Clamp(multiVector.y, -2, 2) * Mathf.Max(maxFriction, 0);
            if (wheelRR || wheelRL)
            {
                //Debug.Log(lateralForceVector);
                //Debug.Log("Evaluation = "+wheelVelocityLS.x);
            }
        }
    }

    private void GetSx()
    {

    }
    
    private void GetSy()
    {

    }


    private void AddTireForce()
    {
        rb.AddForceAtPosition(forceX * transform.forward + forceY * -transform.right, hit.point);
        
    }

    private void KmPH()
    {

       if(Input.GetKeyDown(KeyCode.B))
        {
            StartCoroutine(kmph());
        }
    }

    IEnumerator kmph()
    {
       while(rb.velocity.magnitude *3.6 < 100)
        {
            kmhTester = kmhTester+ 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log(kmhTester);
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

    public void SetHandBrake(bool var)
    {
        
        if (var)
        {
            
            lateralStiffness =1f;
            longStiffness = 1f;
        }
        else
        {
            lateralStiffness = lateralConst;
            longStiffness = longConst;
        }
    }

}
