using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    struct ProjectileData
    {
        public ProjectileData(Vector3 vel, float time)
        {
            this.Velocity = vel;
            this.TimeToLand = time;
        }

        public Vector3 Velocity;
        public float TimeToLand;
    }

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

    //Maximum height the arc of the trajectory of the projectile should reach
    public float MaxHeight = 25.0f;

    //Speeding/slowing down the projectile speed, based on scaling gravity
    public float GravitalSpeed = 1.0f;
    

    //Private variables
    private float _offsetCursor;
    private float _gravity;
    private Vector3 _originalGravity;
    RaycastHit _hitInfo;


    // Initialize private variables
    void Start()
    {
        
        _originalGravity = Physics.gravity;
        Physics.gravity = _originalGravity * GravitalSpeed;
        _gravity = Physics.gravity.y;
        _offsetCursor = 0.01f;
        _hitInfo = new RaycastHit();
    }

    // Update is called once per frame
    void Update()
    {
        //Setup ray -> from screen space to a ray in world space
        Ray cameraRay = CurrentCamera.ScreenPointToRay(Input.mousePosition);

        //Check if rays hit, on hit update information
        bool isHit = CastRay(ref cameraRay);
        if (isHit)
        {
            //Update cursor position
            UpdateMouseCursor();

            //Update launcher rotation
            UpdateLauncher();
        }
        else
        {
            CursorIndicator.SetActive(false);
        }


    }

    void UpdateMouseCursor()
    {
        //Move cursor to hitpoint location
        CursorIndicator.SetActive(true);
        Vector3 hitPoint = _hitInfo.point;
        CursorIndicator.transform.position = new Vector3(hitPoint.x, hitPoint.y + _offsetCursor, hitPoint.z);
    }

    void UpdateLauncher()
    {
        //Rotate launch cannon to velocity towards hitposition
        Vector3 velocity = CalculateProjectileData(LaunchPos.position, _hitInfo.point).Velocity; 
        Quaternion rotation = Quaternion.LookRotation(velocity);
        transform.rotation = rotation;

        //Launch
        if (Input.GetKeyUp(KeyCode.Space))
        {
            //We want to spawn a projectile at launch position and set its initial velocity to "Vi" so that the projectile lands at hitpoint "P"
            //Vi (initial velocity) will be a combination of an Upwards velocity, Downwards Velocity, and Forward Velocity
            Rigidbody obj = Instantiate(ProjectilePrefab, LaunchPos.position, Quaternion.identity);
            obj.velocity = velocity;
            
        }
    }
    
    bool CastRay(ref Ray r)
    {
        return (Physics.Raycast(r, out _hitInfo, MaxRaycastDistance, LayerMask) 
            && _hitInfo.collider.gameObject.tag == TagToRaycast
            && _hitInfo.point.z > transform.position.z);
    }

    ProjectileData CalculateProjectileData(Vector3 startPos, Vector3 targetPos)
    {
        //----------------------- Upwards velocity variables -----------------------
        //We know: the displacement -> MaxHeight
        //We know: the acceleration -> gravity
        //We know: the final velocity -> 0 (standing still at the end)

        //Formula initial velocity: Vf^2 = Vi^2 + 2ad
        //--> Vi = sqrt(Vf^2 -2ad)
        float yVelocity = Mathf.Sqrt(-2 * _gravity * MaxHeight);

        //Formula time: d = Vf * t - (at^2 / 2)
        //--> t = sqrt(-2d/a) with Vf being 0
        //We'll need this for our right velocity time
        float tUp = Mathf.Sqrt(-2 * MaxHeight / _gravity);

        //----------------------- Downwards velocity variables -----------------------
        //We know: displacement = (TargetY - StartY) - MaxHeight
        //We know: acceleration = gravity
        //We know: init velocity = 0

        //Formula time: d = Vi * t + (at^2 / 2)
        //--> t = sqrt(2(Py - MaxHeight) / a) with Vi being 0
        float Py = targetPos.y - startPos.y;
        float tDown = Mathf.Sqrt(2 * (Py - MaxHeight) / _gravity);

        //----------------------- Right velocity variables -----------------------
        //We know: displacement = TargetX - StartX
        //We know: acceleration = 0
        //We know: time = tUp + tDown

        //Formula initial velocity: d = Vi * t + (at^2 / 2)
        //--> Vi = Px / (tUp + tDown) with a being 0
        float Px = targetPos.x - startPos.x;
        float Pz = targetPos.z - startPos.z;
        float time = (tUp + tDown);
       

        float xVelocity = Px / time;
        float zVelocity = Pz / time;
        
        Debug.Log("TIME TO LAND: " + time);

        //Now we have all the variables we need to form our final 3 velocities in each axis and combine them to form the initial velocity of our projectile
        return new ProjectileData(new Vector3(xVelocity, yVelocity, zVelocity), time);
    }
}
