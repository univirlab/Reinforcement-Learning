using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGetDir : MonoBehaviour
{
    [SerializeField] private Transform p11;
    [SerializeField] private Transform p12;
    [SerializeField] private Transform p21;
    [SerializeField] private Transform p22;
    [SerializeField] private Transform p31;
    [SerializeField] private Transform p32;
    [SerializeField] private Transform p41;
    [SerializeField] private Transform p42;
    [SerializeField] private Transform p51;
    [SerializeField] private Transform p52;
    [SerializeField] private Transform p61;
    [SerializeField] private Transform p62;    
    [SerializeField] private Transform p71;
    [SerializeField] private Transform p72;

    void Start()
    {
        print("1: " + Vector3.Distance(p11.position, p12.position));
        print("2: " + Vector3.Distance(p21.position, p22.position));
        print("3: " + Vector3.Distance(p31.position, p32.position));
        print("4: " + Vector3.Distance(p41.position, p42.position));
        print("5: " + Vector3.Distance(p51.position, p52.position));
        print("6: " + Vector3.Distance(p61.position, p62.position));
        print("7: " + Vector3.Distance(p71.position, p72.position));
    }
}
