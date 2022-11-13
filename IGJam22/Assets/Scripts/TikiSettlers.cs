using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TikiSettlers : MonoBehaviour
{
    public enum PopulationState
    {
        None,
        Settler,
        Settler2,
        Tent,
        House,
        Skyscraper,
        Skyscraper2
    }

    public struct Cell
    {
        public PopulationState populationState;
        public GameObject houseInstance;
        public GameObject settler1Instance;
        public GameObject settler2Instance;
    }

    public GameObject settlerPrefab;

    public GameObject tentPrefab;
    public GameObject housePrefab;
    public GameObject skyscraperPrefab;
    public GameObject skyscraper2Prefab;

    public float worshipOMeter = 0.0f;
    public float popOMeter = 0.0f;

    private Cell[] currentCells;
    private Simulation.ISimulation simulation;
    private IslandBalance islandBalance;
    private float loseCounter = 0.0f;
    public bool didLoose = false;
    public bool didWin = false;

    private FMODUnity.StudioEventEmitter musicEmitter;

    // Start is called before the first frame update
    void Start()
    {
        GameObject musicEmitterObject = GameObject.Find("Main_theme");
        musicEmitter = musicEmitterObject.GetComponent<FMODUnity.StudioEventEmitter>();

        simulation = GetComponent<Simulation.Simulation>();
        islandBalance = transform.parent.gameObject.GetComponent<IslandBalance>();
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
        if(didLoose || didWin)
        {
            loseCounter += Time.deltaTime;
            if(loseCounter > 10.0f || Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene("IntroMenu", LoadSceneMode.Single);
            }
            return;
        }

        Vector3 weightVector = new Vector3(0.0f, 0.0f, 0.0f);
        float totalPopulation = 0.0f;

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

                totalPopulation += value;

                weightVector.x += y * Mathf.Min(value, 100.0f);
                weightVector.z += x * Mathf.Min(value, 100.0f);

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
                if(value > 50000.0f)
                {
                    newState = PopulationState.Skyscraper2;
                }

                if(newState != oldState)
                {
                    if(currentCells[index].houseInstance)
                    {
                        Destroy(currentCells[index].houseInstance);
                        currentCells[index].houseInstance = null;
                    }

                    if(currentCells[index].settler1Instance && newState < PopulationState.Settler)
                    {
                        Destroy(currentCells[index].settler1Instance);
                        currentCells[index].settler1Instance = null;
                    }

                    if(currentCells[index].settler2Instance && newState < PopulationState.Settler2)
                    {
                        Destroy(currentCells[index].settler2Instance);
                        currentCells[index].settler2Instance = null;
                    }

                    if(newState > oldState)
                    {
                        if(newState == PopulationState.Settler || newState == PopulationState.Settler2)
                        {
                            GameObject settlerInstance = Instantiate(settlerPrefab);
                            settlerInstance.transform.parent = transform;
                            settlerInstance.transform.localPosition = new Vector3(x*18.0f + Random.Range(0.0f, 15.0f), 200, y*18.0f + Random.Range(0.0f, 15.0f));
                            settlerInstance.transform.localPosition += new Vector3(9.0f, 0.0f, 21.0f); //Additional offset to have everything on the island
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
                        else if(newState == PopulationState.Skyscraper2)
                        {
                            currentCells[index].houseInstance = Instantiate(skyscraper2Prefab);
                        }

                        if(currentCells[index].houseInstance)
                        {
                            GameObject houseInstance = currentCells[index].houseInstance;
                            houseInstance.transform.parent = transform;
                            houseInstance.transform.localPosition = new Vector3(x*18.0f + Random.Range(0.0f, 15.0f), 200, y*18.0f + Random.Range(0.0f, 15.0f));
                            houseInstance.transform.localPosition += new Vector3(9.0f, 0.0f, 21.0f); //Additional offset to have everything on the island
                            houseInstance.transform.localRotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
                            RaycastHit hit;
                            bool didHit = Physics.Raycast(houseInstance.transform.position, -houseInstance.transform.up, out hit);
                            if(didHit)
                            {
                                houseInstance.transform.position -= houseInstance.transform.up * hit.distance;
                                currentCells[index].houseInstance = houseInstance;
                            }

                            if(!didHit || houseInstance.transform.localPosition.y < -30.0f)
                            {
                                Destroy(houseInstance);
                                currentCells[index].houseInstance = null;
                            }
                        }
                    }

                    currentCells[index].populationState = newState;
                }
            }
        }

        islandBalance.IslandAngle.x = Mathf.Clamp(weightVector.x * 0.001f, -15.0f, 15.0f);
        islandBalance.IslandAngle.z = Mathf.Clamp(weightVector.z * 0.001f, -15.0f, 15.0f);

        worshipOMeter += totalPopulation * Time.deltaTime * 0.01f;
        popOMeter = totalPopulation;

        if(weightVector.magnitude > 15000.0f)
        {
            loseCounter += Time.deltaTime;

            if(loseCounter > 15.0f)
            {
                islandBalance.IslandAngle.x = 180.0f;
                islandBalance.IslandAngle.z = 0.0f;
                loseCounter = 0.0f;
                didLoose = true;
            }
        }
        else
        {
            if(loseCounter > 0.0f) loseCounter -= Time.deltaTime;
        }

        if(!didLoose && worshipOMeter > 100000)
        {
            didWin = true;
            loseCounter = 0.0f;
        }

        musicEmitter.SetParameter("panic", loseCounter / 15.0f);
    }
}
