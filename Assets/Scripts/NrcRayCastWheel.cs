using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NrcRayCastWheel : MonoBehaviour
{
    //physics
    public AnimationCurve longSlipCurve;
    public AnimationCurve lateralSlipCurve;
    private float deltaTime;
    //compoents
    private Rigidbody rb;
    //Suspension
    public float restLength;
    public float springStiffness;
    public float damperStiffness;
    private float damperForce;
    private float lastLength;
    private float currentLength;
    private float springForce;
    private float fZ;
    //lineTrace
    private RaycastHit hit;
    private bool isGrounded = false;
    //WHeelParams
    public bool wheelFL;
    public bool wheelFR;
    public bool wheelRL;
    public bool wheelRR;
    public float wheelRadius = 0.3f;
    public float wheelMass = 20f;
    public float relaxationLength;
    public float rollingResistanceCoefficient;
    public float longFrictionCoefficient = 1f;
    public float lateralFrictionCoefficient = 1f;
    public float rollingResistanceTorque;
    [Range(0,100)]
    public float slopeGrade;
    public float slipAnglePeak = 8f;
    public Transform wheelMesh;
    private float wheelInertia;
    
    public float wheelAngularVelocity;
    private float wheelAcceleration;
    private Vector3 linearVelocityLocal;
    private float longTransient;
    private float brakeTorque = 0f;

    //Slip
    public float lateralSlip=1f;
    public float longSlip=1f;
    private float sX;
    private float sY;
    private float slideCoeff;
    private float slipAngle; 
    private float slipAngleDynamic;
    private float fX;
    public float maxSlipAngle;
    private float slipRatio;
    public float thresholdTurnoverRate = 0f;
    //test
    private float tractionTorque;
    private float transientCoeff;
    private float transientSY;
    private float slipAngleNormalized;
    public float loadForce;
    Vector3 combindeFoceVectorNorm;

    private Vector3 w_prevPos;
    private float forwardVelocuty;
    private float sidewaysVelocuty;
    public float steerCoeff;
    //pacejka
    public float sB;
    public float sC;
    public float sE;
    public float sD;
    //steering
    [HideInInspector]
    public float steerAngle;
    private float wheelAngle;
    public float steerTime = 5f;
    //Engine
    private float driveTorque;

    public void Initialize()
    {
        rb = transform.root.GetComponent<Rigidbody>();
        wheelInertia = Mathf.Pow(wheelRadius, 2) * wheelMass * 0.5f;
    }

    public void PhysicsUpdate(float delta,float torqueFL, float torqueFR, float torqueRL, float torqueRR)
    {
        deltaTime = delta;
        GetDriveTorque(torqueFL,torqueFR,torqueRL,torqueRR);
        RaycastSingle();
        Steering();
        if (isGrounded)
        {
            GetSuspensionForce();
            ApplySuspinsionForce();
            WheelLinearVelocity();
            
            GetSx();
            //GetSy();
            GetFy();
            WheelAcceleration();
            UpdateWheelMesh();
            ApplyTireForce();
        }
    }

    private void GetDriveTorque(float fl, float fr, float rl, float rr)
    {
        if (wheelFL)
            driveTorque = fl;
        if (wheelFR)
            driveTorque = fr;
        if (wheelRL)
            driveTorque = rl;
        if (wheelRR)
            driveTorque = rr;
    }

    private void Steering()
    {
        steerAngle = Mathf.Clamp(steerAngle - slipAngle * steerCoeff, -40, 40); // todo
        if (wheelFL || wheelFR)
        {
            wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, deltaTime * steerTime);
            transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
        }
    }
    private void RaycastSingle()
    {
        if (Physics.Raycast(transform.position, -transform.up, out hit, (restLength + wheelRadius)))
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
        springForce = (restLength - currentLength) * springStiffness;
        damperForce = ((lastLength - currentLength) / deltaTime) * damperStiffness;
        fZ = springForce + damperForce;
        lastLength = currentLength; 
    }

    private void ApplySuspinsionForce()
    {
       rb.AddForceAtPosition(fZ * transform.up, transform.position - (transform.up * currentLength));
    }

    private void WheelLinearVelocity()
    {
        linearVelocityLocal = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
    }
    private void WheelAcceleration()
    {
        float frictionTorque = sX * longFrictionCoefficient * wheelRadius * Mathf.Max(fZ, 0);
        float frictionTorqueNegater = -frictionTorque * MapRangeClamped(Mathf.Abs(linearVelocityLocal.z), 0, 1, 1, 0);
        float rollingResistanceTorqueNegater = -rollingResistanceTorque * MapRangeClamped(Mathf.Abs(linearVelocityLocal.z), 0, 1, 1, 0);
        var totalTorque = driveTorque - (frictionTorque + rollingResistanceTorque + frictionTorqueNegater + rollingResistanceTorqueNegater);
        wheelAcceleration = totalTorque / wheelInertia;
        //var totalTorque = driveTorque - tractionTorque;
        if (brakeTorque != 0)
        {
            wheelAngularVelocity -= Mathf.Min(Mathf.Abs(wheelAngularVelocity), ((brakeTorque * Mathf.Sign(wheelAngularVelocity)) / wheelInertia) * deltaTime);
            //if (Mathf.Abs(linearVelocityLocal.z) <= 1)
            //{
            //    longTransient = frictionTorque * MapRangeClamped(Mathf.Abs(linearVelocityLocal.z), brakeTorqueTransientFactorCurve.Evaluate(brakeTorque), 1, 0, 1);
            //}
            //else
            //{
            //    longTransient = 0;
            //}
        }
        else
        {
            wheelAngularVelocity += wheelAcceleration * deltaTime;
            longTransient = 0;
        }
        //wheelAngularVelocity += wheelAcceleration * deltaTime;
    }
    private void GetSx()
    {
        //Sku Notes: 
        //Maximum grip is when car has about 6% of slipRatio. Make sure to make this in curve. Also slipRatio is in % (0-100) so either curve must be time -1 - 1 or -100 - 100 if slipRatio *= 100;
        //Info source https://asawicki.info/Mirror/Car%20Physics%20for%20Games/Car%20Physics%20for%20Games.html
        if (linearVelocityLocal.z == 0)
        {
            slipRatio = 0;
        }
        else
        {
            slipRatio = ((wheelAngularVelocity * wheelRadius) - linearVelocityLocal.z);
        }
        var slipRatioNormalized = longSlipCurve.Evaluate(slipRatio/100f);

        sX = slipRatioNormalized;
        Debug.Log("SX " +sX);
        
    }

    private void GetSy()
    {
        if (linearVelocityLocal.z != 0)
        {
            slipAngle = Mathf.Atan(linearVelocityLocal.x / linearVelocityLocal.z) * Mathf.Rad2Deg;
        }
        else
        {
            slipAngle = 0f;
        }
        var slipAngleNormalized = lateralSlipCurve.Evaluate(slipAngle);
        sY = slipAngleNormalized *lateralFrictionCoefficient *-1;
    }

    private void GetFy()
    {
        var transientMax = Mathf.Sign(-linearVelocityLocal.x);
        if (brakeTorque != 0 && rb.velocity.magnitude >= 0.25f)
        {
            transientCoeff = (Mathf.Abs(linearVelocityLocal.x) / (relaxationLength / 100)) * deltaTime;
        }
        else
        {
            transientCoeff = (Mathf.Abs(linearVelocityLocal.x) / ((relaxationLength * 10) / 100)) * deltaTime;
        }
        transientSY += (transientMax - transientSY) * transientCoeff;
        transientSY = Mathf.Clamp(transientSY, -slopeGrade / 100, slopeGrade / 100);

        if (linearVelocityLocal.z != 0)
        {
            slipAngle = Mathf.Atan(-linearVelocityLocal.x / Mathf.Abs(linearVelocityLocal.z)) * Mathf.Rad2Deg;
        }
        else
        {
            slipAngle = 0;
        }
        if (slipAngle > 0)
        {
            slipAngleNormalized = lateralSlipCurve.Evaluate(slipAngle);
        }
        else
        {
            slipAngleNormalized = lateralSlipCurve.Evaluate(-slipAngle) * -1;
        }

        sY = Mathf.Lerp(transientSY, slipAngleNormalized, MapRangeClamped(linearVelocityLocal.magnitude, slipAnglePeak, slipAnglePeak + thresholdTurnoverRate, 0, 1));
        Debug.Log(sY);
    }

    private float MapRangeClamped(float value, float inRangeA, float inRangeB, float outRangeA, float outRangeB)
    {
        float result = Mathf.Lerp(outRangeA, outRangeB, Mathf.InverseLerp(inRangeA, inRangeB, value));
        return (result);
    }

    private void UpdateWheelMesh()
    {
        
        var wheelangularVelDeg = (wheelAngularVelocity * Mathf.Rad2Deg) * deltaTime;
        wheelMesh.transform.Rotate(wheelangularVelDeg, 0, 0, Space.Self);
    }

    private void ApplyTireForce()
    {
        var forwardForceVectorNormalized = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
        var sideForceVectorNormalized = Vector3.ProjectOnPlane(transform.right, hit.normal).normalized;
        var fzMax = Mathf.Max(0, fZ);
        //AdvancedFrictionForce
        Vector2 combinedForce = new Vector2(sX, sY);
       // combindeFoceVectorNorm = (forwardForceVectorNormalized * sX + sideForceVectorNormalized * fzMax * sY);
        rb.AddForceAtPosition(combinedForce.normalized.x* forwardForceVectorNormalized * fzMax + combinedForce.normalized.y * fzMax * sideForceVectorNormalized, transform.position - (transform.up * currentLength));
        //Debug.Log(aboba);
   }


}
