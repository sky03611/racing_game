using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NrcRayCastWheel : MonoBehaviour
{
    //physics
    public AnimationCurve lateralPacejkaCurve;
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
    [Range(0,100)]
    public float slopeGrade;
    public float slipAnglePeak = 8f;
    public Transform wheelMesh;
    private float wheelInertia;
    [HideInInspector]
    public float wheelAngularVelocity;
    private float wheelAcceleration;
    private Vector3 linearVelocityLocal;
    //Slip
    private float sX;
    private float sY;
    private float slideCoeff;
    private float slipAngle; 
    private float slipAngleDynamic;
    private float fX;
    //test
    private float transientMax;
    private float transientCoeff;
    private float transientSY;
    private float slipAngleNormalized;
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
            WheelAcceleration();
            GetSx();
            GetSy();
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
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, deltaTime * steerTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
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
        rb.AddForceAtPosition(fZ * transform.up, transform.position);
    }

    private void WheelLinearVelocity()
    {
        linearVelocityLocal = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
    }
    private void WheelAcceleration()
    {
        var rollingResistanceTorque = fZ * (rollingResistanceCoefficient / 1000) * wheelAngularVelocity;
        var frictionTorque = fX * wheelRadius;
        var totalTorque = driveTorque - (frictionTorque+rollingResistanceTorque);
        wheelAcceleration = totalTorque / wheelInertia;
        wheelAngularVelocity += wheelAcceleration * deltaTime;
    }
    private void GetSx()
    {
        var targetAngularVelocity = linearVelocityLocal.z / wheelRadius;
        var targetAngularAcceleration = (wheelAngularVelocity - targetAngularVelocity) / deltaTime;
        var targetFrictionTorque = targetAngularAcceleration * wheelInertia;
        var maxFrictionTorque = fZ * wheelRadius * longFrictionCoefficient;

        if (fZ != 0)
        {
            sX = targetFrictionTorque / maxFrictionTorque;
        }
        else
        {
            sX = 0; //Divide by zero fix
        }
        sX = Mathf.Clamp(sX, -1, 1);
    }

    private void GetSy()
    {
         transientMax = Mathf.Sign(linearVelocityLocal.x / -1);
         transientCoeff = (Mathf.Abs(linearVelocityLocal.x) / (relaxationLength / 100)) * deltaTime;
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
            slipAngleNormalized = lateralPacejkaCurve.Evaluate(slipAngle);
        }
        else
        {
            slipAngleNormalized = lateralPacejkaCurve.Evaluate(-slipAngle) * -1;
        }

        sY = Mathf.Lerp(transientSY, slipAngleNormalized * 1f, MapRangeClamped(linearVelocityLocal.magnitude, slipAnglePeak, slipAnglePeak, 0, 1));

        //if (linearVelocityLocal.x != 0)
        //{
        //    slipAngle = Mathf.Atan(-linearVelocityLocal.x / Mathf.Abs(linearVelocityLocal.z));
        //}
        //else
        //{
        //    slipAngle = 0; 
        //}
        //slideCoeff = (Mathf.Abs(linearVelocityLocal.x) / (relaxationLength / 100)) * deltaTime;
        //slideCoeff = Mathf.Clamp01(slideCoeff); 
        //
        //slipAngleDynamic += (Mathf.Lerp(Mathf.Sign(-linearVelocityLocal.x) * (slipAnglePeak / 100), slipAngle, MapRangeClamped(linearVelocityLocal.magnitude, 3, 6, 0, 1)) - slipAngleDynamic) * slideCoeff;
        //slipAngleDynamic = Mathf.Clamp(slipAngleDynamic, -slopeGrade / 100, slopeGrade / 100);
        //
        //sY = slipAngleDynamic / (slipAnglePeak / 100);
        //sY = Mathf.Clamp(sY, -1, 1);
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
        fX = fzMax *sX;
        //SimpleFrictionForce
        //var tempY = Mathf.Clamp(linearVelocityLocal.x / -1f, -1, 1);
        //var combinedVector = forwardForceVectorNormalized * (fzMax *Input.GetAxis("Vertical")) + sideForceVectorNormalized * (fzMax * tempY);

        //AdvancedFrictionForce
        var combinedVector = forwardForceVectorNormalized * (fzMax *sX) + sideForceVectorNormalized * (fzMax * sY);
        rb.AddForceAtPosition(combinedVector, transform.position - (transform.up * currentLength));
 


        //rb.AddForceAtPosition(longitudinalForce * forceZ * frictionCoefficientX * forwardForceVectorNormalized + lateralForce * forceZ * sideForceVectorNormalized, transform.position - (transform.up * currentLength));
    }


}
