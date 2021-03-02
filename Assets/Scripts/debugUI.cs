using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class debugUI : MonoBehaviour
{
    // Start is called before the first frame update
    public Text wheelFLTextDebug;
    public Text wheelFRTextDebug;
    public Text wheelRLTextDebug;
    public Text wheelRRTextDebug;

    public Text wheelFLTextDebug2;
    public Text wheelFRTextDebug2;
    public Text wheelRLTextDebug2;
    public Text wheelRRTextDebug2;

    public Text wheelFLTextDebug3;
    public Text wheelFRTextDebug3;
    public Text wheelRLTextDebug3;
    public Text wheelRRTextDebug3;

    public Text wheelFLTextDebug4;
    public Text wheelFRTextDebug4;
    public Text wheelRLTextDebug4;
    public Text wheelRRTextDebug4;

    public Text currentGear;
    public Text transmissionTorque;
    public Text engineTorque;
    public Text engineRPM;
    public Text carSpeed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TransmissionTorque(float var)
    {
        transmissionTorque.text = var.ToString();
    }

    public void EngineRPM(float var)
    {
        engineRPM.text = var.ToString();
    }

    public void CurrentGear(float var)
    {
        if (var == 0)
        {
            currentGear.text = "N";
        }
        else if (var == -1)
        {
            currentGear.text = "R";
        }
        else
        {
            currentGear.text = var.ToString();
        }
        
    }

    public void EngineTorque(float var)
    {
        engineTorque.text = var.ToString();
    }

    public void CarSpeed(float var)
    {
        carSpeed.text = var.ToString() + " KM/H" ;
    }

    public void WheelFrontLeftDebug(float var)
    {
        wheelFLTextDebug.text = var.ToString();
    }
    public void WheelFrontRightDebug(float var)
    {
        wheelFRTextDebug.text = var.ToString();
    }
    public void WheeRearLeftDebug(float var)
    {
        wheelRLTextDebug.text = var.ToString();
    }
    public void WheeRearRightDebug(float var)
    {
        wheelRRTextDebug.text = var.ToString();
    }


    public void WheelFrontLeftDebug2(float var)
    {
        wheelFLTextDebug2.text = var.ToString();
    }
    public void WheelFrontRightDebug2(float var)
    {
        wheelFRTextDebug2.text = var.ToString();
    }
    public void WheelRearLeftDebug2(float var)
    {
        wheelRLTextDebug2.text = var.ToString();
    }
    public void WheelRearRightDebug2(float var)
    {
        wheelRRTextDebug2.text = var.ToString();
    }


    public void WheelFrontLeftDebug3(float var)
    {
        wheelFLTextDebug3.text = var.ToString();
    }
    public void WheelFrontRightDebug3(float var)
    {
        wheelFRTextDebug3.text = var.ToString();
    }
    public void WheelRearLeftDebug3(float var)
    {
        wheelRLTextDebug3.text = var.ToString();
    }
    public void WheelRearRightDebug3(float var)
    {
        wheelRRTextDebug3.text = var.ToString();
    }


    public void WheelFrontLeftDebug4(float var)
    {
        wheelFLTextDebug4.text = var.ToString();
    }
    public void WheelFrontRightDebug4(float var)
    {
        wheelFRTextDebug4.text = var.ToString();
    }
    public void WheelRearLeftDebug4(float var)
    {
        wheelRLTextDebug4.text = var.ToString();
    }
    public void WheelRearRightDebug4(float var)
    {
        wheelRRTextDebug4.text = var.ToString();
    }
}
