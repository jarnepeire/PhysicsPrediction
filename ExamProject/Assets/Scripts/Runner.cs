using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runner : MonoBehaviour
{
    private bool CanStartRunning;
    private Projectile TargetToCatch;
    float[] timers = new float[3];
    Vector3[] positions = new Vector3[3];
    Vector2[] positions_2D = new Vector2[3];
    public int posCount;

    public Vector3 LaunchPos;
    // Start is called before the first frame update
    void Start()
    {
        CanStartRunning = false;
        posCount = 0;
    }

    private void FixedUpdate()
    {
        if (CanStartRunning)
        {

            if (posCount < 3)
            {

                timers[posCount] = Time.fixedTime;
                positions[posCount] = TargetToCatch.transform.position;
                posCount++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (CanStartRunning)
        {
            
            if (posCount < 3)
            {
                
               /// timers[posCount] = Time.deltaTime;
               // positions[posCount] = TargetToCatch.transform.position;
                //posCount++;
            }
            else
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

                //we need to convert our problem to a 2d problem to apply lagrance on
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
                float D = (b*b) - 4 * a * c;
                float fx1 = (-b - Mathf.Sqrt(D)) / (2 * a);
                float fx2 = (-b + Mathf.Sqrt(D)) / (2 * a);

                //Vector3 dirToTarget = (Vector3.right + cte * Vector3.forward).normalized;
                Vector3 dirToTarget = (Vector3.right + cte * Vector3.forward).normalized;
                float distance = fx2 - fx1;



                //float p1_p2_distanceTransformed = Mathf.Sqrt((dir.x * dir.x) + (dir.z * dir.z));
                //float p1_p2_distanceTransformed = dir.magnitude;
                //float p1_p2_time = timers[1] - timers[0];
                //float p1_p2_time = 0.1f;

                float p1_p2_distanceTransformed = Mathf.Sqrt((dir.x * dir.x) + (dir.z * dir.z));
                float p1_p2_time = timers[1] - timers[0];
                float speed = p1_p2_distanceTransformed / p1_p2_time;


                float REALtotalTime = distance / speed;
                Vector3 predictedLocation = LaunchPos + dirToTarget * speed * REALtotalTime + new Vector3(0f, -9.81f, 0f) * (REALtotalTime * REALtotalTime) / 2f;
                //
                Debug.Log("AI CALCULATED TIME " + REALtotalTime);
                CanStartRunning = false;
            }

        }
    }

    public void StartRunning()
    {
        CanStartRunning = true;
    }

    public void SetTargetToCatch(ref Projectile projectile)
    {
        TargetToCatch = projectile;
    }


}
