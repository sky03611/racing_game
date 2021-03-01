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
    }

    private void Tachometer()
    {
        float totalAngleSize = zeroTachoAngle - maxTachoAngle;
        float rpmNormalized = engineRpm / engineMaxRpm;
        tachoAngle = zeroTachoAngle - rpmNormalized * totalAngleSize;
        tachoNeedle.eulerAngles = new Vector3(0, 0, tachoAngle);
    }
}