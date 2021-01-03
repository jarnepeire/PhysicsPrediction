using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunnerCamera : MonoBehaviour
{
    private Runner _runner;
    private Camera _camera;
    private bool _isRunning;

    // Start is called before the first frame update
    void Start()
    {
        _isRunning = false;
        _camera = GetComponent<Camera>();
        _runner = GetComponentInParent<Runner>();
    }

    // Update is called once per frame
    void Update()
    {
        Projectile projectile = GameObject.FindObjectOfType<Projectile>();
        if (projectile && !_isRunning)
        {
            //Viewport space is defined in [0, 1] here
            Vector3 screenPos = _camera.WorldToViewportPoint(projectile.transform.position);
            if (IsObjectInView(screenPos))
            {
                Debug.Log("BEEP BEEP");
                //------------------------------------------------------------------------------------adjust when fixed
                //_runner.SetTargetToCatch(ref projectile);
                //_runner.StartTracking();
                _isRunning = true;
            }
        }
    }

    bool IsObjectInView(Vector3 pos)
    {
        return (pos.x > 0 && pos.x < 1
             && pos.y > 0 && pos.y < 1
             && pos.z > 0);
    }

    public void ResetRunnerCamera()
    {
        _isRunning = false;
    }
    
}
