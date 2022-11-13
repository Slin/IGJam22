using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class IslandIdle : MonoBehaviour
{
    public float floatingAngle = 10;
    public float speed = 10;

    private Quaternion startRotation;
    private Quaternion endRotation;
    private float time;
    private bool backRotation = false;
    
    // Start is called before the first frame update
    void Start()
    {
        startRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        if (!backRotation)
        {
            time += Time.deltaTime*speed;
        }
        else
        {
            time -= Time.deltaTime*speed;
        }
        if (time != 0)
            transform.localRotation = Quaternion.Slerp(startRotation, endRotation, time);
        if (time >= 1)
        {
            backRotation = true;
        }
        if (backRotation && time <= 0)
        {
            backRotation = false;
            endRotation = Quaternion.AngleAxis(floatingAngle, new Vector3(Random.value*2-1,0,Random.value*2-1).normalized);
        }
    }
}


