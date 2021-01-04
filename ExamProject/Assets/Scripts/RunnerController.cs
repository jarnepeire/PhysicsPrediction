using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunnerController : MonoBehaviour
{
    public RunnerCamera CameraControl;
    public Runner RunnerControl;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            CameraControl.ResetRunnerCamera();
            RunnerControl.ResetRunner();
        }
    }
}
