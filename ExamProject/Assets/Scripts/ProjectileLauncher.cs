using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/* 
 * This is one type of launcher in which a direction and speed is given, and a target position is calculated
 * All physics related formula's are based on the book "AI For Games", by Ian Millington (Paragraph 3.5 Physics Prediction)
 * 
 */

public class ProjectileLauncher : BaseLauncher
{
    [Header("Render Variables")]
    public bool UseHitIndicator = true;
    public Transform HitIndicator;

    void Start()
    {
        transform.rotation = Quaternion.LookRotation(_direction);
        Predicter.SetPhysicsSettings(LaunchPos, _direction, _speed, 0f);
    }

    void Update()
    {
        //Figure out when projectile will land (time to land)
        //Solving for time
        float plus = (-_direction.y * _speed + Mathf.Sqrt((_direction.y * _direction.y) * (_speed * _speed) - 2 * _gravityVector.y * (LaunchPos.position.y - 0))) / _gravityVector.y;
        float min = (-_direction.y * _speed - Mathf.Sqrt((_direction.y * _direction.y) * (_speed * _speed) - 2 * _gravityVector.y * (LaunchPos.position.y - 0))) / _gravityVector.y;
        _totalTime = Mathf.Max(min, plus);

        //Calculate the future position (where the projectile will land)
        Vector3 futurePos = new Vector3
        (
            LaunchPos.position.x + _direction.x * _speed * _totalTime,
            0.01f,
            LaunchPos.position.z + _direction.z * _speed * _totalTime
        );

        //Set indicator sprite to that position to visualize
        if (UseHitIndicator)
            HitIndicator.position = futurePos;

        //Set physics settings for our predictor, so he can visualize the trajectory of our projectile
        Predicter.SetPhysicsSettings(LaunchPos, _direction, _speed, _totalTime);

        //We're going to grab the UI from the controller and set the setting here
        GetComponent<ProjectileLauncherController>().Display.SetTimeToLand(_totalTime);
    }

    public override void LaunchProjectile()
    {
        //Launch projectile
        Projectile p = Instantiate(ProjectilePrefab);
        p.transform.position = LaunchPos.position;
        p.GetComponent<Rigidbody>().velocity = _direction * _speed;
        p.MaxLifeTime = _totalTime * ProjectileLifetimeMultiplier;
    }
}
