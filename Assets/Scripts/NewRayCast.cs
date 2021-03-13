using UnityEngine;

public class NewRayCast : MonoBehaviour
{
    [HideInInspector] public RaycastHit hit;
    private Rigidbody rb;

    private float deltaTime;
    private float driveTorque;
    public bool isGrounded;

    public Transform wheelMesh;

    public bool wheelFR;
    public bool wheelFL;

    public float wheelMass;

    public float restLength;
    public float currentLength;
    private float lastLength;
    public float wheelRadius;
    private float R;

    public float springForce;
    public float springStiffness;
    public float damperForce;
    public float damperStiffness;
    public float fZ;
    public Vector3 fZForce;

    public Vector3 linearVelocityLocal;
    public float visualWheelRot;


    private Vector3 forwardForceVectorNormalized;
    private Vector3 sideForceVectorNormalized;
    public Vector3 combinedForceVectorNormalized;

    public float sX;

    public float relaxationLength;
    private float slidingCoefficient;
    private float slipAngle;
    private float slipAngleDynamic;
    public float slipAnglePeak;
    public float sY;
    [Range(0, 100)]
    public float slopeGrade;

    public float fX;
    //public float fY;

    private float wheelInertia;
    private float wheelAngularAcceleration;
    public float wheelAngularVelocity;
    private float targetAngularVelocity;
    private float targetAngularAcceleration;
    private float targetFrictionTorque;
    public float frictionCoefficient;
    private float maxFriction;
    private float frictionTorque;
    private float totalTorque;

    private float steerAngle;
    private float wheelAngle;

    private float currentGearRatio;
    private float engineAngularVelocity;

    // Start is called before the first frame update
    public void Initialize()
    {
        rb = transform.root.GetComponent<Rigidbody>();
        R = wheelRadius / 100;
        wheelInertia = (Mathf.Pow(wheelRadius, 2) * wheelMass) / 2;
        lastLength = restLength;
    }

    // Update is called once per frame
    public void UpdatePhysics(float _deltaTime, float _driveTorque, float turnAngleL, float turnAngleR, float ratio, float engineVelocity)
    {
        if (wheelFL)
        {
            steerAngle = turnAngleL;
        }
        if (wheelFR)
        {
            steerAngle = turnAngleR;
        }

        
        deltaTime = _deltaTime;
        driveTorque = _driveTorque;
        currentGearRatio = ratio;
        engineAngularVelocity = engineVelocity;
        Steering();
        RaycastSingle();
        WheelRolling();


        if (isGrounded)
        {
            GetSuspensionForce();
            ApplySuspensionForce();
            GetVelocityLocal();
            WheelAcceleration();
            GetSx();
            GetSy();
            AddTireForce();
        }
        else
        {
            ResetValues();
        }
    }



    void RaycastSingle()
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

    void ResetValues()
    {
        currentLength = lastLength = restLength;
        fZ = 0;
        fZForce = Vector3.zero;
    }

    void GetSuspensionForce()
    {
        springForce = (restLength - currentLength) * springStiffness;
        damperForce = ((lastLength - currentLength) / deltaTime) * damperStiffness;
        fZ = springForce + damperForce;
        fZForce = fZ * hit.normal.normalized;
        lastLength = currentLength; //Update For Next Frame
    }

    void ApplySuspensionForce()
    {
        rb.AddForceAtPosition(fZForce, transform.position);
    }

    void GetVelocityLocal()
    {
        linearVelocityLocal = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
    }

    void WheelAcceleration()
    {
        frictionTorque = fX * wheelRadius;
        totalTorque = driveTorque - frictionTorque;
        wheelAngularAcceleration = totalTorque / wheelInertia;
        wheelAngularVelocity += wheelAngularAcceleration * deltaTime;
        wheelAngularVelocity = Mathf.Clamp(wheelAngularVelocity, -engineAngularVelocity/currentGearRatio, engineAngularVelocity/currentGearRatio); //temp action
        Debug.Log(driveTorque);
    }

    void GetSx()
    {
        targetAngularVelocity = linearVelocityLocal.z / wheelRadius;
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

    void GetSy()
    {
        if (linearVelocityLocal.x != 0)
        {
            slipAngle = Mathf.Atan(-linearVelocityLocal.x / Mathf.Abs(linearVelocityLocal.z));
        }
        else
        {
            slipAngle = 0; //Divide by zero fix
        }
        slidingCoefficient = (Mathf.Abs(linearVelocityLocal.x) / (relaxationLength / 100)) * deltaTime;
        slidingCoefficient = Mathf.Clamp01(slidingCoefficient); //Important

        slipAngleDynamic += (Mathf.Lerp(Mathf.Sign(-linearVelocityLocal.x) * (slipAnglePeak / 100), slipAngle, MapRangeClamped(linearVelocityLocal.magnitude, 3, 6, 0, 1)) - slipAngleDynamic) * slidingCoefficient;
        slipAngleDynamic = Mathf.Clamp(slipAngleDynamic, -slopeGrade / 1000, slopeGrade / 1000);

        sY = slipAngleDynamic / (slipAnglePeak / 100);
        sY = Mathf.Clamp(sY, -1, 1); //temp action
    }

    private float MapRangeClamped(float value, float inRangeA, float inRangeB, float outRangeA, float outRangeB)
    {
        float result = Mathf.Lerp(outRangeA, outRangeB, Mathf.InverseLerp(inRangeA, inRangeB, value));
        return (result);
    }

    void AddTireForce()
    {
        forwardForceVectorNormalized = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
        sideForceVectorNormalized = Vector3.ProjectOnPlane(transform.right, hit.normal).normalized;
        combinedForceVectorNormalized = (forwardForceVectorNormalized * Mathf.Max(fZ, 0) * sX) + (sideForceVectorNormalized * Mathf.Max(fZ, 0) * sY);

        fX = sX * Mathf.Max(fZ, 0);

        rb.AddForceAtPosition(combinedForceVectorNormalized, transform.position - (transform.up * currentLength));
    }

    void Steering()
    {
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, deltaTime * 2);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
    }

    private void WheelRolling()
    {
        wheelMesh.transform.Rotate((wheelAngularVelocity * Mathf.Rad2Deg) * deltaTime, 0, 0, Space.Self);
    }

}
