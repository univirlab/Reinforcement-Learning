using UnityEngine;
using Random = UnityEngine.Random;

public class RandomizerInCircle : TablePositionRandomizer
{
    [SerializeField] private Transform circleCenterPoint;
    public float radius = .41f;

    [ContextMenu("Move")]
    public override void Move()
    {
        var vector2 = new Vector2(circleCenterPoint.position.x, circleCenterPoint.position.z) +
                      Random.insideUnitCircle * radius;
        transform.position = new Vector3(vector2.x, 0, vector2.y);

        // while (transform.position.y < .4f)// || transform.position.x < 0.03f)
        // {
        //     transform.position = circleCenterPoint.position + Random.onUnitSphere * radius;
        // }

        // reset velocity
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }
    
    Vector3 Spherical(float theta, float phi) => Quaternion.Euler(0f, theta, phi) * Vector3.forward;
}