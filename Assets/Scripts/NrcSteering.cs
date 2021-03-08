using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NrcSteering : MonoBehaviour
{
    public float steerInputs;
    public float ackermannAngleLeft;
    public float ackermannAngleRight;
    public float wheelBase;
    public float turnRadius;
    public float rearTrack;
    public float steerFactor;
    private NewPhysicsUpdate newPhysics;
    // Start is called before the first frame update
    public void Initialize()
    {
        
    }

    // Update is called once per frame
    public void PhysicsUpdate(float steerInput)
    {
        steerInputs = steerInput;
        AckerMannSteering();
    }

    void AckerMannSteering()
    {
        if (steerInputs > 0)
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrack)) * steerInputs * steerFactor;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrack)) * steerInputs * steerFactor;
        }
        else if (steerInputs < 0)
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrack)) * steerInputs * steerFactor;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrack)) * steerInputs * steerFactor;
        }
        else
        {
            ackermannAngleLeft = 0;
            ackermannAngleRight = 0;
        }
    }
}
