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

    public GameObject settlerPrefab;

    public GameObject tentPrefab;
    public GameObject housePrefab;
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

                if(value > 20.0f)
                {
                    newState = PopulationState.Settler;
                }
                if(value > 50.0f)
                {
                    newState = PopulationState.Settler2;
                }
                if(value > 100.0f)
                {
                    newState = PopulationState.Tent;
                }
                if(value > 1000.0f)
                {
                    newState = PopulationState.House;
                }
                if(value > 10000.0f)
                {
                    newState = PopulationState.Skyscraper;
                }

                if(newState > oldState)
                {
                    if(currentCells[index].houseInstance && (newState == PopulationState.Skyscraper || newState == PopulationState.House))
                    {
                        Destroy(currentCells[index].houseInstance);
                        currentCells[index].houseInstance = null;
                    }

                    if(newState == PopulationState.Settler || newState == PopulationState.Settler2)
                    {
                        GameObject settlerInstance = Instantiate(settlerPrefab);
                        settlerInstance.transform.parent = transform;
                        settlerInstance.transform.localPosition = new Vector3(x*15.0f + Random.Range(0.0f, 10.0f), 200, y*15.0f + Random.Range(0.0f, 10.0f));
                        settlerInstance.transform.localPosition += new Vector3(20.0f, 0.0f, 40.0f); //Additional offset to have everything on the island
                        settlerInstance.transform.localRotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
                        RaycastHit hit;
                        if(Physics.Raycast(settlerInstance.transform.position, -settlerInstance.transform.up, out hit))
                        {
                            settlerInstance.transform.position -= settlerInstance.transform.up * hit.distance;
                        }
                    }
                    else if(newState == PopulationState.Tent)
                    {
                        currentCells[index].houseInstance = Instantiate(tentPrefab);
                    }
                    else if(newState == PopulationState.House)
                    {
                        currentCells[index].houseInstance = Instantiate(housePrefab);
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
                        houseInstance.transform.localPosition += new Vector3(20.0f, 0.0f, 40.0f); //Additional offset to have everything on the island
                        houseInstance.transform.localRotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
                        RaycastHit hit;
                        if(Physics.Raycast(houseInstance.transform.position, -houseInstance.transform.up, out hit))
                        {
                            houseInstance.transform.position -= houseInstance.transform.up * hit.distance;
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
