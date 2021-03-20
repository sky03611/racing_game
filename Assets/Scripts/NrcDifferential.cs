using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NrcDifferential : MonoBehaviour
{
    private NrcTransmission nrcTransmission;
    public float differentialRatio =3.4f; //~for bmw e30 
    public float outputTorqueLeft;
    public float outputTorqueRight;
    public float differentialVelocity;

    public float tempDiffOutputTorque;
    public void Initialize()
    {
        nrcTransmission = GetComponent<NrcTransmission>();
    }

    // Update is called once per frame
    public void PhysicsUpdate(float transmissionTorque)
    {
        GetOutputTorque(transmissionTorque);
    }

    public void GetOutputTorque(float transmissionTorque)
    {
        outputTorqueLeft = transmissionTorque * 0.5f *differentialRatio;
        outputTorqueRight = transmissionTorque * 0.5f * differentialRatio;

    }

    public void GeInputShaftVelocity(float leftWheelVel, float rightWheelVel)
    {
        //Velocity of differential
        differentialVelocity = (leftWheelVel + rightWheelVel) * 0.5f * differentialRatio;
        
    }
}
