using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This class is responsible for translating raw input into launcher actions,
 * and updates the launcher information to the UI display.
 * 
 * E.g. 'SPACE' launches projectile, 'X' increases the speed/force at which the projectile launches with
 */

public class ProjectileLauncherController : MonoBehaviour
{
    [Header("Launcher Variables")]
    public BaseLauncher Launcher;
    public float Speed;
    public float SpeedIncrement = 0.5f;

    [Header("Rotation Variables")]

    //When controlling a launcher that calculates its own direction, it's best to leave this on false
    public bool DisableManualLauncherRotation = false;
    public bool UseMaxAngle = false;
    public float RotationSpeed = 50f;
    public float MaxAngleInDegrees = 90f;

    [Header("UI Variables")]
    public LauncherDisplay Display;

    // Start is called before the first frame update
    void Start()
    {
        if (Launcher)
        {
            Launcher.SetDirection(Vector3.forward);
            Launcher.SetSpeed(Speed);
        }

        Display.SetSpeed(Speed);
        Display.SetDir(Vector3.forward);
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
        if (!DisableManualLauncherRotation)
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
            Vector3 desiredDir = transform.rotation * Vector3.forward;
            Launcher.SetDirection(desiredDir);
        }
        else
        {
            //Rotate towards calculated direction
            Vector3 direction = Launcher.GetDirection();
            Quaternion rot = Quaternion.LookRotation(direction);
            this.transform.rotation = rot;
        }

        //Set UI direction
        Display.SetDir(Launcher.GetDirection());
    }

    private void UpdateSpeed()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Speed += SpeedIncrement;
            Launcher.SetSpeed(Speed);
            Display.SetSpeed(Speed);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Speed -= SpeedIncrement;
            Launcher.SetSpeed(Speed);
            Display.SetSpeed(Speed);
        }
    }

    private void UpdateLaunch()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Launcher.LaunchProjectile();
        }
    }
}
