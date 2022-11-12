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
        if (backRotation && Vector3.Angle(startRotation, gameObject.transform.up) == 0)
        {
            backRotation = false;
        }
        if (Vector3.Angle(startRotation , gameObject.transform.up) > 5 && !backRotation)
        {
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
            switch (direction)
            {
                case IslandIdleDirection.North:
                    gameObject.transform.Rotate(Vector3.right * Time.deltaTime, Space.World);
                    break;
                case IslandIdleDirection.South:
                    gameObject.transform.Rotate(-Vector3.right * Time.deltaTime, Space.World);
                    break;
                case IslandIdleDirection.West:
                    gameObject.transform.Rotate(Vector3.forward * Time.deltaTime, Space.World);
                    break;
                case IslandIdleDirection.East:
                    gameObject.transform.Rotate(-Vector3.forward * Time.deltaTime, Space.World);
                    break;
                default:
                    break;
            }
        }
        else
        {
            gameObject.transform.Rotate(Vector3.up * Time.deltaTime, Space.World);
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


