using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LauncherDisplay : MonoBehaviour
{
    //Display settings for the UI
    private float _speed;
    public Text SpeedText;

    private float _timeToLand;
    public Text TimeToLandText;

    private Vector3 _dir;
    public Text DirText;

    public void SetSpeed(float speed)
    {
        _speed = speed;
        SpeedText.text = _speed.ToString();
    }

    public void SetTimeToLand(float time)
    {
        _timeToLand = time;
        TimeToLandText.text =  _timeToLand.ToString("F2") + "s";
    }

    public void SetDir(Vector3 dir)
    {
        _dir = dir;
        DirText.text = "X: " + _dir.x.ToString("F1") + ", Y: " + _dir.y.ToString("F1") + ", Z: " + _dir.z.ToString("F1");
    }
}
