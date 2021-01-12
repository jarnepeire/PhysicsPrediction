using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncherController : MonoBehaviour
{
    //Public Variables
    [Header("Rotation Variables")]
    public bool DisableMovement = false;
    public bool UseMaxAngle = false;
    public float RotationSpeed = 50f;
    public float MaxAngleInDegrees = 90f;

    [Header("Launch Variables")]
    public bool IsTargetLauncher = false;
    public float Speed;

    [Header("UI Variables")]
    public LauncherDisplay Display;

    //Private Variables
    private ProjectileLauncher _launcher;
    private ProjectileLauncherAtTarget _targetLauncher;
    private float _speedIncrement = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        if (IsTargetLauncher)
        {
            _targetLauncher = GetComponent<ProjectileLauncherAtTarget>();
            _targetLauncher.SetSpeed(Speed);
        }
        else 
        {
            _launcher = GetComponent<ProjectileLauncher>();
            _launcher.SetDirection(Vector3.forward);
            _launcher.SetSpeed(Speed);
        }

        Display.SetSpeed(Speed);
        Display.SetDir(Vector3.forward);
    }

    // Update is called once per frame
    void Update()
    {
        if (!DisableMovement) 
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
        if (!IsTargetLauncher)
        {
            _launcher.SetDirection(direction);
        }
        Display.SetDir(direction);
    }

    private void UpdateSpeed()
    {

        if (Input.GetKeyDown(KeyCode.X))
        {
            Speed += _speedIncrement;
            if (IsTargetLauncher) _targetLauncher.SetSpeed(Speed);
            else _launcher.SetSpeed(Speed);
            Display.SetSpeed(Speed);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Speed -= _speedIncrement;
            if (IsTargetLauncher) _targetLauncher.SetSpeed(Speed);
            else _launcher.SetSpeed(Speed);
            Display.SetSpeed(Speed);
        }
    }

    private void UpdateLaunch()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsTargetLauncher) _targetLauncher.LaunchProjectile();
            else _launcher.LaunchProjectile();
        }
    }
}
