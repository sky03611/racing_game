using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarTransmission : MonoBehaviour
{
    [HideInInspector] public float currentGearRatio;

    public bool inGear;
    public float[] gearRatios;
    public float shiftTime;
    private string[] gearDisplayDictionary;
    public string gearDisplay;
    private int currentGear;
    private int nextGear;
    private float mainGear = 3.82f;
    private CarEngine ce;
    float fu;
    float deltaTime;
    int driveTorqueDivider;

    // Start is called before the first frame update
    public void Initialize(int driveType)
    {
        ce = GetComponent<CarEngine>();
        driveTorqueDivider = driveType;
        inGear = true;
        nextGear = 1;
        currentGear = 1;

        gearDisplayDictionary = new string[gearRatios.Length];
        gearDisplayDictionary[0] = "R";
        gearDisplayDictionary[1] = "N";
        for (int i = 2; i < gearRatios.Length; i++)
        {
            gearDisplayDictionary[i] = Convert.ToString(i - 1);
        }
    }

    void Update()
    {
        currentGearRatio = gearRatios[currentGear] *mainGear;
        gearDisplay = gearDisplayDictionary[currentGear];

    }


    public void PhysicsUpdate(float delta)
    {
        deltaTime = delta;
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
        fu = 0;
        if (currentGearRatio != 0)
        {
            return ce.GetEngineTorque() * currentGearRatio/driveTorqueDivider;
        }
        else
        {
            return 0;
        }

    }



    // Update is called once per frame
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