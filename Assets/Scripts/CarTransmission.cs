using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarTransmission : MonoBehaviour
{
    public float[] gearRatio;
    private float totalGearRatio;
    private int gear = 1;
    private float mainGear = 3.82f;
    private float efficiency = 0.8f;
    private bool isCoroutineExecuting;
    public float gearChangeTime;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(ChangeGear(true));
            //totalGearRatio = 0;
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            StartCoroutine(ChangeGear(false));
            //totalGearRatio = 0;
        }
        totalGearRatio = gearRatio[gear] * mainGear *0.5f; //TODO Оптимизация
    }

    public float TotalGearRatio()
    {
        return totalGearRatio;
    }

    IEnumerator ChangeGear(bool up)
    {
        
        if (isCoroutineExecuting)
        {
            Debug.Log("Break");
            yield break;
        }
            
        isCoroutineExecuting = true;
        yield return new WaitForSeconds(gearChangeTime);
        if (up && gear < gearRatio.Length - 1)
        {
            gear++;
            Debug.Log("FCurrentGear= " + gear);
            //totalGearRatio = gearRatio[gear] * mainGear;
        }

        if (!up && gear > 0)
        {
            gear--;
            Debug.Log("RCurrentGear= " + gear);
            //totalGearRatio = gearRatio[gear] * mainGear;
        }
        isCoroutineExecuting = false;
        
    }
}
