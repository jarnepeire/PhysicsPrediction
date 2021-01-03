using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runner : MonoBehaviour
{
    //Private Variables
    private Vector3 _startPos;
    private Quaternion _startRot;
    private Projectile TargetToCatch;

    public int posCount;
    private Vector3[] positions = new Vector3[3];
    private Vector2[] positions_2D = new Vector2[3];
    private float[] timers = new float[3];

    private bool _canDoTracking;
    private bool _canRun;
    private float _runningTimeElapsed;

    float predictedTotalTime;
    Vector3 predictedFuturePos;
    Vector3 runnerVel;

    // Start is called before the first frame update
    void Start()
    {
        _startPos = transform.position;
        _startRot = transform.rotation;

        _canDoTracking = false;
        posCount = 0;
        _canRun = false;
        _runningTimeElapsed = 0f;
    }

    private void FixedUpdate()
    {
        if (_canDoTracking)
        {
            if (posCount < 3)
            {
                timers[posCount] = Time.fixedTime;
                positions[posCount] = TargetToCatch.transform.position;
                posCount++;
            }
            else
            {
                PredictProjectile();
                StartRunning();
                StopTracking();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_canRun) RunToTarget();
    }

    private void RunToTarget()
    {
        this.GetComponent<Rigidbody>().velocity = runnerVel;
        _runningTimeElapsed += Time.deltaTime;
        if (_runningTimeElapsed > predictedTotalTime)
        {
            StopRunning();
        }
    }

    private void PredictProjectile()
    {

        //Vector3 Direction = new Vector3(1f, 2.5f, 1.5f);
        //float Speed = 3;
        //float time = 0.1f;
        //Vector3 _GravityVector = new Vector3(0f, -9.81f, 0f);
        //
        //positions[0] = LaunchPos + Direction * Speed * 0.1f + _GravityVector * (0.1f * 0.1f) / 2f;
        //positions[1] = LaunchPos + Direction * Speed * 0.2f + _GravityVector * (0.2f * 0.2f) / 2f;
        //positions[2] = LaunchPos + Direction * Speed * 0.3f + _GravityVector * (0.3f * 0.3f) / 2f;

        //make 1 direction
        Vector3 dir = positions[1] - positions[0];
        float cte = dir.z / dir.x;
        //dir = dir.normalized;
        //we need to convert our problem to a 2d problem to apply lagrange on
        //y stays the same
        //positions_2D[0] = new Vector3(positions[0].x + cte * positions[0].z, positions[0].y);
        //positions_2D[1] = new Vector3(positions[1].x + cte * positions[1].z, positions[1].y);
        //positions_2D[2] = new Vector3(positions[2].x + cte * positions[2].z, positions[2].y);

        positions_2D[0] = new Vector3(Mathf.Sqrt((positions[0].x * positions[0].x) + (positions[0].z * positions[0].z)), positions[0].y);
        positions_2D[1] = new Vector3(Mathf.Sqrt((positions[1].x * positions[1].x) + (positions[1].z * positions[1].z)), positions[1].y);
        positions_2D[2] = new Vector3(Mathf.Sqrt((positions[2].x * positions[2].x) + (positions[2].z * positions[2].z)), positions[2].y);

        //lagrange
        float x0 = positions_2D[0].x;
        float y0 = positions_2D[0].y;

        float x1 = positions_2D[1].x;
        float y1 = positions_2D[1].y;

        float x2 = positions_2D[2].x;
        float y2 = positions_2D[2].y;

        //
        float a, b, c;

        float tellerA = y0 * (x1 - x2) - y1 * (x0 - x2) + y2 * (x0 - x1);
        float noemer = (x0 - x1) * (x0 - x2) * (x1 - x2);

        a = tellerA / noemer;

        float tellerB = y0 * (x1 + x2) * (x1 - x2) - y1 * (x0 + x2) * (x0 - x2) + y2 * (x0 + x1) * (x0 - x1);
        b = -(tellerB / noemer);


        float tellerC = y0 * x1 * x2 * (x1 - x2) - y1 * x0 * x2 * (x0 - x2) + y2 * x0 * x1 * (x0 - x1);
        c = tellerC / noemer;

        //
        float D = (b * b) - 4 * a * c;
        float fx1 = (-b - Mathf.Sqrt(D)) / (2 * a);
        float fx2 = (-b + Mathf.Sqrt(D)) / (2 * a);
        float futX = Mathf.Max(fx1, fx2);
        float minX = Mathf.Min(fx1, fx2);

        //voor begin dir te vinde
        //vind afgeleide
        float afgeleide = 2 * a * minX + b;
        Vector3 DIRRRR = dir;
        Vector3 plsWerk = new Vector3(
            DIRRRR.x / Mathf.Sqrt((DIRRRR.x * DIRRRR.x) + (DIRRRR.z * DIRRRR.z)),
            afgeleide,
            DIRRRR.z / Mathf.Sqrt((DIRRRR.x * DIRRRR.x) + (DIRRRR.z * DIRRRR.z))
            );
        plsWerk = plsWerk.normalized;

        Vector3 plsWerk2 = new Vector3
            (
            DIRRRR.x,
            afgeleide * ((DIRRRR.x * DIRRRR.x) + (DIRRRR.z * DIRRRR.z)),
            DIRRRR.z
            );
        plsWerk2 = plsWerk.normalized;
        //vind 3d dir adhv afgeleide


        //Vector3 dirToTarget = (Vector3.right + cte * Vector3.forward).normalized;

        Vector3 dirToTarget = (Vector3.right + cte * Vector3.forward).normalized;
        if (dir.x < 0f) dirToTarget = -dirToTarget;


        //float distance = Mathf.Abs(futX - LaunchPos.x);
        float distance = Mathf.Abs(futX - positions_2D[0].x);
        float distance_NR2 = Mathf.Abs(futX - minX);

        //test
        Vector3 test_Dir1 = positions[1] - positions[0];
        Vector3 test_Dir2 = positions[2] - positions[1];
        float test_time1 = timers[1] - timers[0];
        float test_time2 = timers[2] - timers[1];
        float alpha = test_time1 / test_time2;
        Vector3 test_finalDir = Vector3.Lerp(test_Dir1, test_Dir2, alpha).normalized;
        Vector3 tttt = new Vector3(0.3f, 0f, 0.7f);
        tttt = tttt.normalized;

        Vector3 testtttt_Dir1 = (positions[1] - positions[0]).normalized;
        //test

        //float p1_p2_distanceTransformed = Mathf.Sqrt((dir.x * dir.x) + (dir.z * dir.z));
        //float p1_p2_distanceTransformed = dir.magnitude;
        //float p1_p2_time = timers[1] - timers[0];
        //float p1_p2_time = 0.1f;

        float p1_p2_distanceTransformed = Mathf.Sqrt((dir.x * dir.x) + (dir.z * dir.z));
        float p1_p2_distanceTransformedR = dir.magnitude;
        float p1_p2_time = timers[1] - timers[0];
        float speed = p1_p2_distanceTransformed / p1_p2_time;
        float realSpeed = p1_p2_distanceTransformedR / p1_p2_time;

        //Variables
        float REALtotalTime = distance / speed;
        Vector3 predictedLocation = positions[0] + dir.normalized * realSpeed * REALtotalTime + new Vector3(0f, -9.81f, 0f) * (REALtotalTime * REALtotalTime) / 2f;
        //Vector3 predictedLocation2 = LaunchPos + plsWerk * realSpeed * REALtotalTime + new Vector3(0f, -9.81f, 0f) * (REALtotalTime * REALtotalTime) / 2f;


        predictedTotalTime = REALtotalTime;
        predictedFuturePos = predictedLocation;
        predictedFuturePos.y = 0f;

        Debug.Log("AI CALCULATED TIME " + REALtotalTime);

        //

        Vector3 toTarget = predictedFuturePos - transform.position;
        Vector3 dirrr = toTarget.normalized;
        //Vector3 lookAtRotation = dirrr;
        //lookAtRotation.y = 0f;
        // transform.rotation = Quaternion.LookRotation(lookAtRotation);
        float speeddddd = toTarget.magnitude / predictedTotalTime;
        runnerVel = dirrr * speeddddd;
        runnerVel.y = 0f;
        this.GetComponent<Rigidbody>().velocity = runnerVel;
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
    }

    public void StopRunning()
    {
        _canRun = false;
        this.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    public void SetTargetToCatch(ref Projectile projectile)
    {
        TargetToCatch = projectile;
    }

    public void ResetRunner()
    {
        transform.position = _startPos;
        transform.rotation = _startRot;
        _canRun = false;
    }

}
