using UnityEditor;
using UnityEngine;

public class TwoArmsTablePositionRandomizer : TablePositionRandomizer
{
    [ContextMenu("Move")]
    public override void Move()
    {
        Vector2 tableTopPoint = RandomReachablePointOnTable();
        Vector3 tableCenter = tableBounds.center;
        float x = tableCenter.x + tableTopPoint.x;
        float z = tableCenter.z + tableTopPoint.y;
        
        transform.position = new Vector3(x, targetY, z);

        while (transform.position.z < 0)
        {
            print("z change " + transform.position.z);
            tableTopPoint = RandomReachablePointOnTable();
            tableCenter = tableBounds.center;
            x = tableCenter.x + tableTopPoint.x;
            z = tableCenter.z + tableTopPoint.y;
            transform.position = new Vector3(x, targetY, z);
        }
        
        // random rotation
        Vector3 randomRotation = new Vector3(
            transform.rotation.eulerAngles.x,
            Random.value * 360.0f,
            transform.rotation.eulerAngles.z);
        transform.rotation = Quaternion.Euler(randomRotation);

        // reset velocity
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }
}