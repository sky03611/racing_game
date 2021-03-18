using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FmodCarAduio : MonoBehaviour
{
    public FMODUnity.StudioEventEmitter fmod;
    private NrcEngine nrcEngine;
    private float blowOffTimer;
    // Start is called before the first frame update
    void Start()
    {
        fmod = GetComponent<FMODUnity.StudioEventEmitter>();
        nrcEngine = GetComponent<NrcEngine>();
    }

    public void GetTurboSound()
    {

    }
    void Update()
    {
        fmod.SetParameter("RPM", nrcEngine.engineRpm);
        
    }

    IEnumerator Blowoff()
    {
        blowOffTimer += 0.1f;
        blowOffTimer = Mathf.Clamp(blowOffTimer, 0, 1f);
        yield return new WaitForSeconds(0.1f);
    }
}
