using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NrcEngine : MonoBehaviour
{
    public AnimationCurve engineTorqueCurve;

    private float rpmToRadsSec;
    private float radsSecToRpm;
    [HideInInspector]
    public float engineRpm;
    [HideInInspector]
    public float engineTorque;
    public float startFriction = 50f;
    public float frictionCoefficient = 0.02f;
    public float engineInertia = 0.2f;
    [HideInInspector]
    public float engineAngularVelocity;
    public float maxEngineRpm = 7700;
    public float idleEngineRpm = 700;
    
    public void Initialize(float rpmToRads, float radsToRpm)
    {
        rpmToRadsSec = rpmToRads;
        radsSecToRpm = radsToRpm;
        engineAngularVelocity = 100;
    }

    // Update is called once per frame
    public void PhysicsUpdate(float throttleInput, float delta, float loadTorque)
    {
        engineTorque = engineTorqueCurve.Evaluate(engineRpm);
        //friction
        var friction = engineRpm * frictionCoefficient + startFriction;
        var engineInitialTorque = engineTorque + friction * throttleInput;
        var currentEffectiveTorque = engineInitialTorque - friction;
        //acceleration
        var engineAngularAcceleration = currentEffectiveTorque / engineInertia - loadTorque;
        //velocity = acceleration*deltaTime
        engineAngularVelocity += engineAngularAcceleration * delta; //these 2 strings can be written in 1 string
        //Clamping is not nescessary, can be playedg with frictioncoefficient
        engineAngularVelocity = Mathf.Clamp(engineAngularVelocity, idleEngineRpm *rpmToRadsSec, maxEngineRpm *rpmToRadsSec);
        engineRpm = Mathf.Max(engineAngularVelocity * radsSecToRpm, idleEngineRpm);

    }
}
