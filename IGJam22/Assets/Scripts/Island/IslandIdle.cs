using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class IslandIdle : MonoBehaviour
{
    private IslandIdleDirection direction;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.rotation.eulerAngles.magnitude > 5f)
        {

        }
        switch (direction)
        {
            case IslandIdleDirection.North:
                gameObject.transform.Rotate(Vector3.right);
                break;
            case IslandIdleDirection.South:
                gameObject.transform.Rotate(-Vector3.right);
                break;
            case IslandIdleDirection.West:
                gameObject.transform.Rotate(Vector3.forward);
                break;
            case IslandIdleDirection.East:
                gameObject.transform.Rotate(-Vector3.forward);
                break;
            default:
                break;
        }


    }

    private enum IslandIdleDirection
    {
        North,
        South,
        West,
        East
    }
}


