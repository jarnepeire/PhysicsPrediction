using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncherAtTarget : MonoBehaviour
{
    //Based on the Firing Solution in AI For Games
    //Instead of us giving direction and force to calculate a final position
    //We're going to reverse engineer the direction to land at a final position at a certain force

    // Start is called before the first frame update
    public Projectile ProjectilePrefab;
    public Transform LaunchPos;
    public Transform Target;
    private float _speed = 5f;
    private Vector3 _gravityVector;
    private Vector3 _direction;
    private float _totalTime;
    private Vector3 _Velocity;
    public PhysicsPredicter Predicter;

    public Material ValidShotMaterial;
    public Material InvalidShotMaterial;


    //Viscous Drag Force;
    public float k;
    //Aerodynamic Drag Force;
    public float c;

    struct FiringData
    {
        public FiringData(bool isValid, float time, Vector3 dir)
        {
            IsValid = isValid;
            TimeToTarget = time;
            DirToLaunch = dir;
        }

        public bool IsValid;
        public float TimeToTarget;
        public Vector3 DirToLaunch;
    }

    void Start()
    {
        _gravityVector = new Vector3(0f, -9.81f, 0f);
        GetComponent<MeshRenderer>().material = ValidShotMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        //Calculate dir 
        FiringData firingData = CalculateFiringData();
        if (!firingData.IsValid)
            return;

        _direction = firingData.DirToLaunch;
        _totalTime = firingData.TimeToTarget;

        //Rotate towards calculated direction
        Quaternion rot = Quaternion.LookRotation(firingData.DirToLaunch);
        this.transform.rotation = rot;

        //Update visualization
        Predicter.SetPhysicsSettings(LaunchPos, _direction, _speed, _totalTime, k, c);

        //Since time is calculated in here, we're going to grab the UI from the controller and set the setting here
        GetComponent<ProjectileLauncherController>().Display.SetTimeToLand(_totalTime);

        //Drag force
        
        //Vector3 dragForce = -k * _Velocity - c * new Vector3(_Velocity.x * _Velocity.x, _Velocity.y * _Velocity.y, _Velocity.z * _Velocity.z);

        float air_density = 1.225f;
        float drag_coeff = 1f;
        float radius = ProjectilePrefab.GetComponent<SphereCollider>().radius;
        float area = Mathf.PI * (radius * radius);

        Vector3 dragForce = (float)(0.5 * air_density * drag_coeff * area * (_speed * _speed)) * -_direction;
        Vector3 force = (_direction * _speed) + dragForce;

        _Velocity = _Velocity + force / 1f * Time.deltaTime;
    }

    FiringData CalculateFiringData()
    {
        //Given a target pos, start pos and velocity speed -> we need to solve the direction in which to fire
        //To solve this, we can start by solving for time given our knowns
        Vector3 T = Target.position - LaunchPos.position;
        float dotGT = Vector3.Dot(T, _gravityVector);
        float sqrdSpeed = _speed * _speed;

        //Quadratic equation to solve for two times
        float a = _gravityVector.sqrMagnitude;
        float b = -4 * (dotGT + sqrdSpeed);
        float c = 4 * T.sqrMagnitude;

        //Check if there will be an actual solution given our speed/force factor
        float D = (b * b) - 4 * a * c;
        if (D < 0)
        {
            GetComponent<MeshRenderer>().material = InvalidShotMaterial;
            return new FiringData(false, 0f, Vector3.zero);
        }
        else
        {
            GetComponent<MeshRenderer>().material = ValidShotMaterial;
        }
            
        //Solve for two times
        float time0 = Mathf.Sqrt((-b + Mathf.Sqrt(D)) / (2 * a));
        float time1 = Mathf.Sqrt((-b - Mathf.Sqrt(D)) / (2 * a));

        //Find actual time to target
        float time = 0f;
        if (time0 < 0)
            if (time1 < 0)
                return new FiringData(false, 0f, Vector3.zero);
            else
                time = time1;
        else
            if (time1 < 0)
                time = time0;
            else
                time = Mathf.Min(time0, time1);

        //Return firing direction
        Vector3 dir = (T * 2 - _gravityVector * (time * time)) / (2 * _speed * time);
        FiringData firingData = new FiringData(true, time, dir);
        return firingData;
    }

    Vector3 CalculatePositionWithDragForce(float t)
    {
        //EulerNumber
        float e = 2.718281828459f;

        //Solving the equation: Pt = g - kPt (simplified version for taking drag into account)
        //We can solve the following: Pt = (gt - Ae^-kt) / k + B
        //With A and B being constants found from the position and velocity of the particle at t = 0
        Vector3 A = _speed * _direction - (_gravityVector / k);
        Vector3 B = LaunchPos.position - (A / k);

        //Position in time with drag force applied
        Vector3 Pt = ((_gravityVector * t - A * Mathf.Pow(e, -k * t)) / k) + B;

        return Pt;
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    public void LaunchProjectile()
    {
        //Launch projectile
        Projectile p = Instantiate(ProjectilePrefab);
        p.transform.position = LaunchPos.position;
        p.GetComponent<Rigidbody>().velocity = _Velocity;
        p.MaxLifeTime = _totalTime * 2f;
    }
}
