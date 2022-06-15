using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class TableRandomizerInCircle : TablePositionRandomizer
{
    [SerializeField] private Transform circleCenterPoint;
    public float radius = .41f;

    [ContextMenu("Move")]
    public override void Move()
    {
        transform.position = circleCenterPoint.position + Random.onUnitSphere * radius;

        while (transform.position.y < .4f)// || transform.position.x < 0.03f)
        {
            transform.position = circleCenterPoint.position + Random.onUnitSphere * radius;
        }

        // reset velocity
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }
    
    Vector3 Spherical(float theta, float phi) => Quaternion.Euler(0f, theta, phi) * Vector3.forward;
}