using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastWheels : MonoBehaviour
{
    public GameObject[] topLink;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i <topLink.Length; i++)
        {
            var startRay = topLink[i].transform.position;
           //var endRay = startRay * Vector3.up * 100f ;
            //Debug.Log(endRay);
            //Debug.DrawRay(startRay, endRay);
            //Debug.Log(topLink.Length);
            

        }
        
    }
}
