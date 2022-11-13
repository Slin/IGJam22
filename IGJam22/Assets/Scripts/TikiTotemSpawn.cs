using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TikiTotemSpawn : MonoBehaviour
{
    public PlayerCamera playerCamera;
    public bool isTotem = false;
    private float fallSpeed = 500.0f;
    private bool isFalling = true;
    private Simulation.ISimulation simulation;

    // Start is called before the first frame update
    void Start()
    {
        simulation = FindObjectOfType<Simulation.Simulation>();
        transform.eulerAngles = new Vector3(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if(isFalling)
        {
            fallSpeed += 9.81f * Time.deltaTime;
            float fallDistance = fallSpeed * Time.deltaTime;

            RaycastHit hit;
            bool didHit = Physics.Raycast(transform.position, -Vector3.up, out hit);
            if(didHit && hit.distance <= fallDistance)
            {
                transform.position -= Vector3.up * hit.distance;
                transform.parent = hit.collider.gameObject.transform.parent;
                playerCamera.DoTotemShake();
                isFalling = false;

                Vector3 simulationPosition = transform.localPosition - new Vector3(9.0f, 0.0f, 21.0f);
                simulationPosition.y = 0.0f;
                simulationPosition.x /= 18.0f / 5.0f;
                simulationPosition.z /= 18.0f / 5.0f;

                if(!isTotem)
                {
                    simulation.SetValue(Simulation.Influence.Spirit, (int)simulationPosition.x, (int)simulationPosition.z, 10000.0f, 5.0f);
                }
                else
                {
                    simulation.SetValue(Simulation.Influence.Spirit, (int)simulationPosition.x, (int)simulationPosition.z, 20000.0f, 8.0f);
                }
            }
            else if(transform.position.y < 0.0f)
            {
                Destroy(gameObject);
            }
            else
            {
                Vector3 position = transform.position;
                position.y -= fallDistance;
                transform.position = position;
            }
        }
    }
}
