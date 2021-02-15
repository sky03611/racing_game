using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dashboard : MonoBehaviour
{
    private CarEngine ce;
    private float engineRpm;
    private float engineMaxRpm;
    private float tachoAngle;
    private float speedoAngle;
    private const float zeroTachoAngle = 135;
    private const float maxTachoAngle = -116; //-131
    public Transform tachoNeedle;
    public Transform speedoNeedle;
    void Start()
    {
        ce = GetComponent<CarEngine>();
    }

    public void SetEngineRpm(float currentRpm)
    {
        engineRpm = currentRpm;
    }

    public void SetEngineMaxRpm(float maxRpm)
    {
        engineMaxRpm = maxRpm;
    }

    void Update()
    {
        Tachometer();
    }

    private void Tachometer()
    {
        float totalAngleSize = zeroTachoAngle - maxTachoAngle;
        float rpmNormalized = engineRpm / engineMaxRpm;
        tachoAngle = zeroTachoAngle - rpmNormalized * totalAngleSize;
        tachoNeedle.eulerAngles = new Vector3(0, 0, tachoAngle);
    }
}
