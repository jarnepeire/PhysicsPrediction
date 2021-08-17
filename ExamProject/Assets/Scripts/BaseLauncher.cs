using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseLauncher : MonoBehaviour
{
    [Header("Base Launcher Properties")]
    public Projectile ProjectilePrefab;
    public float ProjectileLifetimeMultiplier = 2f;
    public Transform LaunchPos;
    public PhysicsPredicter Predicter;

    //Initial launch variables
    protected float _speed = 5f;
    protected Vector3 _gravityVector = new Vector3(0f, -9.81f, 0f);
    protected Vector3 _direction = Vector3.forward;

    //Calculated launch variables
    protected float _totalTime = 0f;
    protected Vector3 _velocity;

    public virtual void LaunchProjectile() { }

    public virtual void SetSpeed(float speed)
    {
        _speed = speed;
        var controller = GetComponent<ProjectileLauncherController>();
        if (controller)
        {
            controller.Speed = speed;
            controller.Display.SetSpeed(speed);
        }
    }

    public virtual void SetDirection(Vector3 dir)
    {
        _direction = dir;
        var controller = GetComponent<ProjectileLauncherController>();
        if (controller)
        {
            controller.Display.SetDir(dir);
        }
    }

    public Vector3 GetDirection() { return _direction; }
    public float GetSpeed() { return _speed; }

}
