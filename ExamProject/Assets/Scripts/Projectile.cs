using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float MaxLifeTime;
    public float _lifeTime;

    bool UseDragForce = false;
    public Vector3 DragForce;
    // Start is called before the first frame update
    void Start()
    {
        _lifeTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (UseDragForce)
        {
            this.GetComponent<Rigidbody>().AddForce(DragForce);
        }
        
        _lifeTime += Time.deltaTime;
        if (_lifeTime > MaxLifeTime)
            Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Runner"))
            Destroy(this.gameObject);
    }
}
