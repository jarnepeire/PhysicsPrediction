using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectProjectile : MonoBehaviour
{
    public float ReflectForce = 1f;

    private void OnCollisionEnter(Collision collision)
    {
        GameObject go = collision.gameObject;
        if (go.CompareTag("Projectile"))
        {
            //Create reflect vector (simple idea to send projectile in reflected direction)
            Vector3 reflect = Vector3.Reflect(go.transform.forward, collision.GetContact(0).normal).normalized;
            go.GetComponent<Rigidbody>().AddForce(reflect * ReflectForce, ForceMode.Force);
        }
    }
}
