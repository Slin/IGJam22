using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandShaderAssigner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Simulation.Simulation simulation = FindObjectOfType<Simulation.Simulation>();
        Material material = GetComponent<Renderer>().material;
        material.SetTexture("_Blend", simulation.GetTexture(Simulation.Influence.Spirit));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
