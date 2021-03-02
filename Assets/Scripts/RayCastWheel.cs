using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastWheel : MonoBehaviour
{
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
    public bool isGrounded = false;
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
    public float frictionCoefficientX;
    public float frictionCoefficientY;

    public float steerAngle;
    public float cornerStiffness;
    public float strangeFactor;
    private float wheelAngle;
    private CarEngine ce;
    private CarTransmission ct;
    private bool isDriven;

    private float aboba3;
    private float wheelAngularVelocityTest;
    //

    float deltaTime;
    float driveTorque;
    [HideInInspector]
    public RaycastHit hit;
    public float currentLength;
    float frictionTorque;
    void Start()
    {
        isDriven = false;
        ce = transform.root.GetComponent<CarEngine>();
        cc = transform.root.GetComponent<CarController>();
        rb = transform.root.GetComponent<Rigidbody>();
        ct = transform.root.GetComponent<CarTransmission>();
        debugUI = transform.root.GetComponent<debugUI>();
        rayLength = springTravel + springRestLength + wheelRadius;
        rayMinLenth = springRestLength - springTravel;
    }

    public void PhysicsUpdate(float delta, float torque)
    {
        //Debugger();
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
            TireFrictionTorque();

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
        //fZForce = fZ * hit.normal.normalized;
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
        wheelAngularVelocity = Mathf.Sign(maxWheelSpeed)* Mathf.Min(Mathf.Abs(wheelAngularVelocity += wheelAngularAcceleration), Mathf.Abs(maxWheelSpeed));
        wheelAngularVelocity = wheelAngularVelocity + frictionTorque;
        // wheelAngularVelocity = Mathf.Min(maxWheelSpeed, wheelAngularVelocityTest);
        ; //todo
        // wheelAngularVelocity = Mathf.Sign(maxWheelSpeed) * Mathf.Min(Mathf.Abs(wheelAngularVelocity += (driveTorque / wheelInertia) * deltaTime), Mathf.Abs(maxWheelSpeed));
         if (ct.GetTotalGearRatio() != 0)
         {
             maxWheelSpeed = ce.EngineAngularVelocity() / ct.GetTotalGearRatio();
         }
         else
         {
             maxWheelSpeed = 100;
         }
    }  //

    private void TireFrictionTorque()
    {
        
       // wheelAngularVelocity += frictionTorque;
        //max wheel speed on current gear
        

        if (Input.GetKey(KeyCode.Space))
        {
            if (wheelRL)
                wheelAngularVelocity = 0;
            if (wheelRR)
                wheelAngularVelocity = 0;
        }
    }

    private void WheelRolling()
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
            debugUI.CurrentGear(ct.GetCurrentGear()-1);
            debugUI.TransmissionTorque(driveTorque);
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

        

        Debug.DrawLine(transform.position,(new Vector3(forceX,0,forceY)), Color.red);
       // Debug.DrawLine(this.transform.position,  (forceY * transform.right) -this.transform.position, Color.cyan);
        //Debug.DrawLine(this.transform.position, forceY * -transform.right);
        //forceX = forceZ * Input.GetAxis("Vertical");
        //forceY = forceZ * Input.GetAxis("Horizontal");
    }

    private void GetWheelSlipCombined()
    {
        if (isGrounded)
        {
            lateralSlipNormalized = Mathf.Clamp(wheelVelocityLS.x * cornerStiffness*-1, -1, 1);
            longSlipVelocity = wheelAngularVelocity * wheelRadius - wheelVelocityLS.z;

            if (wheelVelocityLS.z * longSlipVelocity > 0)
            {
                
                //longSlipNormalized = Mathf.Clamp(longSlipVelocity * longStiffness, -2, 2);
                longSlipNormalized = Mathf.Clamp(driveTorque / wheelRadius / Mathf.Max(0.00001f, forceZ), -2, 2);

            }
            else
            {

                longSlipNormalized = Mathf.Clamp(longSlipVelocity * longStiffness, -1, 1);
            }
            //longLateralVector.x = longSlipNormalized;
            //longLateralVector.y = lateralSlipNormalized;
            Vector2 lateralLongVector = new Vector2(longSlipNormalized,lateralSlipNormalized);
            longLateralVector = lateralLongVector;
            //var combinedSlip = longLateralVector.magnitude;
            var combinedSlip = lateralLongVector.magnitude;
            Vector2 tireForceNormalized = slipForceCurve.Evaluate(combinedSlip) * longLateralVector.normalized;
            //Debug.Log(lateralSlipNormalized);
            
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
        else
        {
            Debug.Log("No Gorund");
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

    private void IsGrounded(RaycastHit hit)
    {
        if (hit.collider)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }
}
