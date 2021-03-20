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
    public AnimationCurve nrcPacejka;

    private float nrcLong;

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

    //private float sX;
    private float sY;

    private float fX;
    private float sX;

    //Pacejka's shit
    float a0 = 1.4f;
    float a1 = 0f;
    float a2 = 1100f;
    float a3 = 1100f;
    float a4 = 10f;
    float a5 = 0f;
    float a6 = 0f;
    float a7 = -2f;
    float a8 = 0f;
    float a9 = 0f;
    float a10 = 0f;
    float a11 = 0f;
    float a12 = 0f;
    float a13 = 0f;
    float a14 = 0f;
    float a15 = 0f;
    float a16 = 0f;
    float a17 = 0f;
    float camberAngle = 0;

    float pb0 = 1.5f;
    float pb1 = 0f;
    float pb2 = 1100f;
    float pb3 = 0f;
    float pb4 = 300f;
    float pb5 = 0f;
    float pb6 = 0f;
    float pb7 = 0f;
    float pb8 = -2f;
    float pb9 = 0f;
    float pb10 = 0f;
    float pb11 = 0f;
    float pb12 = 0f;
    float pb13 = 0f;

    float nlC;
    float nlD;
    float nlBCD;
    float nlB;
    float nlE;
    float nlH;
    float nlV;
    float nlBx1;

    float nC;
    float nD;
    float nBCD;
    float nB;
    float nE;
    float nH;
    float nV;
    float nBx1;
    float slip;
    float nlSlip;



    float fZp;
    float longitudinalForce;
    float lateralForce;

    float longSlip;
    float lateralSlip;
    float tireForceLong;
    float tireForceLat;

    float wheelAngularAcc;


    private void CalculatePacejka()
    {
        slip = longSlipVelocity;
        nlSlip = Mathf.Atan(wheelVelocityLS.x / wheelVelocityLS.z) * Mathf.Rad2Deg;
        fZp = forceZ / 1000f;
        nC = pb0;
        nD = fZp * (pb1 * fZp + pb2);
        nBCD = (pb3 * Mathf.Pow(fZp, 2) + pb4 * fZp) * Mathf.Pow(nE, -pb5 * fZp);
        nB = nBCD / (nC * nD);
        nE = (pb6 * Mathf.Pow(fZp, 2) + pb7 * fZp + pb8) * (1 - pb13 * Mathf.Sign(slip + nH));
        nH = pb9 * fZp + pb10;
        nV = pb11 * fZp + pb12;
        nBx1 = nB *(slip + nH);

        longitudinalForce = nD * Mathf.Sin(nC * Mathf.Atan(nBx1 - nE * (nBx1 - Mathf.Atan(nBx1)))) + nV;

        //lateral
        nlC = a0;
        nlD = fZp * (a1 * fZp + a2) * (1 - a15 * Mathf.Pow(camberAngle, 2));
        nlBCD = a3 * Mathf.Sin(Mathf.Atan(fZp / a4) * 2) * (1 - a5 * camberAngle);
        nlB = nlBCD / (nlC * nlD);
        nlE = (a6 * fZp + a7) * (1 - (a16 * camberAngle + a17) * Mathf.Sign(nlSlip + nlH));
        nlH = a8 * fZp + a9 + a10 * camberAngle;
        nlV = a11 * fZp + a12 + (a13 * fZp + a14) * camberAngle * fZp;
        nlBx1 = nlB * (nlSlip + nlH);

        lateralForce = nlD * Mathf.Sin(nlC * Mathf.Atan(nlBx1 - nlE * (nlBx1 - Mathf.Atan(nlBx1)))) + nlV;
        Debug.Log(lateralForce);
    }

    float sB = 10f;
    float sC = 1.9f;
    float sD =1f;
    float sE = 0.97f;

     void SimplePacejka()
    {
        if (wheelVelocityLS.z == 0)
        {
            longSlip = 0;
        }
        else
        {
            if (wheelAngularAcc > 0)
            {
                longSlip = ((wheelAngularVelocity * wheelRadius) - wheelVelocityLS.z) / (wheelAngularVelocity * wheelRadius);
            }
            else
            {
                longSlip = -(wheelVelocityLS.z - (wheelAngularVelocity * wheelRadius)) / wheelVelocityLS.z;
            }
        }

        var normalizedSlipRatio = nrcPacejka.Evaluate(longSlip * 100);
        fX = normalizedSlipRatio * forceZ /1000f;
        Debug.Log(fX); 
        //if (Mathf.Abs(wheelAngularVelocity * wheelRadius) > Mathf.Abs(wheelVelocityLS.z))
        //{
        //    longSlip  = ((wheelAngularVelocity * wheelRadius) - wheelVelocityLS.z) / (wheelAngularVelocity * wheelRadius);
        //}
        //else
        //{
        //    longSlip = -(wheelVelocityLS.z - (wheelAngularVelocity * wheelRadius)) / wheelVelocityLS.z;
        //}
        //longSlip *= 100f;
        //longitudinalForce = longSlipForceCurve.Evaluate(longSlip);
        //longitudinalForce = sD * Mathf.Sin(1.65f * Mathf.Atan(sB * longSlip - sE * (sB * longSlip - Mathf.Atan(nB * longSlip))));
        var lateralAngle = Mathf.Atan(-wheelVelocityLS.x / Mathf.Abs(wheelVelocityLS.z)) * Mathf.Rad2Deg;
        lateralForce =  sD * Mathf.Sin(1.3f * Mathf.Atan(sB * lateralAngle - sE * (sB * lateralAngle - Mathf.Atan(nB * lateralAngle))));
    }


    void Start()
    {
        cc = transform.root.GetComponent<CarController>();
        rb = transform.root.GetComponent<Rigidbody>();
        lateralConst = lateralStiffness;
        longConst = longStiffness;
        wheelInertia = Mathf.Pow(wheelRadius, 2) * wheelMass * 0.5f;
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
            WheelAccelerationOnGround();
            SimplePacejka();
            
            //WheelAngularVelocity();
            //CalculatePacejka();

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


    void WheelAccelerationOnGround()
    {
        frictionTorque = fX * wheelRadius;
        var totalTorque = driveTorque + frictionTorque;
        wheelAngularAcc = totalTorque / wheelInertia;
        wheelAngularVelocity += wheelAngularAcc * deltaTime;
        

    }

    private void GetSx()
    {
        var targetAngularVelocity = wheelVelocityLS.z / wheelRadius;
        var targetAngularAcceleration = (wheelAngularVelocity - targetAngularVelocity) / deltaTime;
        var targetFrictionTorque = targetAngularAcceleration * wheelInertia;
        //var maxFriction = forceZ * wheelRadius * frictionCoefficient;
        if (forceZ != 0)
        {
            //sX = targetFrictionTorque / maxFriction;
        }
        else
        {
            sX = 0; //Divide by zero fix
        }
        sX = Mathf.Clamp(sX, -1, 1); //temp action
    }

    private void WheelAngularVelocity()
    {
        

        //float brakingTorque = brakeTorque * Mathf.Sign(wheelAngularVelocity);
        //rollingResistanceTorque = forceZ * (rollingResistanceCoefficient / 1000) * wheelAngularVelocity;
        //frictionTorque = fX * wheelRadius;
        //totalTorque = driveTorque - (frictionTorque + rollingResistanceTorque + shaftResistanceTorque);
       var wheelAngularAcceleration = driveTorque / wheelInertia;
       wheelAngularVelocity += wheelAngularAcceleration * deltaTime;
        //if (Input.GetAxisRaw("Brakes") != 0 && MPH <= 1.5f)
        //{
        //    wheelAngularVelocity = Mathf.MoveTowards(wheelAngularVelocity, 0, deltaTime * 6);
        //}
        //else
        //{

        //}

       // longSlipVelocity = longSlip;
       // longSlipVelocity = ((wheelAngularVelocity * wheelRadius) - wheelVelocityLS.z);
       // wheelInertia = Mathf.Pow(wheelRadius, 2) * wheelMass * 0.5f;
       // frictionTorque = (Mathf.Max(0, forceZ) * wheelRadius * Mathf.Clamp(longSlipVelocity / -strangeFactor, -1, 1));
       // var wheelAngularAcceleration = (driveTorque + frictionTorque) / wheelInertia;
       // wheelAngularVelocity += (wheelAngularAcceleration) * deltaTime;
       // //wheelAngularVelocity = Mathf.Clamp(wheelAngularVelocity, -120, 120); //temp action
       // //wheelAngularVelocity = Mathf.Clamp(wheelAngularVelocity, -ce.EngineMaxAngularVelocity() / ct.currentGearRatio, ce.EngineMaxAngularVelocity() / ct.currentGearRatio); //temp action



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
            
            var slipAngle = Mathf.Atan((wheelVelocityLS.x / wheelVelocityLS.z) * Mathf.Rad2Deg);
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


    private void AddTireForce()
    {
         var forwardForceVectorNormalized = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
         var sideForceVectorNormalized = Vector3.ProjectOnPlane(transform.right, hit.normal).normalized;
         rb.AddForceAtPosition(longitudinalForce*forceZ*frictionCoefficientX * forwardForceVectorNormalized + lateralForce *forceZ * sideForceVectorNormalized, transform.position - (transform.up * currentLength));

       // var forwardForceVectorNormalized = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
       // var sideForceVectorNormalized = Vector3.ProjectOnPlane(transform.right, hit.normal).normalized;
       // var combinedForceVectorNormalized = (forwardForceVectorNormalized * Mathf.Max(forceZ, 0) * fX) + (sideForceVectorNormalized * Mathf.Max(forceZ, 0) * lateralForce);
       //
       //// fX = sX * Mathf.Max(forceZ, 0);
       //
       // rb.AddForceAtPosition(combinedForceVectorNormalized, transform.position - (transform.up * currentLength));

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

   

}
