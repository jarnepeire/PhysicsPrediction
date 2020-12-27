using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewLauncher : MonoBehaviour
{
    //Based on AI For Games (3.5 Physics Prediction)
    //Main Formula for Projectile Trajectory (in vacuum)
    // Pt = P0 + U * Sm * t + (gt^2 / 2)
    //  @Pt = position at time t
    //  @P0 = start position of trajectory
    //  @U  = direction the trajectory started in
    //  @Sm = 'velocity', it's more like a speed of the projectile
    //  @t  = time since the trajectory started
    //  @g  = gravity (0, -9.81, 0)

    public Vector3 Direction;
    public Transform LaunchPos;
    public float Speed;
    public Projectile ProjectilePrefab;
    public Transform HitIndicator;
    public LineRenderer LineRender;
    public Runner _runner;

    private float _Gravity;
    private Vector3 _GravityVector;
    public int lineSegments;

    // Start is called before the first frame update
    void Start()
    {
        _Gravity = -9.81f;
        _GravityVector = new Vector3(0f, -9.81f, 0f);

        transform.rotation = Quaternion.LookRotation(Direction);
        LineRender.positionCount = lineSegments + 1;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Figure out when grenade will land
            //Solving for time Ti 
            

            float plus = ( -Direction.y * Speed + Mathf.Sqrt((Direction.y * Direction.y) * (Speed * Speed) - 2 * _Gravity * (LaunchPos.position.y - 0) ) ) / _Gravity;
            float min = ( -Direction.y * Speed - Mathf.Sqrt((Direction.y * Direction.y) * (Speed * Speed) - 2 * _Gravity * (LaunchPos.position.y - 0) ) ) / _Gravity;
            float Ti = Mathf.Max(min, plus);
          
           
            Debug.Log("LAUNCHER TIME " + Ti);


            //launch projectile
            Projectile p = Instantiate(ProjectilePrefab);
            p.transform.position = LaunchPos.position;
            p.GetComponent<Rigidbody>().velocity = Direction * Speed;
            p.MaxLifeTime = Ti;

    
            //future pos
            Vector3 futurePos = new Vector3
            (
                LaunchPos.position.x + Direction.x * Speed * Ti,
                0.1f,
                LaunchPos.position.z + Direction.z * Speed * Ti
            );
            HitIndicator.position = futurePos;

            //draw line
            float timeSegment = Ti / (float)lineSegments;
            LineRender.SetPosition(0, LaunchPos.position);
            for (int i = 0; i < lineSegments; i++)
            {
                if (i == 0)
                    continue;

                float time = i * timeSegment;
                Vector3 postAtTime = LaunchPos.position + Direction * Speed * time + _GravityVector * (time * time) / 2f;
                LineRender.SetPosition(i, postAtTime);
            }
            LineRender.SetPosition(lineSegments, futurePos);


            _runner.LaunchPos = LaunchPos.position;
            _runner.SetTargetToCatch(ref p);
            _runner.StartRunning();
        }
    }
}
