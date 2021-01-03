using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncherController : MonoBehaviour
{
    //Public Variables
    [Header("Rotation Variables")]
    public float RotationSpeed = 50f;
    bool UseMaxAngle = false;
    public float MaxAngleInDegrees = 90f;

    [Header("Launch Variables")]
    public float Speed;

    //Private Variables
    private ProjectileLauncher _launcher;
    private float _speedIncrement = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        _launcher = GetComponent<ProjectileLauncher>();
        _launcher.SetDirection(Vector3.forward);
        _launcher.SetSpeed(Speed);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRotation();
        UpdateSpeed();
        UpdateLaunch();
    }

    private void UpdateRotation()
    {
        //Store old rotation
        Quaternion oldRot = transform.rotation;

        //Rotate
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.rotation = Quaternion.AngleAxis(-RotationSpeed * Time.deltaTime, Vector3.up) * transform.rotation;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.rotation = Quaternion.AngleAxis(RotationSpeed * Time.deltaTime, Vector3.up) * transform.rotation;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.rotation = Quaternion.AngleAxis(-RotationSpeed * Time.deltaTime, Vector3.right) * transform.rotation;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.rotation = Quaternion.AngleAxis(RotationSpeed * Time.deltaTime, Vector3.right) * transform.rotation;
        }

        if (UseMaxAngle)
        {
            //Angle check -> in case our rotation causes our object to go over the defined max angle, we return to the old rotation
            float angle = Quaternion.Angle(Quaternion.LookRotation(Vector3.forward), transform.rotation);
            if (angle > MaxAngleInDegrees || transform.rotation.x > 0f) //x > 0 is so that the launcher can not look more down to the ground
            {
                transform.rotation = oldRot;
            }
        }

        //Set Direction
        Vector3 direction = transform.rotation * Vector3.forward;
        _launcher.SetDirection(direction);
    }

    private void UpdateSpeed()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Speed += _speedIncrement;
            _launcher.SetSpeed(Speed);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Speed -= _speedIncrement;
            _launcher.SetSpeed(Speed);
        }
    }

    private void UpdateLaunch()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _launcher.LaunchProjectile();
        }
    }
}
