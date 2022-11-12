using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TikiSettlers : MonoBehaviour
{
    public enum PopulationState
    {
        None,
        Settler,
        Settler2,
        Tent,
        House,
        Skyscraper
    }

    public struct Cell
    {
        public PopulationState populationState;
        public GameObject houseInstance;
    }

    public GameObject tentPrefab;
    public GameObject skyscraperPrefab;

    private Cell[] currentCells;
    private Simulation.ISimulation simulation;

    // Start is called before the first frame update
    void Start()
    {
        simulation = GetComponent<Simulation.Simulation>();
        currentCells = new Cell[400];
        for(int i = 0; i < 400; i++)
        {
            currentCells[i].populationState = PopulationState.None;
            currentCells[i].houseInstance = null;
        }
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
                PopulationState oldState = currentCells[index].populationState;

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
                    if(currentCells[index].houseInstance && newState == PopulationState.Skyscraper)
                    {
                        Destroy(currentCells[index].houseInstance);
                        currentCells[index].houseInstance = null;
                    }

                    if(newState == PopulationState.Tent)
                    {
                        currentCells[index].houseInstance = Instantiate(tentPrefab);
                    }
                    else if(newState == PopulationState.Skyscraper)
                    {
                        currentCells[index].houseInstance = Instantiate(skyscraperPrefab);
                    }

                    if(currentCells[index].houseInstance)
                    {
                        GameObject houseInstance = currentCells[index].houseInstance;
                        houseInstance.transform.parent = transform;
                        houseInstance.transform.localPosition = new Vector3(x*15.0f + Random.Range(0.0f, 10.0f), 200, y*15.0f + Random.Range(0.0f, 10.0f));
                        houseInstance.transform.localRotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
                        RaycastHit hit;
                        if(Physics.Raycast(houseInstance.transform.position, -houseInstance.transform.up, out hit))
                        {
                            houseInstance.transform.localPosition -= houseInstance.transform.up * hit.distance;
                            currentCells[index].houseInstance = houseInstance;
                        }
                        else
                        {
                            Destroy(houseInstance);
                            currentCells[index].houseInstance = null;
                        }
                    }

                    currentCells[index].populationState = newState;
                }
            }
        }
    }
}
