using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dashboard : MonoBehaviour
{
    private CarEngine ce;
    private CarTransmission ct;
    private Rigidbody rb;
    private float engineRpm;
    private float engineMaxRpm;
    private float tachoAngle;
    private float speedoAngle;
    private const float zeroTachoAngle = 135;
    private const float maxTachoAngle = -116; //-131
    public Transform tachoNeedle;
    public Text speedo;
    public Text currentGear;
    void Start()
    {
        ce = GetComponent<CarEngine>();
        ct = GetComponent<CarTransmission>();
        rb = GetComponent<Rigidbody>();
        engineMaxRpm = ce.engineMaxRpm;
    }

    public void SetEngineRpm()
    {
        engineRpm = ce.GetEngineRPM();
        
    }
    public void SetEngineMaxRpm(float maxRpm)
    {
        engineMaxRpm = ce.engineMaxRpm;
    }

    public void PhysicsUpdate()
    {
        Tachometer();
        SetEngineRpm();
        CurrentGear(ct.GetCurrentGear()-1);
        Speedo();
    }

    private void Tachometer()
    {
        float totalAngleSize = zeroTachoAngle - maxTachoAngle;
        float rpmNormalized = engineRpm / engineMaxRpm;
        tachoAngle = zeroTachoAngle - rpmNormalized * totalAngleSize;
        tachoNeedle.eulerAngles = new Vector3(0, 0, tachoAngle);
    }

    public void CurrentGear(float var)
    {
        if (var == 0)
        {
            currentGear.text = "N";
        }
        else if (var == -1)
        {
            currentGear.text = "R";
        }
        else
        {
            currentGear.text = var.ToString();
        }
    }

    public void Speedo()
    {
        speedo.text = Mathf.Round(rb.velocity.magnitude * 3.6f).ToString();
    }
}