﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/* General Concept
 *  Simulate the trajection of the projectile upon launch with its current direction and speed and visualize this through the line renderer
 *  
 * Two Ways:
 * (1) The Custom Way:
 * Based upon real physics formula's (ref: AI For Games, Ian Millington) we will simulate our own trajectory of where our ball be in time
 * We can define a few variables to get a smooth trajectory along with our calculated physics variables from the projectile launcher
 * 
 * (2) The Unity Way:
 * We can create a scene by code, run a simulation of this and track the ball's position to determine its trajectory
 * This way, all the math and physics is being left to Unity's built in system, we simply ask for the position in time
 * 
 */

public class PhysicsPredicter : MonoBehaviour
{
    //Global Variables
    [Header("General Variables")]
    public bool UseCustomPhysicsPrediction = true;
    public bool UsingDrag = false;
    public GameObject Projectile;
    public List<GameObject> SceneObjectsToSimulate;

    [Header("Visualization Variables")]
    public LineRenderer LineRender;
    public Material LineMaterial;
    public int CustomLineSegments = 50;
    
    //Private Variables
    private Scene _testingScene;
    private PhysicsScene _testingPhysicsScene;
    private PhysicsScene _currentPhysicsScene;
    private Transform _launchPos;
    private Vector3 _direction;
    private float _speed;
    private float _totalTimeToLand;
    private Vector3 _gravityVector = new Vector3(0f, -9.81f, 0f);
    private int _lineSegmentsSimulation = 175;
    private float _k;
    private float _c;

    private Color _lrColorOrange = new Color(229f / 255f, 146f / 255f, 61f / 255f);
    private Color _lrColorWhite = new Color(1f, 1f, 1f);

    void Start()
    {
        //Since we will have our custom physics system and Unity's as one, we need to manually tell which scene to simulate and when
        //Therefore, auto simulation needs to be turned off
        Physics.autoSimulation = false;

        //Gather our current physics scene -> we will be simulating this one in a fixed update
        _currentPhysicsScene = SceneManager.GetActiveScene().GetPhysicsScene();

        //Create our testing scene for Unity's physics system
        CreateSceneParameters par = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
        _testingScene = SceneManager.CreateScene("TestingScene", par);
        _testingPhysicsScene = _testingScene.GetPhysicsScene();

        //We need to copy over our scene objects to our test scene, to correctly simulate the case of collisions and other factors too
        foreach (GameObject go in SceneObjectsToSimulate)
        {
            //Create a copy of the game object in our original scene
            Transform tempTransform = go.transform;
            GameObject copy = Instantiate(go);
            copy.transform.SetPositionAndRotation(tempTransform.position, tempTransform.rotation);
            copy.transform.localScale = tempTransform.localScale;

            //If the object has a renderer, we don't want it to render in our actual scene
            Renderer tempRenderer = go.GetComponent<Renderer>();
            if (tempRenderer)
            {
                tempRenderer.enabled = false;
            }

            //Finally move the object to the test scene
            SceneManager.MoveGameObjectToScene(copy, _testingScene);
        }

        //If no line renderer is set, try and gather a line renderer from this object
        if (!LineRender)
        {
            LineRender = GetComponent<LineRenderer>();
            if (!LineRender)
                Debug.Log("No LineRenderer was assigned!");
        }
    }

    private void FixedUpdate()
    {
        _currentPhysicsScene.Simulate(Time.fixedDeltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        //Raw input to swap physics prediction line
        if (Input.GetKeyDown(KeyCode.C))
        {
            UseCustomPhysicsPrediction = !UseCustomPhysicsPrediction;
        }

        //Visualize trajectory
        if (UseCustomPhysicsPrediction)
        {
            VisualizeCustomTrajectory();
        }
        else
        {
            VisualizeUnityTrajectory();
        }
    }

    private void VisualizeCustomTrajectory()
    {
        //(1) The Custom Way:
        //Calculate the positions at certain times in the future
        //To determine at which time, we check how many line segments we want in total -> divide total time / segments
        float timeSegment = _totalTimeToLand / (float)CustomLineSegments;

        //Set color of trajectory 
        if (UsingDrag)
            LineMaterial.SetColor("_Color", _lrColorWhite);
        else
            LineMaterial.SetColor("_Color", _lrColorOrange);

        //Set position count
        LineRender.positionCount = CustomLineSegments + 1;

        //Set start position at launch 
        LineRender.SetPosition(0, _launchPos.position);

        //Set all next positions to pre-calculated positions in the near future
        for (int i = 0; i < CustomLineSegments; i++)
        {
            if (i == 0)
                continue;

            float time = i * timeSegment;
            Vector3 postAtTime = (UsingDrag) ? CalculatePositionWithDragForce(time) : CalculatePositionWithoutDragForce(time);
            LineRender.SetPosition(i, postAtTime);
        }

        //Set last position to the ultimate landing position
        Vector3 futurePos = (UsingDrag) ? CalculatePositionWithDragForce(_totalTimeToLand) : CalculatePositionWithoutDragForce(_totalTimeToLand);
        LineRender.SetPosition(CustomLineSegments, futurePos);
    }

    private void VisualizeUnityTrajectory()
    {
        //(2) The Unity Way:
        if (_currentPhysicsScene.IsValid() && _testingPhysicsScene.IsValid())
        {
            //Simulate the physics in test scene as if we were to launch the projectile now
            GameObject testProj = Instantiate(Projectile);
            SceneManager.MoveGameObjectToScene(testProj, _testingScene);
            testProj.transform.position = _launchPos.position;
            testProj.GetComponent<Rigidbody>().velocity = _direction * _speed;
            testProj.GetComponent<Rigidbody>().drag = _k;

            //Set color of trajectory 
            if (UsingDrag)
                LineMaterial.SetColor("_Color", _lrColorWhite);
            else
                LineMaterial.SetColor("_Color", _lrColorOrange);

            //Simulate the scene, every "fixedDeltaTime" seconds track its position and store in line renderer to visualize
            LineRender.positionCount = _lineSegmentsSimulation;
            for (int i = 0; i < _lineSegmentsSimulation; i++)
            {
                _testingPhysicsScene.Simulate(Time.fixedDeltaTime);
                LineRender.SetPosition(i, testProj.transform.position);
            }

            //Get rid of test projectile, we don't want our memory to be flooded after running it for too long
            Destroy(testProj);
        }
    }

    Vector3 CalculatePositionWithoutDragForce(float t)
    {
        //Solving for Pt = p0 + u*s*t + (gt^2)/2
        return _launchPos.position + _direction * (_speed * t) + _gravityVector * (t * t) / 2f;
    }

    Vector3 CalculatePositionWithDragForce(float t)
    {
        //EulerNumber
        float e = 2.718281828459f;

        //Solving the equation: Pt = g - kPt (simplified version for taking drag into account)
        //We can solve the following: Pt = (gt - Ae^-kt) / k + B
        //With A and B being constants found from the position and velocity of the particle at t = 0
        Vector3 A = _speed * _direction - (_gravityVector / _k);
        Vector3 B = _launchPos.position + (A / _k);

        //Position in time with drag force applied
        Vector3 Pt = ((_gravityVector * t - A * Mathf.Pow(e, -_k * t)) / _k) + B;

        return Pt;
    }

    public void SetPhysicsSettings(Transform launchPos, Vector3 dir, float speed, float totalTime, bool usingDrag = false, float k = 0, float c = 0)
    {
        _launchPos = launchPos;
        _direction = dir;
        _speed = speed;
        _totalTimeToLand = totalTime;
        _k = k;
        _c = c;
        UsingDrag = usingDrag;
    }
}