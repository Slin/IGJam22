using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandBalance : MonoBehaviour
{
    public Vector3 IslandAngle;
    public float Speed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation , Quaternion.Euler(IslandAngle), Time.deltaTime*Speed); 
    }
}
