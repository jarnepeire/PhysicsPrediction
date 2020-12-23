using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    [Header("Raycast Variables")]
    //Sprite to display where the mouse cursor is currently looking at in the world 
    public GameObject CursorIndicator;

    //Camera to cast rays from (usually main camera)
    public Camera CurrentCamera = Camera.main;

    //Filter in the objects you want to raycast on 
    public string TagToRaycast;

    //Necesarry for raycasting -> default should succeed
    public LayerMask LayerMask = default;

    //Maximum distance for valid raycasts
    public float MaxRaycastDistance = 100f;

    [Header("Projectile Variables")]
    //Prefab of the projectile we want to launch (make sure it has a rigid body!)
    public Rigidbody ProjectilePrefab;

    //Starting position of where to launch the projectile from
    public Transform LaunchPos;

    public float MaxHeight;

    

    //Private variables
    private float _offsetCursor;

    // Initialize private variables
    void Start()
    {
        _offsetCursor = 0.01f;
    }

    // Update is called once per frame
    void Update()
    {
        //Setup ray -> from screen space to a ray in world space
        Ray cameraRay = CurrentCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo = new RaycastHit();

        //Check if rays hit, on hit update information
        bool isHit = CastRay(ref cameraRay, ref hitInfo);
        if (isHit)
        {
            //Update cursor position
            UpdateMouseCursor(ref hitInfo);

            //Update launcher rotation
            UpdateLauncherRotation(ref hitInfo);
        }
        else
        {
            CursorIndicator.SetActive(false);
        }

        //Update inputs
        if (Input.GetKeyUp(KeyCode.Space))
        {
            //...launch
            //Idea: 
            //We want to spawn a projectile at launch position and set its initial velocity to "V" so that the projectile lands at hitpoint P


            //We're looking at hitpoint P
            // -> calcule the absolute X and Y distance from our launch point to the hit point
            float Px = hitInfo.point.x - LaunchPos.position.x;
            float Py = hitInfo.point.y - LaunchPos.position.y;
        }
    }

    void UpdateMouseCursor(ref RaycastHit hitInfo)
    {
        //Move cursor to hitpoint location
        CursorIndicator.SetActive(true);
        Vector3 hitPoint = hitInfo.point;
        CursorIndicator.transform.position = new Vector3(hitPoint.x, hitPoint.y + _offsetCursor, hitPoint.z);
    }

    void UpdateLauncherRotation(ref RaycastHit hitInfo)
    {
        //Rotate launch cannon to velocity towards hitposition
        Quaternion rotation = Quaternion.LookRotation(hitInfo.point);
    }
    
    bool CastRay(ref Ray r, ref RaycastHit hitInfo)
    {
        return (Physics.Raycast(r, out hitInfo, MaxRaycastDistance, LayerMask) 
            && hitInfo.collider.gameObject.tag == TagToRaycast
            && hitInfo.point.z > transform.position.z);
    }
}
