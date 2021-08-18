using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    private Transform _transform;
    public float MovementSpeed = 5f;

    void Start()
    {
        _transform = GetComponent<Transform>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            _transform.Translate(MovementSpeed * Time.deltaTime* Vector3.left);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            _transform.Translate(MovementSpeed * Time.deltaTime * Vector3.right);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            _transform.Translate(MovementSpeed * Time.deltaTime * Vector3.down);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            _transform.Translate(MovementSpeed * Time.deltaTime * Vector3.up);
        }
    }
}
