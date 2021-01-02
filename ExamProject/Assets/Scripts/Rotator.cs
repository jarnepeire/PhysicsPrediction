using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    //Public Variables
    [Header("Rotation Variables")]
    public float RotationSpeed = 50f;
    bool UseMaxAngle = false;
    public float MaxAngleInDegrees = 90f;


    public Vector3 Direction;
    private ProjectileLauncher _launcher;
    // Start is called before the first frame update
    void Start()
    {
        _launcher = GetComponent<ProjectileLauncher>();
    }

    // Update is called once per frame
    void Update()
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
        Direction = transform.rotation * Vector3.forward;
        _launcher.Direction = Direction;
    }
}