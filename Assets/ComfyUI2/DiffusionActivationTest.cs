using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiffusionActivationTest : MonoBehaviour
{
    public ComfyOrganizer comfyOrg;
    public GameObject testGameObj;

    public DiffusionRequest diffReq;
    
    // Start is called before the first frame update
    void Start()
    {
        comfyOrg.SendDiffusionRequest(diffReq);
    }
}
