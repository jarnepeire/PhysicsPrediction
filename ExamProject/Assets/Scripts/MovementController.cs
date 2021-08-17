using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    private Transform _transform;
    public float MovementSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        _transform = GetComponent<Transform>();
    }

    // Update is called once per frame
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
