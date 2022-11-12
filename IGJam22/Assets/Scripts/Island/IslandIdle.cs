using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class IslandIdle : MonoBehaviour
{
    private IslandIdleDirection direction;
    private Vector3 startRotation;
    private bool backRotation = false;
    // Start is called before the first frame update
    void Start()
    {
        startRotation = transform.up;
    }

    // Update is called once per frame
    void Update()
    {
        var test = Mathf.Abs(Vector3.Angle(startRotation, gameObject.transform.up));
        if (backRotation && test <= 2)
        {
            backRotation = false;
        }
        else if (Vector3.Angle(startRotation , gameObject.transform.up) > 5 && !backRotation)
        {
            backRotation = true;
            int randomInt = Random.Range(0, 3);

            switch (randomInt)
            {
                case 0:
                    direction = IslandIdleDirection.North;
                    break;
                case 1:
                    direction = IslandIdleDirection.South;
                    break;
                case 2:
                    direction = IslandIdleDirection.West;
                    break;
                case 3:
                    direction = IslandIdleDirection.East;
                    break;
                default:
                    break;
            }


        }

        if (backRotation == false)
        {
            Vector3 tempRotation = Vector3.zero;
            switch (direction)
            {
                case IslandIdleDirection.North:
                    tempRotation = Vector3.right * Time.deltaTime;
                    break;
                case IslandIdleDirection.South:
                    tempRotation = -Vector3.right * Time.deltaTime;
                    break;
                case IslandIdleDirection.West:
                    tempRotation = Vector3.forward * Time.deltaTime;
                    break;
                case IslandIdleDirection.East:
                    tempRotation = -Vector3.forward * Time.deltaTime;
                    break;
                default:
                    break;
            }
            gameObject.transform.Rotate(tempRotation * 10, Space.World);
        }
        else
        {
            Vector3 tempRotation = Vector3.zero;
            switch (direction)
            {
                case IslandIdleDirection.North:
                    tempRotation = -Vector3.right * Time.deltaTime;
                    break;
                case IslandIdleDirection.South:
                    tempRotation = Vector3.right * Time.deltaTime;
                    break;
                case IslandIdleDirection.West:
                    tempRotation = -Vector3.forward * Time.deltaTime;
                    break;
                case IslandIdleDirection.East:
                    tempRotation = Vector3.forward * Time.deltaTime;
                    break;
                default:
                    break;
            }
            gameObject.transform.Rotate(tempRotation * 10, Space.World);
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


