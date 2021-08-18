using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runner : MonoBehaviour
{
    struct ProjectilePrediction
    {
        public ProjectilePrediction(float speed, float time, Vector3 pos)
        {
            PredictedSpeed = speed;
            TimeToCatch = time;
            PredictedLandingPos = pos;
        }

        public float PredictedSpeed;
        public float TimeToCatch;
        public Vector3 PredictedLandingPos;
    }

    [Header("Materials")]
    public Material InactiveMaterial;
    public Material ActiveMaterial;

    [Header("Catching")]
    public Transform CatchLocation;
    public bool UseLagrangePrediction = false;

    //Private Variables
    private Vector3 _startPos;
    private Quaternion _startRot;
    private Projectile _targetToCatch;

    private int _posCount;
    private Vector3[] _positions3D = new Vector3[3];
    private Vector2[] _positions2D = new Vector2[3];
    private float[] _timers3D = new float[3];

    private bool _canDoTracking;
    private bool _canRun;
    private float _runningTimeElapsed;

    private Vector3 _gravityVector = new Vector3(0f, -9.81f, 0f);
    private Vector3 _runnerVel;
    private ProjectilePrediction _projectilePrediction;

    void Start()
    {
        _projectilePrediction = new ProjectilePrediction(0f, 0f, new Vector3(0f, 0f, 0f));

        GetComponent<MeshRenderer>().material = InactiveMaterial;
        _startPos = transform.position;
        _startRot = transform.rotation;

        _canDoTracking = false;
        _posCount = 0;
        _canRun = false;
        _runningTimeElapsed = 0f;
    }

    private void FixedUpdate()
    {
        if (_canDoTracking)
        {
            if (_posCount < 3)
            {
                _timers3D[_posCount] = Time.fixedTime;
                _positions3D[_posCount] = _targetToCatch.transform.position;
                _posCount++;
            }
            else
            {
                _projectilePrediction = PredictProjectile();
                StartRunning();
                StopTracking();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_canRun) 
            RunToTarget();
    }

    private void RunToTarget()
    {
        this.GetComponent<Rigidbody>().velocity = _runnerVel;
        _runningTimeElapsed += Time.deltaTime;
        if (_runningTimeElapsed > _projectilePrediction.TimeToCatch)
        {
            StopRunning();
        }
    }

    private ProjectilePrediction PredictProjectile()
    {
        if (UseLagrangePrediction)
            return PredictProjectileLagrange();
        else
            return PredictProjectilePhysics3D();
    }

    private ProjectilePrediction PredictProjectileLagrange()
    {
        //Make direction vector going from the first observed position to the second
        Vector3 dir = _positions3D[1] - _positions3D[0];

        //We need to convert our 3D problem to a 2D problem to apply a Lagrange Interpolation on
        _positions2D[0] = new Vector3(Mathf.Sqrt((_positions3D[0].x * _positions3D[0].x) + (_positions3D[0].z * _positions3D[0].z)), _positions3D[0].y);
        _positions2D[1] = new Vector3(Mathf.Sqrt((_positions3D[1].x * _positions3D[1].x) + (_positions3D[1].z * _positions3D[1].z)), _positions3D[1].y);
        _positions2D[2] = new Vector3(Mathf.Sqrt((_positions3D[2].x * _positions3D[2].x) + (_positions3D[2].z * _positions3D[2].z)), _positions3D[2].y);

        //Apply lagrange to our 2D problem
        float[] ips = Lagrange(_positions2D);

        //From these intersection points we can define:
        //Our highest point to be the landing X position in our 2D-plane
        float futX = Mathf.Max(ips[0], ips[1]);

        //We'll create this fake 2D speed to find the remaining time until landing
        //We can take a projected distance from the first two positions and divide it by the time it needed from 1 point to the other
        float p1_p2_distance2D = Mathf.Sqrt((dir.x * dir.x) + (dir.z * dir.z));
        float p1_p2_time = _timers3D[1] - _timers3D[0];
        float speed2D = p1_p2_distance2D / p1_p2_time;

        //We calculate the 2D distance, from the first observed position to the landing X in our 2D-plane
        //If we devide this distance by the speed for that plane, we get the real remaining time until landing
        float distance2D = Mathf.Abs(futX - _positions2D[0].x);
        float timeToLand = distance2D / speed2D;

        //To actually predict the landing location in 3D, we will have to use estimations
        //Est. of the dir -> dir between first two observed points
        //Est. of the speed -> we can't use our "2d" speed, so we divide our 3D distance by the time
        float p1_p2_distance3D = dir.magnitude;
        float speed3D = p1_p2_distance3D / p1_p2_time;

        //Now we can predict our final location, because our time factor here is the remaining time, this counters out the first observed direction we have
        //Which results in a pretty accurate predicted location
        Vector3 projectileDir = dir.normalized;
        Vector3 predictedLocation = _positions3D[0] + projectileDir * (speed3D * timeToLand) + _gravityVector * (timeToLand * timeToLand) / 2f;

        //Setup prediction information
        ProjectilePrediction prediction;
        prediction.PredictedLandingPos = predictedLocation;
        prediction.PredictedLandingPos.y = 0f;
        prediction.TimeToCatch = timeToLand;
        prediction.PredictedSpeed = speed3D;

        return prediction;
    }

    private ProjectilePrediction PredictProjectilePhysics3D()
    {
        //Make direction vector going from the first observed position to the second
        Vector3 dir = _positions3D[1] - _positions3D[0];

        //To actually predict the landing location in 3D, we will have to use estimations
        //Est. of the dir -> dir between first two observed points
        //Est. of the speed -> we divide our 3D distance by the time
        float p1_p2_distance = dir.magnitude;
        float p1_p2_time = _timers3D[1] - _timers3D[0];
        float speed = p1_p2_distance / p1_p2_time;

        //We're going to predict the time of when the projectile will be at catch height
        Vector3 projectileDir = dir.normalized;
        float yToCatch = CatchLocation.position.y;

        float plus = (-projectileDir.y * speed + Mathf.Sqrt((projectileDir.y * projectileDir.y) * (speed * speed) - 2 * _gravityVector.y * (yToCatch - 0f))) / _gravityVector.y;
        float min = (-projectileDir.y * speed - Mathf.Sqrt((projectileDir.y * projectileDir.y) * (speed * speed) - 2 * _gravityVector.y * (yToCatch - 0f))) / _gravityVector.y;
        float timeToCatch = Mathf.Max(min, plus);

        //Calculate the future position (where the projectile will land on catch height)
        Vector3 futureCatchPos = new Vector3
        (
            _positions3D[0].x + projectileDir.x * speed * timeToCatch,
            0.01f,
            _positions3D[0].z + projectileDir.z * speed * timeToCatch
        );

        //Setup prediction information
        ProjectilePrediction prediction;
        prediction.PredictedLandingPos = futureCatchPos;
        prediction.PredictedLandingPos.y = 0f;
        prediction.TimeToCatch = timeToCatch;
        prediction.PredictedSpeed = speed;

        return prediction;
    }

    private float[] Lagrange(Vector2[] positions2D)
    {
        //Using Lagrange's Polynomial Interpolation
        //We're going to define a quadratic polynomial using 3 points captured in space
        float x0 = positions2D[0].x;
        float y0 = positions2D[0].y;

        float x1 = positions2D[1].x;
        float y1 = positions2D[1].y;

        float x2 = positions2D[2].x;
        float y2 = positions2D[2].y;

        //Find a, b and c in ax^2 + bx + c
        float nominatorA = y0 * (x1 - x2) - y1 * (x0 - x2) + y2 * (x0 - x1);
        float denominator = (x0 - x1) * (x0 - x2) * (x1 - x2);
        float a = nominatorA / denominator;

        float nominatorB = y0 * (x1 + x2) * (x1 - x2) - y1 * (x0 + x2) * (x0 - x2) + y2 * (x0 + x1) * (x0 - x1);
        float b = -(nominatorB / denominator);

        float nominatorC = y0 * x1 * x2 * (x1 - x2) - y1 * x0 * x2 * (x0 - x2) + y2 * x0 * x1 * (x0 - x1);
        float c = nominatorC / denominator;

        //Calculate two intersection points of this quadratic equation 
        //Keep in mind, these intersection points are defined in our 2D-plane
        float D = (b * b) - 4 * a * c;
        float ip1 = (-b - Mathf.Sqrt(D)) / (2 * a);
        float ip2 = (-b + Mathf.Sqrt(D)) / (2 * a);

        //Return intersection points
        float[] intersectionPoints = new float[2];
        intersectionPoints[0] = ip1;
        intersectionPoints[1] = ip2;

        return intersectionPoints;
    }

    public void StartTracking()
    {
        _canDoTracking = true;
    }

    public void StopTracking()
    {
        _canDoTracking = false;
    }

    public void StartRunning()
    {
        _canRun = true;
        GetComponent<MeshRenderer>().material = ActiveMaterial;

        //Setup movement for runner
        Vector3 toTarget = _projectilePrediction.PredictedLandingPos - transform.position;
        Vector3 dir = toTarget.normalized;
        
        //Rotate to target
        Vector3 lookAtRotation = dir;
        lookAtRotation.y = 0f;
        transform.rotation = Quaternion.LookRotation(lookAtRotation);

        //Set velocity
        float speed = toTarget.magnitude / _projectilePrediction.TimeToCatch;
        _runnerVel = dir * speed;
        _runnerVel.y = 0f;
        this.GetComponent<Rigidbody>().velocity = _runnerVel;
    }

    public void StopRunning()
    {
        _canRun = false;
        GetComponent<MeshRenderer>().material = InactiveMaterial;
        this.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    public void SetTargetToCatch(ref Projectile projectile)
    {
        _targetToCatch = projectile;
    }

    public void ResetRunner()
    {
        StopRunning();
        _targetToCatch = null;
        _projectilePrediction = new ProjectilePrediction(0f, 0f, new Vector3(0f, 0f, 0f));
        transform.SetPositionAndRotation(_startPos, _startRot);
        _runningTimeElapsed = 0f;
        _canDoTracking = false;
        _posCount = 0;
    }

}