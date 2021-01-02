using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProjectileLauncher : MonoBehaviour
{
    //All physics related formula's are based on the book "AI For Games", by Ian Millington (Paragraph 3.5 Physics Prediction)
    public Vector3 Direction;
    public Transform LaunchPos;
    public float Speed;
    public Projectile ProjectilePrefab;
    public Transform HitIndicator;
    public LineRenderer LineRender;
    public Runner _runner;

    public GameObject Ball;

    private float _Gravity;
    private Vector3 _GravityVector;
    public int lineSegments;

    private Scene _testingScene;
    private PhysicsScene _testingPhysicsScene;
    private PhysicsScene _currentPhysicsScene;


    public GameObject obstacles;
    List<GameObject> dummyObstacles = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        Physics.autoSimulation = false;

        Direction = Direction.normalized;
        _Gravity = -9.81f;
        _GravityVector = new Vector3(0f, -9.81f, 0f);

        transform.rotation = Quaternion.LookRotation(Direction);
        LineRender.positionCount = lineSegments + 1;


        //
        _currentPhysicsScene = SceneManager.GetActiveScene().GetPhysicsScene();
        CreateSceneParameters p = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
        _testingScene = SceneManager.CreateScene("Prediction", p);
        _testingPhysicsScene = _testingScene.GetPhysicsScene();


        foreach (Transform t in obstacles.transform)
        {
            if (t.gameObject.GetComponent<Collider>() != null)
            {
                GameObject fakeT = Instantiate(t.gameObject);
                fakeT.transform.position = t.position;
                fakeT.transform.rotation = t.rotation;
                Renderer fakeR = fakeT.GetComponent<Renderer>();
                if (fakeR)
                {
                    fakeR.enabled = false;
                }
                SceneManager.MoveGameObjectToScene(fakeT, _testingScene);
                dummyObstacles.Add(fakeT);
            }
        }

    }

    private void FixedUpdate()
    {
        if (_currentPhysicsScene.IsValid())
            _currentPhysicsScene.Simulate(Time.fixedDeltaTime);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Speed += 0.5f;
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Speed -= 0.5f;
        }

        //Update Launcher
        //Figure out when grenade will land
        //Solving for time Ti 
        float plus = (-Direction.y * Speed + Mathf.Sqrt((Direction.y * Direction.y) * (Speed * Speed) - 2 * _Gravity * (LaunchPos.position.y - 0))) / _Gravity;
        float min = (-Direction.y * Speed - Mathf.Sqrt((Direction.y * Direction.y) * (Speed * Speed) - 2 * _Gravity * (LaunchPos.position.y - 0))) / _Gravity;
        float Ti = Mathf.Max(min, plus);

        //future pos
        Vector3 futurePos = new Vector3
        (
            LaunchPos.position.x + Direction.x * Speed * Ti,
            0.1f,
            LaunchPos.position.z + Direction.z * Speed * Ti
        );
        HitIndicator.position = futurePos;

        if (_currentPhysicsScene.IsValid() && _testingPhysicsScene.IsValid())
        {


            //prediction in test scene
            GameObject testProj = Instantiate(Ball);
            //GameObject plane = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Plane));
            //plane.transform.localScale = new Vector3(14, 0, 14);

            //testProj.AddComponent<Rigidbody>();
            SceneManager.MoveGameObjectToScene(testProj, _testingScene);
            //SceneManager.MoveGameObjectToScene(plane, _testingScene);
            testProj.transform.position = LaunchPos.position;
            testProj.GetComponent<Rigidbody>().velocity = Direction * Speed;

            LineRender.positionCount = 150;
            //new drawing line
            for (int i = 0; i < 150; i++)
            {
                _testingPhysicsScene.Simulate(Time.fixedDeltaTime);
                LineRender.SetPosition(i, testProj.transform.position);
            }

            Destroy(testProj);
        }


        ////draw line
        //float timeSegment = Ti / (float)lineSegments;
        //LineRender.SetPosition(0, LaunchPos.position);
        //for (int i = 0; i < lineSegments; i++)
        //{
        //    if (i == 0)
        //        continue;

        //    float time = i * timeSegment;
        //    Vector3 postAtTime = LaunchPos.position + Direction * Speed * time + _GravityVector * (time * time) / 2f;
        //    LineRender.SetPosition(i, postAtTime);
        //}
        //LineRender.SetPosition(lineSegments, futurePos);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //launch projectile
            Projectile p = Instantiate(ProjectilePrefab);
            p.transform.position = LaunchPos.position;
            p.GetComponent<Rigidbody>().velocity = Direction * Speed;
            p.MaxLifeTime = Ti * 2;


            //Set runner ready
            _runner.LaunchPos = LaunchPos.position;
            _runner.SetTargetToCatch(ref p);
            _runner.StartRunning();
        }
    }
}
