using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TikiSettlers : MonoBehaviour
{
    public GameObject tentPrefab;

    private enum PopulationState
    {
        None,
        Settler,
        Settler2,
        Tent,
        House,
        Skyscraper
    }

    private PopulationState[] currentPopulation;
    private Simulation.ISimulation simulation;

    // Start is called before the first frame update
    void Start()
    {
        simulation = GetComponent<Simulation.Simulation>();
        currentPopulation = new PopulationState[400];
    }

    // Update is called once per frame
    void Update()
    {
        for(int x = -10; x < 10; x++)
        {
            for(int y = -10; y < 10; y++)
            {

                float value = 0.0f;
                for(int simulationX = x * 5; simulationX < x * 5 + 5; simulationX++)
                {
                    for(int simulationY = y * 5; simulationY < y * 5 + 5; simulationY++)
                    {
                        float fieldValue;
                        if(simulation.GetValue(Simulation.Influence.Population, simulationX, simulationY, out fieldValue))
                        {
                            value += fieldValue;
                        }
                    }
                }

                value /= 25.0f;

                int index = (y + 10) * 20 + (x + 10);
                PopulationState newState = PopulationState.None;
                PopulationState oldState = currentPopulation[index];

                if(value > 2.0f)
                {
                    newState = PopulationState.Settler;
                }
                if(value > 8.0f)
                {
                    newState = PopulationState.Settler2;
                }
                if(value > 15.0f)
                {
                    newState = PopulationState.Tent;
                }
                if(value > 50.0f)
                {
                    newState = PopulationState.House;
                }
                if(value > 150.0f)
                {
                    newState = PopulationState.Skyscraper;
                }

                if(newState > oldState)
                {
                    if(newState == PopulationState.Tent)
                    {
                        GameObject tentInstance = Instantiate(tentPrefab);
                        tentInstance.transform.parent = transform;
                        tentInstance.transform.localPosition = new Vector3(x*15.0f + Random.Range(0.0f, 10.0f), 200, y*15.0f + Random.Range(0.0f, 10.0f));
                        tentInstance.transform.localRotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
                        RaycastHit hit;
                        if(Physics.Raycast(tentInstance.transform.position, -transform.up, out hit))
                        {
                            tentInstance.transform.localPosition -= transform.up * hit.distance;
                        }
                        else
                        {
                            Destroy(tentInstance);
                        }
                    }
                    currentPopulation[index] = newState;
                }
            }
        }
    }
}
