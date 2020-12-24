using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runner : MonoBehaviour
{

    public bool StartRunning;
    public bool StopRunning;
    public Vector3 TargetToCatch;



    public float InitialVelocity;
    public float Acceleration;
    private float _currentVelocity;


    // Start is called before the first frame update
    void Start()
    {
        StartRunning = false;
        StopRunning = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (StartRunning && !StopRunning)
        {
            _currentVelocity += Acceleration * Time.fixedDeltaTime;
            transform.Translate(Vector3.one * _currentVelocity * Time.fixedDeltaTime);
        }
    }
}
