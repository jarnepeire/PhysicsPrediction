using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProjectileLauncher : MonoBehaviour
{
    //All physics related formula's are based on the book "AI For Games", by Ian Millington (Paragraph 3.5 Physics Prediction)
    [Header("General Variables")]
    public Transform LaunchPos;
    public Projectile ProjectilePrefab;
    public float ProjectileLifetimeMultiplier = 2f;

    [Header("Render Variables")]
    public PhysicsPredicter predicter;
    public bool UseHitIndicator = true;
    public Transform HitIndicator;

    //Private Variables
    private float _speed;
    private Vector3 _direction = Vector3.forward;
    private Vector3 _gravityVector;
    private float _totalTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        _gravityVector = new Vector3(0f, -9.81f, 0f);
        transform.rotation = Quaternion.LookRotation(_direction);
        predicter.SetPhysicsSettings(LaunchPos, _direction, _speed, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        //Figure out when projectile will land (time to land)
        //Solving for time (_totalTime)
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
        predicter.SetPhysicsSettings(LaunchPos, _direction, _speed, _totalTime);

        //Since time is calculated in here, we're going to grab the UI from the controller and set the setting here
        GetComponent<ProjectileLauncherController>().Display.SetTimeToLand(_totalTime);
    }

    public void SetDirection(Vector3 dir)
    {
        _direction = dir;
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
        p.GetComponent<Rigidbody>().velocity = _direction * _speed;
        p.MaxLifeTime = _totalTime * ProjectileLifetimeMultiplier;
    }
}
