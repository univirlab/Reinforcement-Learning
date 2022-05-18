using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public GameObject targetToRotateAround;
    public float speed = 20;

    public void UpdateSpeed()
    {
        speed = Random.Range(15, 25);
    }
    
    private void Update()
    {
        transform.RotateAround(targetToRotateAround.transform.position, Vector3.up, speed * Time.deltaTime);
    }
}
