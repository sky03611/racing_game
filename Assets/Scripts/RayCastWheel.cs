using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastWheel : MonoBehaviour
{
    [HideInInspector] public RaycastHit hit;
    private Rigidbody rb;
    private CarController cc;
    private debugUI debugUI;
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
    public float wheelInertia;
    private float wheelAngularVelocity;
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
    private float maxWheelSpeed;
    private float longSlipVelocity;
    private float lateralSlipNormalized;
    private float longSlipNormalized;
    private float transmissionTorque;
    private float wheelangularVelDeg;
    private Vector2 longLateralVector;
    public float longStiffness = 1f;

    public float steerAngle;
    public float cornerStiffness;
    private float wheelAngle;
    private CarEngine ce;
    private CarTransmission ct;
    private bool isDriven;
    private Vector3 currentwheelVelocity;

    private float driveAngularVelocity;

    private float aboba3;
    private float wheelAngularVelocityTest;







    //newMethod
   public  float currentLength;
   public  float fZ;
   public  Vector3 fZForce;
   public  float deltaTime;
   public  float frictionTorque;
   public  float wheelAngularAcceleration;
   public  float totalTorque;
   public  float fX;
   public  float driveTorque;
   public  float targetAngularVelocity;
   public  float targetFrictionTorque;
   public  float targetAngularAcceleration;
   public  float sX;
   public  float maxFriction;
   public  float frictionCoefficient;
   public  float sY;
    [Range(0, 100)]
    public float slopeGrade;
    public  float slipAnglePeak;
   public  float slipAngleDynamic;
   public  float slipAngle;
   public  float slidingCoefficient;
   public  float relaxationLength;

    private Vector3 sideForceVectorNormalized;
    private Vector3 forwardForceVectorNormalized;
    private Vector3 combinedForceVectorNormalized;





    // Start is called before the first frame update
    void Start()
    {
        isDriven = false;
        ce = transform.root.GetComponent<CarEngine>();
        cc = transform.root.GetComponent<CarController>();
        rb = transform.root.GetComponent<Rigidbody>();
        ct = transform.root.GetComponent<CarTransmission>();
        debugUI = transform.root.GetComponent<debugUI>();
        //rayLength = springTravel + springRestLength + wheelRadius;
        //rayMinLenth = springRestLength - springTravel;
    }

    void Update()
    {
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, Time.deltaTime * cc.steerBackTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, Time.fixedDeltaTime * cc.steerBackTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
        //ApplySuspinsionForces();
        //
        //WheelAngularVelocity();
        //GetWheelSlipCombined();
        //WheelEngineSync();
        Debugger();
        //WheelRolling();
    }

    public void Initialize()
    {
        rb = transform.root.GetComponent<Rigidbody>();
        wheelInertia = (Mathf.Pow(wheelRadius, 2) * wheelMass) / 2;
        springLastLength = springRestLength;
    }

    public void UpdatePhysics(float delta, float torque)
    {
        deltaTime = delta;
        driveTorque = torque;
        RaycastSingle();
        GetSuspensionForce();
        ApplySuspensionForce();
        GetVelocityLocal();
        //WheelAcceleration();
        WheelAngularVelocity();
        TireFrictionTorque();
        GetWheelSlipCombined();
       // GetSx();
       // GetSy();
        AddTireForce();
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
        fZ = springForce + damperForce;
        fZForce = fZ * hit.normal.normalized;
        springLastLength = currentLength; //Update For Next Frame
    }

    private void ApplySuspensionForce()
    {
        rb.AddForceAtPosition(fZForce, transform.position);
    }

    private void GetVelocityLocal()
    {
        wheelVelocityLS = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
    }

    private void WheelAcceleration()
    {
        frictionTorque = fX * wheelRadius;
        totalTorque = driveTorque * Input.GetAxis("Vertical") - frictionTorque;
        
        wheelAngularAcceleration = totalTorque / wheelInertia;
        wheelAngularVelocity += wheelAngularAcceleration * deltaTime;
        wheelAngularVelocity = Mathf.Clamp(wheelAngularVelocity, -120, 120); //temp action

        // frictionTorque = fX * wheelRadius;
        // totalTorque = driveTorque - frictionTorque;
        //wheelAngularVelocity = Mathf.Sign(maxWheelSpeed) * Mathf.Min(Mathf.Abs(wheelAngularVelocity += (ct.GetTrasmissionTorque() / wheelInertia -frictionTorque) * deltaTime), Mathf.Abs(maxWheelSpeed));

      //  if (ct.TotalGearRatio() != 0)
      //  {
      //      maxWheelSpeed = ce.EngineAngularVelocity() / ct.TotalGearRatio();
      //  }
      //  else
      //  {
      //      maxWheelSpeed = 100;
      //  }
        
    }


   // public void WheelAngularVelocity(float delta)
   // {
   //     wheelInertia = Mathf.Pow(wheelRadius, 2) * wheelMass * 0.5f; //todo
   //     if (isDriven)
   //     {
   //         transmissionTorque = ct.GetTrasmissionTorque();
   //     }
   //     else
   //     {
   //         transmissionTorque = 0f;
   //     }
   //     //wheelAngularVelocityTest += Time.fixedDeltaTime * transmissionTorque / wheelInertia;
   //     //wheelAngularVelocity = Mathf.Min(maxWheelSpeed, wheelAngularVelocityTest);
   //     
   // }

        void GetSx()
    {
        targetAngularVelocity = wheelVelocityLS.z / wheelRadius;
        targetAngularAcceleration = (wheelAngularVelocity - targetAngularVelocity) / deltaTime;
        targetFrictionTorque = targetAngularAcceleration * wheelInertia;
        maxFriction = fZ * wheelRadius * frictionCoefficient;

        if (fZ != 0)
        {
            sX = targetFrictionTorque / maxFriction;
        }
        else
        {
            sX = 0; //Divide by zero fix
        }
        sX = Mathf.Clamp(sX, -1, 1); //temp action
    }

    //void GetSy()
    //{
    //    if (wheelVelocityLS.x != 0)
    //    {
    //        slipAngle = Mathf.Atan(-wheelVelocityLS.x / Mathf.Abs(wheelVelocityLS.z));
    //    }
    //    else
    //    {
    //        slipAngle = 0; //Divide by zero fix
    //    }
    //    slidingCoefficient = (Mathf.Abs(wheelVelocityLS.x) / (relaxationLength / 100)) * deltaTime;
    //    slidingCoefficient = Mathf.Clamp01(slidingCoefficient); //Important
    //
    //    slipAngleDynamic += (Mathf.Lerp(Mathf.Sign(-wheelVelocityLS.x) * (slipAnglePeak / 100), slipAngle, MapRangeClamped(wheelVelocityLS.magnitude, 3, 6, 0, 1)) - slipAngleDynamic) * slidingCoefficient;
    //    slipAngleDynamic = Mathf.Clamp(slipAngleDynamic, -slopeGrade / 1000, slopeGrade / 1000);
    //
    //    sY = slipAngleDynamic / (slipAnglePeak / 100);
    //    sY = Mathf.Clamp(sY, -1, 1); //temp action
    //}
    //
    //private float MapRangeClamped(float value, float inRangeA, float inRangeB, float outRangeA, float outRangeB)
    //{
    //    float result = Mathf.Lerp(outRangeA, outRangeB, Mathf.InverseLerp(inRangeA, inRangeB, value));
    //    return (result);
    //}
    void AddTireForce()
    {
        rb.AddForceAtPosition(forceX*transform.forward + forceY*transform.right, hit.point);
    }

    //public void ApplySuspinsionForces(float delta)
    //{
    //    Debug.Log("Rabotaem???");
    //    rayLength = springTravel + springRestLength + wheelRadius;
    //    springLastLength = springLength;
    //    if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, rayLength))
    //    {
    //        
    //        
    //        springLength = hit.distance - wheelRadius;
    //        springVelocity = (springLastLength - springLength) / delta;
    //        damperForce = damper * springVelocity;
    //        Mathf.Clamp(springLength, rayMinLenth, rayLength);
    //        springForce = (springStiffness * (springRestLength - springLength)) + damperForce;
    //        forceZ = springForce;
    //
    //        // wheelMesh.transform.localPosition = new Vector3(wheelMesh.transform.localPosition.x, wheelRadius - hit.distance, wheelMesh.transform.localPosition.z);
    //        finalForce = ((springForce * this.transform.up) + (forceY * transform.right) + (forceX * this.transform.forward));
    //        rb.AddForceAtPosition(finalForce, hit.point);
    //        Debug.DrawRay(transform.position, (-transform.up * rayLength));
    //        //wheelVelocityLS = this.transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
    //        IsGrounded(hit);
    //    }
    //}

    public void WheelAngularVelocity()
    {
        wheelInertia = Mathf.Pow(wheelRadius, 2) * wheelMass * 0.5f; //todo
        //wheelAngularVelocityTest += Time.fixedDeltaTime * transmissionTorque / wheelInertia;
        //wheelAngularVelocity = Mathf.Min(maxWheelSpeed, wheelAngularVelocityTest);
        wheelAngularVelocity = Mathf.Sign(maxWheelSpeed) * Mathf.Min(Mathf.Abs(wheelAngularVelocity += (ce.ToWheels() / wheelInertia) * deltaTime), Mathf.Abs(maxWheelSpeed));
        
    }

    public void TireFrictionTorque()
    {
        var frictionTorque = (Mathf.Max(0, fZ) * wheelRadius * Mathf.Clamp(longSlipVelocity / -15, -1, 1)) / wheelInertia * deltaTime;
        wheelAngularVelocity += frictionTorque;
        //max wheel speed on current gear
        if (isDriven)
        {
            if (ct.GetTotalGearRatio() != 0)
            {
                maxWheelSpeed = ce.EngineAngularVelocity() / ct.GetTotalGearRatio();
            }
            else
            {
                maxWheelSpeed = 100;
            }
        }
        else
        {
            maxWheelSpeed = wheelAngularVelocity;
        }
    }

    public void WheelRolling(float delta)
    {
        wheelangularVelDeg = (wheelAngularVelocity * Mathf.Rad2Deg) * deltaTime;
        wheelMesh.transform.Rotate(wheelangularVelDeg, 0, 0, Space.Self);  
    }

    private void Debugger()
    {

        if (wheelFL)
        {
            debugUI.WheelFrontLeftDebug(wheelAngularVelocity);
            debugUI.WheelFrontLeftDebug2(wheelVelocityLS.x);
            debugUI.WheelFrontLeftDebug3(wheelVelocityLS.y);
            debugUI.WheelFrontLeftDebug4(wheelVelocityLS.z);

            debugUI.EngineRPM(ce.GetEngineRPM());
            //debugUI.CurrentGear(ct.GetCurrentGear()-1);
            //debugUI.TransmissionTorque(ct.GetTrasmissionTorque());
            debugUI.CarSpeed(Mathf.Round(rb.velocity.magnitude * 3.6f));
            debugUI.EngineTorque(ce.GetEngineTorque());
        }
        if (wheelFR)
        {
            debugUI.WheelFrontRightDebug(wheelAngularVelocity);
            debugUI.WheelFrontRightDebug2(wheelVelocityLS.x);
            debugUI.WheelFrontRightDebug3(wheelVelocityLS.y);
            debugUI.WheelFrontRightDebug4(wheelVelocityLS.z);
        }
        if (wheelRL)
        {
            debugUI.WheeRearLeftDebug(wheelAngularVelocity);
            debugUI.WheelRearLeftDebug2(wheelVelocityLS.x);
            debugUI.WheelRearLeftDebug3(wheelVelocityLS.y);
            debugUI.WheelRearLeftDebug4(wheelVelocityLS.z);
        }
        if (wheelRR)
        {
            debugUI.WheeRearRightDebug(wheelAngularVelocity);
            debugUI.WheelRearRightDebug2(wheelVelocityLS.x);
            debugUI.WheelRearRightDebug3(wheelVelocityLS.y);
            debugUI.WheelRearRightDebug4(wheelVelocityLS.z);
        }
    }

    public void GetWheelSlipCombined()
    {
        if (isGrounded)
        {
            lateralSlipNormalized = Mathf.Clamp(wheelVelocityLS.x * cornerStiffness*-1, -1, 1);
            longSlipVelocity = wheelAngularVelocity * wheelRadius - wheelVelocityLS.z;
   
            if (wheelVelocityLS.z * longSlipVelocity > 0)
            {
                
                //longSlipNormalized = Mathf.Clamp(longSlipVelocity * longStiffness, -2, 2);
                longSlipNormalized = Mathf.Clamp(ce.ToWheels() / wheelRadius / Mathf.Max(0.00001f, fZ), -2, 2);
   
            }
            else
            {
   
                longSlipNormalized = Mathf.Clamp(longSlipVelocity * longStiffness, -2, 2);
            }
            //longLateralVector.x = longSlipNormalized;
            //longLateralVector.y = lateralSlipNormalized;
            Vector2 lateralLongVector = new Vector2(longSlipNormalized,lateralSlipNormalized);
            longLateralVector = lateralLongVector;
            //var combinedSlip = longLateralVector.magnitude;
            var combinedSlip = lateralLongVector.magnitude;
            Vector2 tireForceNormalized = slipForceCurve.Evaluate(combinedSlip) * longLateralVector.normalized;
            //Debug.Log(lateralSlipNormalized);
            forceX = tireForceNormalized.x * Mathf.Max(0.0f, fZ);
            forceY = tireForceNormalized.y * Mathf.Max(0.0f, fZ);
        }
        else
        {
            Debug.Log("No Gorund");
        }
        
    }

    public float GetWheelAngularVelocity()
    {
        return wheelAngularVelocity;
    }

    
}
