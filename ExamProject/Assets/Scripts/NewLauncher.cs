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

    private float _Gravity;
    // Start is called before the first frame update
    void Start()
    {
        _Gravity = -9.81f;
        transform.rotation = Quaternion.LookRotation(Direction);
        LineRender.positionCount = 10;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Figure out when grenade will land
            //Solving for time Ti 
            float Ti = ( -Direction.y * Speed - Mathf.Sqrt((Direction.y * Direction.y) * (Speed * Speed) - 2 * _Gravity * (LaunchPos.position.y - 0) ) ) / _Gravity;
            if (Ti < 0f)
                Ti = ( -Direction.y * Speed - Mathf.Sqrt((Direction.y * Direction.y) * (Speed * Speed) - 2 * _Gravity * (LaunchPos.position.y - 0) ) ) / _Gravity;
           
            Debug.Log("TIME " + Ti);



            Projectile p = Instantiate(ProjectilePrefab);
            p.transform.position = LaunchPos.position;
            p.GetComponent<Rigidbody>().velocity = Direction * Speed;
            p.MaxLifeTime = Ti;



            Vector3 futurePos = new Vector3
            (
                LaunchPos.position.x + Direction.x * Speed * Ti,
                0,
                LaunchPos.position.z + Direction.z * Speed * Ti
            );
            HitIndicator.position = futurePos;

            //draw line
            for (int i = 0; i < 10; i++)
            {

                //float time = Ti / (float)i;
                //Vector3 post = LaunchPos.position + Direction * (Speed * time + (_Gravity * (time * time)) / 2f);
                Vector3 post = calcposintime(Direction * Speed, i / 10f);
                LineRender.SetPosition(i, post);
            }


        }
    }




    Vector3 calcposintime(Vector3 vel, float time)
    {
        Vector3 Vxz = vel;
        Vxz.y = 0f;

        Vector3 res = LaunchPos.position + vel * time;
        float sy = _Gravity * (time * time) + (vel.y * time) + LaunchPos.position.y;
        res.y = sy;

        return res;
    }
}
