using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NrcTransmission : MonoBehaviour
{
    private bool inGear;
    [HideInInspector]
    public float currentGearRatio;
    [HideInInspector]
    public float inputShaftVelocity;
    public float[] gearRatio;
    public float shiftTime;
    private int currentGear;
    private int nextGear;

    private float inputTorque;
    public float outputTorque;


    public void Initialize()
    {
        inGear = true;
        nextGear = 1;
        currentGear = 1;
    }

    public void PhysicsUpdate(float clutchInputTorque)
    {
        inputTorque = clutchInputTorque;
        currentGearRatio = gearRatio[currentGear]; //TODO Revamp
        GetOutputTorque();
    }



    private void GetOutputTorque()
    {
        outputTorque = inputTorque * gearRatio[currentGear];
    }

    public void GetInputShaftVelocity(float inputVelocity)
    {
        inputShaftVelocity = inputVelocity * gearRatio[currentGear];
    }

    public void GearUp()
    {
        if (inGear && currentGear != 0 &&currentGear<gearRatio.Length-1)
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
