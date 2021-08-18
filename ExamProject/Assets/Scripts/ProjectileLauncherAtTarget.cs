﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * This is one type of launcher in which a target position and speed is given, and a desired direction is calculated
 * All physics related formula's are based on the book "AI For Games", by Ian Millington (Paragraph 3.5 Physics Prediction)
 */

public class ProjectileLauncherAtTarget : BaseLauncher
{
    [Header("Specific Launcher Options")]
    public Transform Target;
    public Material ValidShotMaterial;
    public Material InvalidShotMaterial;

    [Header("Drag Force Options")]
    public GameObject DragIndicator;
    public bool UseDrag = false;
    public bool RefineLauncherToHitTarget = false;
    //Viscous Drag Force;
    public float k;
    //Aerodynamic Drag Force;
    public float c;
    //Amount of units away a drag force prediction can be off
    public float DragMargin = 2f;

    //Variables refining algorithm for drag force adjustments
    private int _refineIterations = 3;
    private int _currentRefineIterations = 0;
    private bool _isRefined = true;
    private float _speedBeforeRefinedDrag;
    private Vector3 _directionBeforeRefinedDrag;
    private Vector3 _dragLandingPos;
    private int _iterationCounter;
    private int _maxIterations;
    private float _startSpeedToRefineFrom = 10f;

    //Struct to store prediction data in
    struct FiringData
    {
        public FiringData(bool isValid, float time, Vector3 dir)
        {
            IsValid = isValid;
            TimeToTarget = time;
            DirToLaunch = dir;
        }

        public bool IsValid;
        public float TimeToTarget;
        public Vector3 DirToLaunch;
    }

    void Start()
    {
        _iterationCounter = 0;
        _maxIterations = 50;

        Predicter.SetPhysicsSettings(LaunchPos, _direction, _speed, 0f);
        DragIndicator.GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<MeshRenderer>().material = ValidShotMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        //Update input
        UpdateInputToggles();
        
        //Decide whether to keep refining or not
        if (_isRefined)
        {
            if (_currentRefineIterations <= 0)
            {
                _isRefined = true;
            }
            else
            {
                --_currentRefineIterations;
                _isRefined = false;
            }
        }

        //Show drag indicator/update direction to calculated drag direction
        if (UseDrag)
        {
            //Calculate and show landing position if drag forces are applied
            _dragLandingPos = CalculatePositionWithDragForce(_totalTime, _direction);
            _dragLandingPos.y = 0.01f;
            DragIndicator.GetComponent<SpriteRenderer>().enabled = true;
            DragIndicator.transform.position = _dragLandingPos;
            
            //Calculate and adjust the launcher to still hit the target through the drag forces
            if (RefineLauncherToHitTarget)
            {
                if (!_isRefined)
                {
                    //Store variables to reset in case refining has no possible result
                    _directionBeforeRefinedDrag = _direction;
                    _speedBeforeRefinedDrag = _speed;

                    //Used to make sure the lowest amount of speed/force is used to predict with
                    //With high amounts of the speed the direction of the trajectory is almost a straight line
                    _speed = _startSpeedToRefineFrom;

                    //Calculate firing data and adjust to launcher
                    bool validUpdate = AdjustLauncherToFiringData();
                    if (!validUpdate)
                        return;

                    //Refine direction and speed to hit target
                    bool success = true;
                    _direction = RefinedLauncherWithDragForce(ref success);
                    if (success)
                    {
                        GetComponent<MeshRenderer>().material = ValidShotMaterial;
                    }
                    else
                    {
                        SetSpeed(_speedBeforeRefinedDrag);
                        GetComponent<MeshRenderer>().material = InvalidShotMaterial;
                    }
                }
            }
            else
            {
                //Calculate firing data and adjust to launcher
                bool validUpdate = AdjustLauncherToFiringData();
                if (!validUpdate)
                    return;
            }
        }
        //No Drag Predictions
        else
        {
            //Calculate firing data and adjust to launcher
            bool validUpdate = AdjustLauncherToFiringData();
            if (!validUpdate)
                return;
        }
        
        //Update visualization of trajectory
        Predicter.SetPhysicsSettings(LaunchPos, _direction, _speed, _totalTime, UseDrag, (UseDrag) ? k : 0f, (UseDrag) ? c : 0f);

        //Since time is calculated in here, we're going to grab the UI from the controller and set the setting here
        GetComponent<ProjectileLauncherController>().Display.SetTimeToLand(_totalTime);

        //Update velocity to launch projectile with
        _velocity = _direction * _speed;
    }

    FiringData CalculateFiringData()
    {
        //Given a target pos, start pos and velocity speed -> we need to solve the direction in which to fire
        //To solve this, we can start by solving for time given our knowns
        Vector3 T = Target.position - LaunchPos.position;
        float dotGT = Vector3.Dot(T, _gravityVector);
        float sqrdSpeed = _speed * _speed;

        //Quadratic equation to solve for two times
        float a = _gravityVector.sqrMagnitude;
        float b = -4 * (dotGT + sqrdSpeed);
        float c = 4 * T.sqrMagnitude;

        //Check if there will be an actual solution given our speed/force factor
        float D = (b * b) - 4 * a * c;
        if (D < 0)
        {
            GetComponent<MeshRenderer>().material = InvalidShotMaterial;
            return new FiringData(false, 0f, Vector3.zero);
        }
        else
        {
            GetComponent<MeshRenderer>().material = ValidShotMaterial;
        }
            
        //Solve for two times
        float time0 = Mathf.Sqrt((-b + Mathf.Sqrt(D)) / (2 * a));
        float time1 = Mathf.Sqrt((-b - Mathf.Sqrt(D)) / (2 * a));

        //Find actual time to target
        float time = 0f;
        if (time0 < 0)
            if (time1 < 0)
                return new FiringData(false, 0f, Vector3.zero);
            else
                time = time1;
        else
            if (time1 < 0)
                time = time0;
            else
                time = Mathf.Min(time0, time1);

        //Return firing direction
        Vector3 dir = (T * 2 - _gravityVector * (time * time)) / (2 * _speed * time);
        FiringData firingData = new FiringData(true, time, dir);
        return firingData;
    }

    Vector3 RefinedLauncherWithDragForce(ref bool success, bool isRecursiveAttempt = false)
    {
        //Only recalculate the refined drag direction when asked for
        if (_isRefined)
            return _direction;

        //Calculate firing solution, and get initial landing position guess
        int sign = (_dragLandingPos.x < 0 || _dragLandingPos.z < 0) ? -1 : 1;
        int signT = (Target.position.x < 0 || Target.position.z < 0) ? -1 : 1;
        float distanceToDragLanding = (_dragLandingPos - LaunchPos.position).magnitude * sign;
        float distanceToTarget = (Target.position - LaunchPos.position).magnitude * signT;
        float distanceDiff = Mathf.Abs(distanceToDragLanding) - Mathf.Abs(distanceToTarget);

        //If our initial guess isn't too far from the landing position with drag -> use initial direction and speed
        if (Mathf.Abs(distanceDiff) < DragMargin + float.Epsilon)
        {
            _isRefined = true;
            _speed = _speedBeforeRefinedDrag;
            return _direction;
        }

        //Binary search to ensure closer trajectory to target
        float minBound = 0;
        float maxBound = 0;
       
        //Forward vector to target
        Vector3 fwdDirection = _direction;
        fwdDirection.y = 0;
        fwdDirection = fwdDirection.normalized;

        //Angle from forward vector to target to the direction vector in radians
        float angle = Vector3.Angle(fwdDirection, _direction) * Mathf.Deg2Rad;
        if (distanceDiff > 0)
        {
            //Maximum bound -> use shortest possible shot as minimum bound
            maxBound = angle;
            minBound = -Mathf.PI / 2f;

            //Create new rotated direction
            Vector3 dirCopy = _direction;
            Vector3 newDir = Quaternion.AngleAxis(minBound * Mathf.Rad2Deg, fwdDirection) * dirCopy;

            //Calculate new landing pos and check if it's close enough in distance to target
            Vector3 landingPos = CalculatePositionWithDragForce(_totalTime, newDir);
            landingPos.y = 0.01f;
            float distanceToLandPos = (landingPos - LaunchPos.position).magnitude * sign;
            float diff = Mathf.Abs(distanceToLandPos) - Mathf.Abs(distanceToTarget);

            //If so, use this as direction
            if (Mathf.Abs(diff) < DragMargin + float.Epsilon)
            {
                _isRefined = true;
                return newDir;
            }   
        }
        else
        {
            bool keepLoop = true;
            while (keepLoop)
            {
                //Need to check for maximum bound here
                minBound = angle;
                maxBound = Mathf.PI / 4f;

                //Create new rotated direction
                Vector3 dirCopy = _direction;
                Vector3 newDir = Quaternion.AngleAxis(maxBound * Mathf.Rad2Deg, fwdDirection) * dirCopy;
                
                //Calculate new landing pos and check if it's close enough in distance to target
                Vector3 landingPos = CalculatePositionWithDragForce(_totalTime, newDir);
                landingPos.y = 0.01f;
                float distanceToLandPos = (landingPos - LaunchPos.position).magnitude * sign;
                float diff = Mathf.Abs(distanceToLandPos) - Mathf.Abs(distanceToTarget);

                //If so, use this as direction
                if (Mathf.Abs(diff) < DragMargin + float.Epsilon)
                {
                    _isRefined = true;
                    return newDir;
                }

                //If not, check if we can get closer to the target with best possible angle we rotated with (45degrees)
                //By increasing the speed 
                if (diff < 0)
                {
                    if (_iterationCounter == 0)
                        SetSpeed(1f);

                    //Need to increase the force in that case, and retry until we get it right
                    SetSpeed(_speed + 1);
                    ++_iterationCounter;
                    if (_iterationCounter < _maxIterations)
                    {
                        continue;
                    }
                    else
                    {
                        //If it seems we can't even get it right even by increasing the force, just reset it and mark as invalid shot
                        _iterationCounter = 0;
                        SetSpeed(_speedBeforeRefinedDrag);
                        _isRefined = true;
                        success = false;
                        return _directionBeforeRefinedDrag;
                    }
                }
                else
                {
                    keepLoop = false;
                }
            }
        }

        //We have a min and max bound -> so search for something inbetween that will fit the margin
        Vector3 closestDir = _direction;
        float rDist = DragMargin * 1000f;

        _iterationCounter = 0;
        while (Mathf.Abs(rDist) < DragMargin + float.Epsilon)
        {
            _iterationCounter++;
            float a = (maxBound - minBound) / 2f;

            //Keep rotating the direction from min- to max-bound
            Vector3 dirCopy = _direction;
            closestDir = Quaternion.AngleAxis(a * Mathf.Rad2Deg, fwdDirection) * dirCopy;

            //Calculate new landing pos and check if it's close enough in distance to target
            Vector3 landingPos = CalculatePositionWithDragForce(_totalTime, closestDir);
            landingPos.y = 0.01f;
            float dist = (landingPos - LaunchPos.position).magnitude * sign;
            rDist = Mathf.Abs(dist) - Mathf.Abs(distanceToTarget);

            //Adjust boundaries of search
            if (rDist < 0)
                minBound = angle;
            else
                maxBound = angle;

            //Stop searching after an amount of tries and mark is invalid
            if (_iterationCounter > _maxIterations)
            {
                success = false;
                _isRefined = true;
                return closestDir.normalized;
            }
        }

        //Otherwise return successful direction
        _isRefined = true;
        return closestDir.normalized;
    }

    Vector3 CalculatePositionWithDragForce(float t, Vector3 dir)
    {
        //EulerNumber
        float e = 2.718281828459f;

        //Solving the equation: Pt = g - kPt (simplified version for taking drag into account)
        //We can solve the following: Pt = (gt - Ae^-kt) / k + B
        //With A and B being constants found from the position and velocity of the particle at t = 0
        Vector3 A = _speed * dir - (_gravityVector / k);
        Vector3 B = LaunchPos.position + (A / k);

        //Position in time with drag force applied
        Vector3 Pt = ((_gravityVector * t - A * Mathf.Pow(e, -k * t)) / k) + B;

        return Pt;
    }

    public override void SetDirection(Vector3 dir)
    {
        //Don't update direction, direction is calculated in this type of launcher
    }

    public override void LaunchProjectile()
    {
        //Launch projectile
        Projectile p = Instantiate(ProjectilePrefab);
        p.transform.position = LaunchPos.position;

        Rigidbody rb = p.GetComponent<Rigidbody>();
        rb.velocity = _velocity;
     
        if (UseDrag)
            rb.drag = k;

        p.MaxLifeTime = _totalTime * ProjectileLifetimeMultiplier;
    }

    bool AdjustLauncherToFiringData()
    {
        //Calculate firing data to reach target
        FiringData firingData = CalculateFiringData();
        if (!firingData.IsValid)
            return false;

        _direction = firingData.DirToLaunch;
        _totalTime = firingData.TimeToTarget;
        return true;
    }
    private void UpdateInputToggles()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            UseDrag = !UseDrag;
            DragIndicator.GetComponent<SpriteRenderer>().enabled = UseDrag;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            RefineLauncherToHitTarget = !RefineLauncherToHitTarget;
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            _isRefined = false;
            _currentRefineIterations = _refineIterations;
        }
    }
}
