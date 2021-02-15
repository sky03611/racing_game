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
    private float wheelForwardVelocity;
    private float maxWheelSpeed;
    private float longSlipVelocity;
    private float lateralSlipNormalized;
    private float longSlipNormalized;
    private float lateralLongSlipNormalized;
    private Vector2 aboba; //todo
    public float longStiffness = 1f;

    public float steerAngle;
    public float cornerStiffness;
    private float wheelAngle;
    private CarEngine ce;
    private CarTransmission ct;
    public bool isDriven;

    private float aboba3;
    private float aboba2;
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
            //WheelRolling();


        
    }

    private void WheelAngularVelocity()
    {
        //angular acc = torque/inertia
        wheelInertia = Mathf.Pow(wheelRadius, 2) * wheelMass *0.5f; //todo

        var angAcc = ce.GetEngineTransTorque() / wheelInertia;
        //wheelAngularVelocity +=angAcc * Time.deltaTime; //todo

        //maxwheelspeed aboba3 = ct.TotalGearRatio;
        //aboba2 = max wheel speed
        if (ct.TotalGearRatio() == 0)
        {
            aboba3 = 999999f;
        }
        aboba3 = ct.TotalGearRatio();
        aboba2 = ce.EngineAngularVelocity() / aboba3;

      //  //angular vel = torque/inertia*deltaTime;
      //  //wheelAngularVelocity = (wheelAngularVelocity+= (ce.GetEngineTorque() / wheelInertia)) *Time.deltaTime;
       wheelAngularVelocity = Mathf.Sign(maxWheelSpeed) * Mathf.Min(Mathf.Abs(wheelAngularVelocity += (ce.GetEngineTorque() / wheelInertia) *Time.deltaTime), Mathf.Abs(maxWheelSpeed));
      //  //max wheel speed on current gear
        if (ct.TotalGearRatio() != 0)
        {
            totalGearRatio = ct.TotalGearRatio();
            maxWheelSpeed = ce.EngineAngularVelocity() / ct.TotalGearRatio(); ;
        }
      //  else
      //  {
      //      totalGearRatio = 1f;
      //      maxWheelSpeed = 99999;
      //  }
       
        

    }

    private void ApplySuspinsionForces()
    {
        rayLength = springTravel + springRestLength + wheelRadius;
        springLastLength = springLength;
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, rayLength))
        {
            wheelVelocityLS = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
             //wheelForwardVelocity = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point)).z;
            springLength = hit.distance - wheelRadius;
            springVelocity = (springLastLength - springLength) / Time.fixedDeltaTime;
            damperForce = damper * springVelocity;
            Mathf.Clamp(springLength, rayMinLenth, rayLength);
            springForce = (springStiffness * (springRestLength - springLength)) + damperForce;
            wheelMesh.transform.localPosition = new Vector3(wheelMesh.transform.localPosition.x, wheelRadius - hit.distance, wheelMesh.transform.localPosition.z);
           //if (isDriven)
           //{
                finalForce = ((springForce * transform.up) + (forceY * transform.right) + (forceX * transform.forward));
            //}
           // if (!isDriven)
           // {
           //     finalForce = springForce * transform.up + (forceY * transform.right) +(forceX *transform.forward);
           // }
            rb.AddForceAtPosition(finalForce, hit.point);

            //fZ = springForce * -transform.up;
            Debug.DrawRay(transform.position, (-transform.up * rayLength));
        }
    }

    private void WheelRolling()
    {
        var angularVelocity = (aboba2 * Mathf.Rad2Deg)  * Time.deltaTime;

        //var angularVelocity = wheelVelocityLS.z / wheelRadius;
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
            wheelMesh.transform.Rotate(-1*angularVelocity, 0, 0, Space.Self);
        }
    }

    private void TireFriction()
    {

        //forceY = Mathf.Clamp(forceY, springForce*-1 * wheelVelocityLS.y * -1, springForce*-1 * wheelVelocityLS.y );
        //
        ////forceY = wheelVelocityLS.x * springForce *-1;
        //if (Input.GetAxis("Vertical") != 0)
        //{
        //    forceX =Input.GetAxis("Vertical") * 2200;
        //
        //}
        
       // Mathf.Clamp(frictionForce, fZ.y * -1, fZ.y);
    }

    private void GetWheelSlipCombined()
    {
        longSlipVelocity = wheelAngularVelocity * wheelRadius - wheelVelocityLS.x;
        lateralSlipNormalized = Mathf.Clamp(wheelVelocityLS.y * cornerStiffness *-1, -1, 1);

       if(wheelVelocityLS.x * longSlipVelocity > 0)
        {
            longSlipNormalized = Mathf.Clamp(ce.GetEngineTransTorque() / wheelRadius / Mathf.Max(0.00001f, springForce),-2,2);

        }
        else
        {
            longSlipNormalized = Mathf.Clamp(longSlipVelocity * longStiffness, -2, 2);
            Debug.Log("PickMe");
        }
        aboba.x = longSlipNormalized;
        aboba.y = lateralSlipNormalized;
        Debug.Log("Aboba = " +aboba);
        var combinedSlip = aboba.magnitude;
        var tireForceNormalized = slipForceCurve.Evaluate(combinedSlip) * aboba.normalized * Mathf.Max(0.0f, springForce);
        Debug.Log("tireforceNorma=" + tireForceNormalized);
        Debug.Log("abobaNorma= " + aboba.normalized);
        Debug.Log("combinedSlip= " + combinedSlip);
        Debug.Log("slipForceCurve= " + slipForceCurve.Evaluate(combinedSlip));
        Debug.Log("EngineTransTorque= " + slipForceCurve.Evaluate(combinedSlip));

        forceX = tireForceNormalized.x;
        //forceY = tireForceNormalized.y;

       // Debug.Log(tireForceNormalized.x);

        ////LateralSlip
        ////cornerStiffness = 0.5f; //TODO
        //longSlipVelocity = aboba2 * wheelRadius - wheelVelocityLS.x;
        ////Debug.Log("longslipvel" + longSlipVelocity);
        //lateralSlipNormalized = Mathf.Clamp(wheelVelocityLS.y * cornerStiffness *-1, -1, 1);
        ////lateralLongSlipNormalized = Mathf.Clamp (wheelVelocityLS.x * longSlipVelocity * longStiffness, -2, 2);
        //
        ////if carspeed * slipspeed>0 ==> traction; else friction;  
        //if (wheelVelocityLS.x * longSlipVelocity>0)
        //{
        //    var traction = ce.GetEngineTorque() / wheelRadius / Mathf.Max(0.00001f, springForce *-1);
        //
        //    longSlipNormalized = traction; //Mathf.Clamp(traction, -2, 2);
        //}
        //else
        //{
        //    longSlipNormalized = Mathf.Clamp(longSlipVelocity * longStiffness, -2, 2);
        //}
       //// Debug.Log("LongSlipNormalized= " + longSlipNormalized);
        //aboba = new Vector2(longSlipNormalized, lateralSlipNormalized);
        //
        //var tireForceNormalized = slipForceCurve.Evaluate(aboba.magnitude) *aboba.normalized * Mathf.Max(0.1f, springForce *-1);
        //Debug.Log(forceX);
        //forceX = tireForceNormalized.x;
        //forceY = tireForceNormalized.y;

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
