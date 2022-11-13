using System;
using System.Collections.Generic;
using UnityEngine;

namespace Simulation
{
    public class VisTextures : MonoBehaviour
    {
        public Material targetMat;
        private Simulation sim;

        public void Start()
        {
            targetMat = GetComponent<MeshRenderer>().material;
            sim ??= FindObjectOfType<Simulation>();
            targetMat.SetTexture("_Population", sim.GetTexture(Influence.Population));
            targetMat.SetTexture("_Spirit", sim.GetTexture(Influence.Spirit));
        }
    }
}