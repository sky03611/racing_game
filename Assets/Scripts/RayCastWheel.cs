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
    private float wheelInertia;
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

    private float aboba3;
    private float wheelAngularVelocityTest;
    // Start is called before the first frame update
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
        ApplySuspinsionForces();
        
        WheelAngularVelocity();
        GetWheelSlipCombined();
        WheelEngineSync();
        Debugger();
        WheelRolling();
    }
    
    public void SetDriveType()
    {
        isDriven = true;
    }

    private void WheelEngineSync()
    {
        if (cc.DriveTypeInt() == 0)
        {
            //awd
        }
        else if(cc.DriveTypeInt() == 1)
        {
            //fwd
        }
        else
        {
            //rwd
        }
    }

    private void ApplySuspinsionForces()
    {
        rayLength = springTravel + springRestLength + wheelRadius;
        springLastLength = springLength;
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, rayLength))
        {
            //wheelVelocityLS = this.transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
            wheelVelocityLS = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
            springLength = hit.distance - wheelRadius;
            springVelocity = (springLastLength - springLength) / Time.fixedDeltaTime;
            damperForce = damper * springVelocity;
            Mathf.Clamp(springLength, rayMinLenth, rayLength);
            springForce = (springStiffness * (springRestLength - springLength)) + damperForce;
            forceZ = springForce;

            // wheelMesh.transform.localPosition = new Vector3(wheelMesh.transform.localPosition.x, wheelRadius - hit.distance, wheelMesh.transform.localPosition.z);
            finalForce = ((springForce * this.transform.up) + (forceY * transform.right) + (forceX * this.transform.forward));
            rb.AddForceAtPosition(finalForce, hit.point);
            Debug.DrawRay(transform.position, (-transform.up * rayLength));
            IsGrounded(hit);
        }
    }

    private void WheelAngularVelocity()
    {
        wheelInertia = Mathf.Pow(wheelRadius, 2) * wheelMass * 0.5f; //todo
        if (isDriven)
        {
            transmissionTorque = ct.GetTrasmissionTorque();
        }
        else
        {
            transmissionTorque = 0f;
        }
        //wheelAngularVelocityTest += Time.fixedDeltaTime * transmissionTorque / wheelInertia;
        //wheelAngularVelocity = Mathf.Min(maxWheelSpeed, wheelAngularVelocityTest);
        wheelAngularVelocity = Mathf.Sign(maxWheelSpeed) * Mathf.Min(Mathf.Abs(wheelAngularVelocity += (ct.GetTrasmissionTorque() / wheelInertia) * Time.fixedDeltaTime), Mathf.Abs(maxWheelSpeed));
        TireFrictionTorque();
    }

    private void TireFrictionTorque()
    {
        var frictionTorque = (Mathf.Max(0, forceZ) * wheelRadius * Mathf.Clamp(longSlipVelocity / -15, -1, 1)) / wheelInertia * Time.fixedDeltaTime;
        wheelAngularVelocity += frictionTorque;
        //max wheel speed on current gear
        if (isDriven)
        {
            if (ct.TotalGearRatio() != 0)
            {
                maxWheelSpeed = ce.EngineAngularVelocity() / ct.TotalGearRatio();
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
        wheelangularVelDeg = (wheelAngularVelocity * Mathf.Rad2Deg) * Time.fixedDeltaTime;
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
            debugUI.TransmissionTorque(ct.GetTrasmissionTorque());
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
                longSlipNormalized = Mathf.Clamp(ct.GetTrasmissionTorque() / wheelRadius / Mathf.Max(0.00001f, forceZ), -2, 2);

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
            forceX = tireForceNormalized.x * Mathf.Max(0.0f, forceZ);
            forceY = tireForceNormalized.y * Mathf.Max(0.0f, forceZ);
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
