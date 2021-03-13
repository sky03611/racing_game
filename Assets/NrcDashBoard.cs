using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NrcDashBoard : MonoBehaviour
{
    private Rigidbody rb;
    private float engineRpm;
    private float engineMaxRpm;
    private float tachoAngle;
    private float speedoAngle;
    private const float zeroTachoAngle = 135;
    private const float maxTachoAngle = -93.11f; //-131
    private const float zeroSpeedoAngle = 135;
    private const float maxSpeedoAngle = -135; //-131
    public Transform tachoNeedle;
    public Transform speedoNeedle;
    public Text speedo;
    public Text currentGear;
    public void Initialize()
    {
        rb = GetComponent<Rigidbody>();
         
    }

    public void SetEngineRpm(float var)
    {
        engineRpm = var;

    }
    public void SetEngineMaxRpm(float maxRpm)
    {
        engineMaxRpm = maxRpm;
    }

    void Update()
    {
        Tachometer();
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
        float gear = var - 1;
        if (gear == 0)
        {
            currentGear.text = "N";
        }
        else if (gear == -1)
        {
            currentGear.text = "R";
        }
        else
        {
            currentGear.text = gear.ToString();
        }
    }

    public void Speedo()
    {
       // speedo.text = Mathf.Round(rb.velocity.magnitude * 3.6f).ToString();
        float totalAngleSizeSpd = zeroSpeedoAngle - maxSpeedoAngle;
        float spdNormalized = Mathf.Round(rb.velocity.magnitude * 3.6f) / 260;//maxspd
        speedoAngle = zeroSpeedoAngle - spdNormalized * totalAngleSizeSpd;
        speedoNeedle.eulerAngles = new Vector3(0, 0, speedoAngle);
    }
}