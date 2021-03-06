using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarTransmission : MonoBehaviour
{
    [HideInInspector] public float currentGearRatio;
    private CarEngine ce;
    private bool inGear;
    public bool zf6HP19;
    public float[] gearRatios;
    public float shiftTime;
    private int currentGear;
    private int nextGear;
    private float mainGear = 3.82f;
    private int driveTorqueDivider;

    public void Initialize(int driveType)
    {
        ce = GetComponent<CarEngine>();
        driveTorqueDivider = driveType;
        inGear = true;
        nextGear = 1;
        currentGear = 1;
    }

    void Update()
    {
        currentGearRatio = gearRatios[currentGear] *mainGear;
    }

    public void PhysicsUpdate(float delta)
    {
        AutomaticGearBox();
    }

    public float GetTotalGearRatio()
    {
        return currentGearRatio ;
    }

    public int GetCurrentGear()
    {
        return currentGear;
    }

    public float GetTransmissionTorque()
    {
        if (currentGearRatio != 0)
        {
            return ce.GetEngineTorque() * currentGearRatio/driveTorqueDivider;
        }
        else
        {
            return 0;
        }
    }

    void AutomaticGearBox() 
    {
        if (zf6HP19)
        {
            if (ce.GetEngineRPM() > 6900 &&currentGear!=6 && !ce.clutchEng)
            {
                    Debug.Log("GearUp");
                currentGear++;
            }
            if (ce.GetEngineRPM() < 3500 && currentGear>2 && !ce.clutchEng)
            {
                currentGear--;
                Debug.Log("GearDown");
            }
        }
    }

    public void GearUp()
    {
        if (inGear && currentGear != gearRatios.Length - 1)
        {
            if (currentGear != 1)
            {
                nextGear++;
                StartCoroutine(GearChange(nextGear, shiftTime));
            }
            else
            {
                nextGear++;
                currentGear = nextGear;
            }
        }
    }

    public void GearDown()
    {
        if (inGear && currentGear != 0)
        {
            if (currentGear != 1)
            {
                nextGear--;
                StartCoroutine(GearChange(nextGear, shiftTime));
            }
            else
            {
                nextGear--;
                currentGear = nextGear;
            }
        }
    }

    IEnumerator GearChange(int nextGear, float shiftTime)
    {
        inGear = false;
        currentGear = 1;
        yield return new WaitForSeconds(shiftTime);
        currentGear = nextGear;
        inGear = true;
    }
}