using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysycsUpdate : MonoBehaviour
{
    private float delta;
    private CarEngine ce;
    private CarTransmission ct;
    private CarController cc;
    
    void Start()
    {
        ce = GetComponent<CarEngine>();
        ct = GetComponent<CarTransmission>();
        cc = GetComponent<CarController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        delta = Time.fixedDeltaTime;
    }
}
