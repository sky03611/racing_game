using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NrcClutch : MonoBehaviour
{
    //SCript by AESTHETIC
    [SerializeField] public enum ClutchType { Manual, Automatic };
    public ClutchType clutchType;

    private float RadPS_To_RPM;
    private float clutchInput;

    private float clutchAngularVelocity;
    private float engineAngularVelocity;
    private float gearboxRatio;

    public float clutchSlip;
    public float clutchLock;
    public float clutchStiffness;
    public float clutchCapacity;
    private float lastClutchTorque;
    [Range(0, 0.9f)]
    public float clutchDamping;
    public float clutchTorque;

    // Start is called before the first frame update
    public void Initialize(float _RadPS_To_RPM)
    {
        RadPS_To_RPM = _RadPS_To_RPM;
    }

    // Update is called once per frame
    public void UpdatePhysics(float _outputShaftVelocity, float _engineAngularVelocity, float _gearboxRatio, float _clutchInput)
    {
        FrontPlate();
        BackPlate();
    }

    private void BackPlate()
    {
        
    }

    private void FrontPlate()
    {
        throw new NotImplementedException();
    }

    float MapRangeClamped(float value, float inRangeA, float inRangeB, float outRangeA, float outRangeB)
    {
        float result = Mathf.Lerp(outRangeA, outRangeB, Mathf.InverseLerp(inRangeA, inRangeB, value));
        return (result);
    }
}
