using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NrcTransmission : MonoBehaviour
{
    [HideInInspector] public float currentGearRatio;
    private NrcDashBoard nrcDashBoard;
    private bool inGear;
    public bool zf6HP19;
    public float[] gearRatios;
    public float shiftTime;
    public float transmissionTorque;
    private int currentGear;
    private int nextGear;
    private float mainGear = 3.82f;

    public void Initialize()
    {
        nrcDashBoard = GetComponent<NrcDashBoard>();
        inGear = true;
        nextGear = 1;
        currentGear = 1;
    }

    public void PhysicsUpdate()
    {
        currentGearRatio = gearRatios[currentGear] * mainGear;
        nrcDashBoard.CurrentGear(currentGear);
    }

   public void TransmissionTorque(float engineTorque)
    {
        transmissionTorque = engineTorque * currentGearRatio;
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
