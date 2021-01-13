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


    public GameObject DragIndicator;
    public bool UseDrag = false;
    //Viscous Drag Force;
    public float k;
    //Aerodynamic Drag Force;
    public float c;
    public float dragMargin;
    private Vector3 _dragLandingPos;

    int iterationCounter;
    int maxIterations;
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
        iterationCounter = 0;
        maxIterations = 50;

        DragIndicator.GetComponent<SpriteRenderer>().enabled = false;
        _gravityVector = new Vector3(0f, -9.81f, 0f);
        GetComponent<MeshRenderer>().material = ValidShotMaterial;
    }

    // Update is called once per frame
    void Update()
    {

        //Input DRAG
        if (Input.GetKeyDown(KeyCode.R))
        {
            UseDrag = !UseDrag;
            DragIndicator.GetComponent<SpriteRenderer>().enabled = false;
        }

        //Calculate dir 
        FiringData firingData = CalculateFiringData();
        if (!firingData.IsValid)
            return;

        _direction = firingData.DirToLaunch;
        _totalTime = firingData.TimeToTarget;

        //Rotate towards calculated direction
        Quaternion rot = Quaternion.LookRotation(firingData.DirToLaunch);
        this.transform.rotation = rot;

        //Show drag indicator
        if (UseDrag)
        {
            
            DragIndicator.GetComponent<SpriteRenderer>().enabled = true;
            _dragLandingPos = CalculatePositionWithDragForce(_totalTime, _direction);
            _dragLandingPos.y = 0.01f;
            DragIndicator.transform.position = _dragLandingPos;

            //CalculateRefinedDirWithDragForce();
        }
        
        //Update visualization
        Predicter.SetPhysicsSettings(LaunchPos, _direction, _speed, _totalTime, UseDrag, k, c);

        //Since time is calculated in here, we're going to grab the UI from the controller and set the setting here
        GetComponent<ProjectileLauncherController>().Display.SetTimeToLand(_totalTime);
        _Velocity = _direction * _speed;
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

    Vector3 CalculateRefinedDirWithDragForce()
    {
        //Calculate firing solution, and get initial landing position guess
        float distanceToDragLanding = (_dragLandingPos - LaunchPos.position).magnitude;
        float distanceToTarget = (Target.position - LaunchPos.position).magnitude;
        float distanceDiff = distanceToDragLanding - distanceToTarget;

        //If our initial guess isn't too far from the landing position with drag -> use initial direction
        if (Mathf.Abs(distanceDiff) < dragMargin + float.Epsilon)
        {
            return _direction;
        }


        //Binary search to ensure closer trajectory to target
        float minBound = 0;
        float maxBound = 0;
        Vector3 dirToT = Target.position - LaunchPos.position;
        float angle = Mathf.Asin(_direction.y / _direction.magnitude);
        if (distanceDiff > 0)
        {
            //Maximum bound -> use shortest possible shot as minimum bound
            maxBound = angle;
            minBound = -Mathf.PI / 2f;

            //Create new rotated direction
            Vector3 dirCopy = _direction;
            Vector3 newDir = Quaternion.AngleAxis(minBound * Mathf.Rad2Deg, Vector3.forward) * dirCopy;

            //Calculate new landing pos and check if it's close enough in distance to target
            Vector3 landingPos = CalculatePositionWithDragForce(_totalTime, newDir);
            landingPos.y = 0.01f;
            float distanceToLandPos = (landingPos - LaunchPos.position).magnitude;
            float diff = distanceToLandPos - distanceToTarget;

            //If so, use this as direction
            if (Mathf.Abs(diff) < dragMargin + float.Epsilon)
                return newDir;
        }
        else
        {
            //Need to check for maximum bound here
            minBound = angle;
            maxBound = Mathf.PI / 4f;

            //Create new rotated direction
            Vector3 dirCopy = _direction;
            Vector3 newDir = Quaternion.AngleAxis(maxBound * Mathf.Rad2Deg, Vector3.forward) * dirCopy;

            //Calculate new landing pos and check if it's close enough in distance to target
            Vector3 landingPos = CalculatePositionWithDragForce(_totalTime, newDir);
            landingPos.y = 0.01f;
            float distanceToLandPos = (landingPos - LaunchPos.position).magnitude;
            float diff = distanceToLandPos - distanceToTarget;

            //If so, use this as direction
            if (Mathf.Abs(diff) < dragMargin + float.Epsilon)
                return newDir;

            //If not, check if we even overshoot the target with best possible angle
            if (diff < 0)
            {
                //Need to increase the force in that case, and retry until we get it right
                _speed += 1f;
                iterationCounter += 1;
                if (iterationCounter < maxIterations)
                {
                    return CalculateRefinedDirWithDragForce();
                }
                else
                {
                    //If it seems we can't even get it right even by increasing the force, just reset it and return regular direction
                    iterationCounter = 0;
                    _speed -= maxIterations;
                    return _direction;
                }
            }
        }

        //We have a min and max bound -> so search for something inbetween that will fit the margin
        Vector3 closestDir = new Vector3();

        float rDist = float.MaxValue;
        while (Mathf.Abs(rDist) < dragMargin + float.Epsilon)
        {
            float a = (maxBound - minBound) / 2f;

            Vector3 dirCopy = _direction;
            closestDir = Quaternion.AngleAxis(a * Mathf.Rad2Deg, Vector3.forward) * dirCopy;

            //Calculate new landing pos and check if it's close enough in distance to target
            Vector3 landingPos = CalculatePositionWithDragForce(_totalTime, closestDir);
            landingPos.y = 0.01f;
            float dist = (landingPos - LaunchPos.position).magnitude;
            rDist = dist - distanceToTarget;

            if (rDist < 0)
                minBound = angle;
            else
                maxBound = angle;
        }
        return closestDir;

    }

    Vector3 CalculatePositionWithDragForce(float t, Vector3 dir)
    {
        //EulerNumber
        float e = 2.718281828459f;

        //Solving the equation: Pt = g - kPt (simplified version for taking drag into account)
        //We can solve the following: Pt = (gt - Ae^-kt) / k + B
        //With A and B being constants found from the position and velocity of the particle at t = 0
        Vector3 A = _speed * dir - (_gravityVector / k);
        Vector3 B = LaunchPos.position + (A / k);

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

        Rigidbody rb = p.GetComponent<Rigidbody>();
        rb.velocity = _Velocity;
     
        if (UseDrag)
            rb.drag = k;

        p.MaxLifeTime = _totalTime * 2f;
    }
}
