using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breaks : MonoBehaviour
{
    public float breakBias;
    public float[] breakRatio = new float[2];
    public float breakStrength;
    private float breakValue;
    private float[] breakTorque = new float[2];
    private float[] wAngVel = new float [4];
    private CarController cc;
    public void Initialize()
    {
        cc = GetComponent<CarController>();
    }

    public void PhysicsUpdate(float delta, float input)
    {
        if (input < 0)
        {
            breakValue = input;



            for (int i = 0; i < breakTorque.Length; i++)
            {
                breakTorque[i] = breakRatio[i] * breakStrength * breakValue;

            }

            for (int i = 0; i < cc.rayCastWheels.Length - 2; i++)
            {
                wAngVel[i] = Mathf.Sign(cc.rayCastWheels[i].GetWheelAngularVelocity()) / cc.rayCastWheels[i].wheelInertia * delta * breakTorque[0];
            }

            for (int i = 2; i < cc.rayCastWheels.Length; i++)
            {
                wAngVel[i] = Mathf.Sign(cc.rayCastWheels[i].GetWheelAngularVelocity()) / cc.rayCastWheels[i].wheelInertia * delta * breakTorque[1];
                Debug.Log("BreakTorque= " + wAngVel[0]);
            }

            for (int i = 0; i < cc.rayCastWheels.Length; i++)
            {
                cc.rayCastWheels[i].SetWhelAngularVelocity(wAngVel[i]);
            }
        }

        else
        {
            for (int i = 0; i < cc.rayCastWheels.Length; i++)
            {
                Debug.Log("Set");
                cc.rayCastWheels[i].SetWhelAngularVelocity(0f);
            }
        }

    }
}
